import 'package:injectable/injectable.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../core/network/dio_client.dart';

abstract class AuthRemoteDataSource {
  Future<Map<String, dynamic>> login(String username, String password);
  Future<void> logout();
  Future<void> revokeRefreshToken(String refreshToken);
}

@LazySingleton(as: AuthRemoteDataSource)
class AuthRemoteDataSourceImpl implements AuthRemoteDataSource {
  final DioClient _dioClient;

  AuthRemoteDataSourceImpl(this._dioClient);

  @override
  Future<Map<String, dynamic>> login(String username, String password) async {
    final response = await _dioClient.dio.post(
      ApiConstants.login,
      data: {'username': username, 'password': password},
    );
    return response.data;
  }

  @override
  Future<void> logout() async {
    await _dioClient.dio.post(ApiConstants.logout);
  }

  @override
  Future<void> revokeRefreshToken(String refreshToken) async {
    await _dioClient.dio.post(
      ApiConstants.revoke,
      data: {'refreshToken': refreshToken},
    );
  }
}
