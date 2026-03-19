import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'app.dart';
import 'core/config/app_routes.dart';
import 'core/config/env_config.dart';
import 'core/config/router.dart';
import 'core/di/injection.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  EnvConfig.init();
  await initializeDateFormatting('vi_VN');
  await configureDependencies();

  final storage = getIt<FlutterSecureStorage>();
  final token = await storage.read(key: 'auth_token');
  final initialRoute = token != null ? AppRoutes.home : AppRoutes.welcome;
  appRouter = createAppRouter(initialLocation: initialRoute);

  runApp(const BiBoApp());
}
