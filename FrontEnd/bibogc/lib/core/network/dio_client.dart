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
      InterceptorsWrapper(
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
        onError: (DioException e, handler) {
          _logger.e(
            'Error: ${e.message}',
            error: e.error,
            stackTrace: e.stackTrace,
          );
          return handler.next(e);
        },
      ),
    );
  }

  Dio get dio => _dio;
}
