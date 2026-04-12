import 'dart:async';
import 'dart:io';
import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:injectable/injectable.dart';
import 'package:logger/logger.dart';
import '../config/env_config.dart';
import '../constants/api_constants.dart';
import 'network_service.dart';

@lazySingleton
class DioClient {
  static const _wanFailoverKey = '_wan_failover_attempted';

  final Dio _dio;
  final FlutterSecureStorage _storage;
  final Logger _logger;
  final NetworkService _networkService;

  static final _logoutController = StreamController<String>.broadcast();
  Stream<String> get forceLogoutStream => _logoutController.stream;

  DioClient({
    required FlutterSecureStorage storage,
    required Logger logger,
    required NetworkService networkService,
  }) : _storage = storage,
       _logger = logger,
       _networkService = networkService,
       _dio = Dio(
         BaseOptions(
           baseUrl: EnvConfig.network.wanOrigin,
           connectTimeout: const Duration(seconds: 15),
           receiveTimeout: const Duration(seconds: 15),
           headers: {
             'Content-Type': 'application/json',
             'Accept': 'application/json',
           },
         ),
       ) {
    if (EnvConfig.isDevelopment) {
      (_dio.httpClientAdapter as IOHttpClientAdapter).createHttpClient = () {
        final client = HttpClient();
        client.badCertificateCallback = (cert, host, port) => true;
        return client;
      };
    }
    _dio.interceptors.add(
      QueuedInterceptorsWrapper(
        onRequest: (options, handler) async {
          await _networkService.initialize();
          options.baseUrl = _networkService.currentBaseUrl;
          unawaited(
            _networkService.revalidateInBackground(
              reason: 'request ${options.method} ${options.path}',
            ),
          );
          _logger.i(
            'Request: ${options.method} ${options.path} [${options.baseUrl}]',
          );
          try {
            final token = await _storage.read(key: 'auth_token');
            if (token != null) {
              options.headers['Authorization'] = 'Bearer $token';
            }
          } catch (e) {
            _logger.e('Error reading token: $e');
          }
          return handler.next(options);
        },
        onResponse: (response, handler) {
          _logger.i(
            'Response: ${response.statusCode} ${response.statusMessage} '
            '[${response.requestOptions.baseUrl}]',
          );
          return handler.next(response);
        },
        onError: (DioException e, handler) async {
          _logger.e(
            'Error: ${e.response?.statusCode} ${e.requestOptions.method} '
            '${e.requestOptions.path}\nResponse body: ${e.response?.data}',
          );
          if (e.response?.statusCode == 401 &&
              e.requestOptions.extra['_retry'] != true) {
            try {
              final newToken = await _attemptTokenRefresh();
              if (newToken != null) {
                final retryOptions = e.requestOptions;
                retryOptions.headers['Authorization'] = 'Bearer $newToken';
                retryOptions.extra['_retry'] = true;
                final retryResponse = await _dio.fetch(retryOptions);
                return handler.resolve(retryResponse);
              }
            } catch (refreshError) {
              _logger.e('Token refresh failed: $refreshError');
            }
            await _forceLogout(
              'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
            );
            return handler.reject(e);
          }

          if (_shouldRetryOnWan(e)) {
            try {
              final response = await _retryRequestOverWan(e.requestOptions);
              await _networkService.markLanAsUnavailable();
              return handler.resolve(response);
            } catch (retryError) {
              _logger.w('WAN failover retry failed: $retryError');
            }
          }

          return handler.next(e);
        },
      ),
    );
  }

  Future<String?> _attemptTokenRefresh() async {
    final storedRefresh = await _storage.read(key: 'refresh_token');
    if (storedRefresh == null) return null;

    _logger.i('Attempting token refresh...');
    final refreshResponse = await _postWithLanWanFallback(
      path: ApiConstants.refreshToken,
      data: {'refreshToken': storedRefresh},
    );

    final newToken =
        refreshResponse.data['token'] ?? refreshResponse.data['accessToken'];
    if (newToken != null) {
      await _storage.write(key: 'auth_token', value: newToken as String);
      final newRefresh = refreshResponse.data['refreshToken'];
      if (newRefresh != null) {
        await _storage.write(
          key: 'refresh_token',
          value: newRefresh as String,
        );
      }
      _logger.i('Token refreshed successfully');
      return newToken;
    }
    return null;
  }

  Future<void> _forceLogout(String message) async {
    _logger.w('Force logout: $message');
    try {
      final refreshToken = await _storage.read(key: 'refresh_token');
      if (refreshToken != null) {
        await _postWithLanWanFallback(
          path: ApiConstants.revoke,
          data: {'refreshToken': refreshToken},
        );
      }
    } catch (e) {
      _logger.w('Failed to revoke token on server: $e');
    }
    await _storage.delete(key: 'auth_token');
    await _storage.delete(key: 'refresh_token');
    _logoutController.add(message);
  }

  bool _shouldRetryOnWan(DioException error) {
    final attempted = error.requestOptions.extra[_wanFailoverKey] == true;
    final canFailover = _networkService.shouldRetryOverWan(
      requestContext: RequestContext(failoverAttempted: attempted),
    );
    return canFailover && _isConnectionFailure(error);
  }

  bool _isConnectionFailure(DioException error) {
    if (error.response != null) return false;
    return error.type == DioExceptionType.connectionError ||
        error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.sendTimeout ||
        error.type == DioExceptionType.receiveTimeout;
  }

  Future<Response<dynamic>> _retryRequestOverWan(RequestOptions source) async {
    final headers = Map<String, dynamic>.from(source.headers);
    final extra = Map<String, dynamic>.from(source.extra)
      ..[_wanFailoverKey] = true;
    final retriedRequest = source.copyWith(
      baseUrl: EnvConfig.network.wanOrigin,
      headers: headers,
      extra: extra,
    );
    return _dio.fetch<dynamic>(retriedRequest);
  }

  Future<Response<dynamic>> _postWithLanWanFallback({
    required String path,
    required Map<String, dynamic> data,
  }) async {
    final options = BaseOptions(
      headers: {'Content-Type': 'application/json', 'Accept': 'application/json'},
    );
    final primaryDio = Dio(options..baseUrl = _networkService.currentBaseUrl);
    try {
      return await primaryDio.post(path, data: data);
    } on DioException catch (e) {
      if (_networkService.isLanActive && _isConnectionFailure(e)) {
        _logger.w('Primary auth endpoint failed on LAN, retrying on WAN...');
        final fallbackDio = Dio(
          BaseOptions(baseUrl: EnvConfig.network.wanOrigin, headers: options.headers),
        );
        return fallbackDio.post(path, data: data);
      }
      rethrow;
    }
  }

  Dio get dio => _dio;
}
