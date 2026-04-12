import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'app.dart';
import 'core/config/app_routes.dart';
import 'core/config/env_config.dart';
import 'core/config/router.dart';
import 'core/di/injection.dart';
import 'core/network/network_service.dart';
import 'core/security/biometric_auth_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  EnvConfig.init();
  await initializeDateFormatting('vi_VN');
  await configureDependencies();
  await getIt<NetworkService>().initialize();

  final storage = getIt<FlutterSecureStorage>();
  final token = await storage.read(key: 'auth_token');
  String initialRoute = token != null ? AppRoutes.home : AppRoutes.welcome;
  if (token != null) {
    final biometricService = getIt<BiometricAuthService>();
    final biometricAvailable = await biometricService.isAvailable();
    if (biometricAvailable) {
      final authorized = await biometricService.authenticate(
        reason: 'Xac thuc sinh trac hoc de mo ung dung POS',
      );
      initialRoute = authorized ? AppRoutes.home : AppRoutes.login;
    }
  }
  appRouter = createAppRouter(initialLocation: initialRoute);

  runApp(const BiBoApp());
}
