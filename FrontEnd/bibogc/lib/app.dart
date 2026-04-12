import 'dart:async';
import 'package:flutter/material.dart';
import 'core/config/app_routes.dart';
import 'core/config/router.dart';
import 'core/config/theme.dart';
import 'core/di/injection.dart';
import 'core/network/dio_client.dart';
import 'core/network/network_service.dart';

class BiBoApp extends StatefulWidget {
  const BiBoApp({super.key});

  @override
  State<BiBoApp> createState() => _BiBoAppState();
}

class _BiBoAppState extends State<BiBoApp> with WidgetsBindingObserver {
  late final StreamSubscription<String> _logoutSub;
  final _scaffoldMessengerKey = GlobalKey<ScaffoldMessengerState>();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
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
    WidgetsBinding.instance.removeObserver(this);
    _logoutSub.cancel();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      unawaited(getIt<NetworkService>().onAppResumed());
    }
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
      builder: (context, child) {
        return SafeArea(
          maintainBottomViewPadding: true,
          child: child ?? const SizedBox.shrink(),
        );
      },
      routerConfig: appRouter,
    );
  }
}
