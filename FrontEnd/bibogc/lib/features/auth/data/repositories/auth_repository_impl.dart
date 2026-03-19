import 'package:dio/dio.dart';
import 'package:dartz/dartz.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:injectable/injectable.dart';
import 'package:logger/logger.dart';

import '../../../../core/error/failures.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/auth_remote_datasource.dart';

@LazySingleton(as: AuthRepository)
class AuthRepositoryImpl implements AuthRepository {
  final AuthRemoteDataSource _remoteDataSource;
  final FlutterSecureStorage _storage;
  final Logger _logger;

  AuthRepositoryImpl(this._remoteDataSource, this._storage, this._logger);

  @override
  Future<Either<Failure, void>> login(String username, String password) async {
    try {
      final response = await _remoteDataSource.login(username, password);
      // Assuming response has 'token'. Adjust key based on actual API.
      // If API returns { "token": "..." } use response['token']
      // If API returns { "accessToken": "..." } use response['accessToken']
      // For now, I'll check for both or print keys if failed.
      final token = response['token'] ?? response['accessToken'];

      if (token != null) {
        await _storage.write(key: 'auth_token', value: token);
        // Also save refreshToken if available
        if (response['refreshToken'] != null) {
          await _storage.write(
            key: 'refresh_token',
            value: response['refreshToken'],
          );
        }
        return const Right(null);
      } else {
        return const Left(ServerFailure('Không tìm thấy token trong phản hồi'));
      }
    } on DioException catch (e) {
      // Handle Dio Errors specifically
      if (e.response != null) {
        // Try to parse error message from server response
        // Common formats: { "message": "..." } or { "detail": "..." } or { "error": "..." }
        final data = e.response?.data;
        String? message;
        if (data is Map<String, dynamic>) {
          message = data['message'] ?? data['detail'] ?? data['title'];
        }
        return Left(
          ServerFailure(message ?? 'Lỗi máy chủ: ${e.response?.statusCode}'),
        );
      } else {
        return const Left(
          ServerFailure('Lỗi kết nối mạng. Vui lòng kiểm tra lại.'),
        );
      }
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<void> logout() async {
    try {
      final refreshToken = await _storage.read(key: 'refresh_token');
      if (refreshToken != null) {
        await _remoteDataSource.revokeRefreshToken(refreshToken);
      }
      await _remoteDataSource.logout();
    } catch (e) {
      _logger.w('Server logout failed (tokens will be cleared locally): $e');
    }
    await _storage.delete(key: 'auth_token');
    await _storage.delete(key: 'refresh_token');
  }

  @override
  Future<bool> isLoggedIn() async {
    final token = await _storage.read(key: 'auth_token');
    return token != null;
  }
}
