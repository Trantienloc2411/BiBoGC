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
  late final StreamSubscription<String> _logoutSub;
  final _scaffoldMessengerKey = GlobalKey<ScaffoldMessengerState>();

  @override
  void initState() {
    super.initState();
    _logoutSub = getIt<DioClient>().forceLogoutStream.listen((message) {
      _scaffoldMessengerKey.currentState?.showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.logout, color: Colors.white, size: 20),
              const SizedBox(width: 12),
              Expanded(child: Text(message)),
            ],
          ),
          behavior: SnackBarBehavior.floating,
          backgroundColor: Colors.red.shade700,
          duration: const Duration(seconds: 4),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(10),
          ),
        ),
      );
      appRouter.go(AppRoutes.login);
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
      scaffoldMessengerKey: _scaffoldMessengerKey,
      routerConfig: appRouter,
    );
  }
}
