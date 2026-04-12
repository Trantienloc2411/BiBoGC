import 'package:injectable/injectable.dart';
import 'package:local_auth/local_auth.dart';
import 'package:logger/logger.dart';

@lazySingleton
class BiometricAuthService {
  final LocalAuthentication _localAuth;
  final Logger _logger;

  BiometricAuthService({
    required LocalAuthentication localAuth,
    required Logger logger,
  }) : _localAuth = localAuth,
       _logger = logger;

  Future<bool> isAvailable() async {
    try {
      final canCheck = await _localAuth.canCheckBiometrics;
      final isDeviceSupported = await _localAuth.isDeviceSupported();
      return canCheck && isDeviceSupported;
    } catch (e) {
      _logger.w('Unable to check biometric availability: $e');
      return false;
    }
  }

  Future<bool> authenticate({
    String reason = 'Xac thuc sinh trac hoc de dang nhap',
  }) async {
    try {
      final available = await isAvailable();
      if (!available) return false;
      return await _localAuth.authenticate(
        localizedReason: reason,
        biometricOnly: false,
        persistAcrossBackgrounding: true,
      );
    } catch (e) {
      _logger.w('Biometric authenticate failed: $e');
      return false;
    }
  }
}
