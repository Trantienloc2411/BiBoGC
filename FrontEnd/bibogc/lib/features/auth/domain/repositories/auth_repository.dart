import 'package:dartz/dartz.dart';
import '../../../../core/error/failures.dart';

abstract class AuthRepository {
  Future<Either<Failure, void>> login(String username, String password);
  Future<void> logout();
  Future<bool> isLoggedIn();
  Future<String?> getLastUsername();
}
