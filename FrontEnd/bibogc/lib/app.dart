import 'dart:async';
import 'package:flutter/material.dart';
import 'core/config/app_routes.dart';
import 'core/config/router.dart';
import 'core/config/theme.dart';
import 'core/di/injection.dart';
import 'core/network/dio_client.dart';

class BiBoApp extends StatefulWidget {
  const BiBoApp({super.key});

  @override
  State<BiBoApp> createState() => _BiBoAppState();
}

class _BiBoAppState extends State<BiBoApp> {
  late final StreamSubscription<void> _logoutSub;

  @override
  void initState() {
    super.initState();
    _logoutSub = getIt<DioClient>().forceLogoutStream.listen((_) {
      appRouter.go(AppRoutes.welcome);
    });
  }

  @override
  void dispose() {
    _logoutSub.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: "BiBo's Grocery",
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.system,
      routerConfig: appRouter,
    );
  }
}
