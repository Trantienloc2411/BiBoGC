import 'dart:async';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:injectable/injectable.dart';
import 'package:logger/logger.dart';
import '../constants/api_constants.dart';

@lazySingleton
class DioClient {
  final Dio _dio;
  final FlutterSecureStorage _storage;
  final Logger _logger;

  static final _logoutController = StreamController<String>.broadcast();
  Stream<String> get forceLogoutStream => _logoutController.stream;

  DioClient({required FlutterSecureStorage storage, required Logger logger})
    : _storage = storage,
      _logger = logger,
      _dio = Dio(
        BaseOptions(
          baseUrl: ApiConstants.baseUrl,
          connectTimeout: const Duration(seconds: 15),
          receiveTimeout: const Duration(seconds: 15),
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
          },
        ),
      ) {
    _dio.interceptors.add(
      QueuedInterceptorsWrapper(
        onRequest: (options, handler) async {
          _logger.i('Request: ${options.method} ${options.path}');
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
            'Response: ${response.statusCode} ${response.statusMessage}',
          );
          return handler.next(response);
        },
        onError: (DioException e, handler) async {
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
          return handler.next(e);
        },
      ),
    );
  }

  Future<String?> _attemptTokenRefresh() async {
    final storedRefresh = await _storage.read(key: 'refresh_token');
    if (storedRefresh == null) return null;

    _logger.i('Attempting token refresh...');
    final refreshDio = Dio(
      BaseOptions(
        baseUrl: ApiConstants.baseUrl,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    final refreshResponse = await refreshDio.post(
      ApiConstants.refreshToken,
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
    await _storage.delete(key: 'auth_token');
    await _storage.delete(key: 'refresh_token');
    _logoutController.add(message);
  }

  Dio get dio => _dio;
}
