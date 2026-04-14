import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'core/bloc/theme_cubit.dart';
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
  late final ThemeCubit _themeCubit;
  final _scaffoldMessengerKey = GlobalKey<ScaffoldMessengerState>();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);

    _themeCubit = ThemeCubit()..loadSavedTheme();

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
    _themeCubit.close();
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
    return BlocBuilder<ThemeCubit, ThemeMode>(
      bloc: _themeCubit,
      builder: (_, themeMode) {
        return MaterialApp.router(
          title: "BiBo's Grocery",
          debugShowCheckedModeBanner: false,
          theme: AppTheme.lightTheme,
          darkTheme: AppTheme.darkTheme,
          themeMode: themeMode,
          scaffoldMessengerKey: _scaffoldMessengerKey,
          // Re-provide ThemeCubit inside the MaterialApp so all routes can
          // access it via context.read<ThemeCubit>() / context.watch<ThemeCubit>()
          builder: (context, child) {
            return BlocProvider.value(
              value: _themeCubit,
              child: SafeArea(
                maintainBottomViewPadding: true,
                child: child ?? const SizedBox.shrink(),
              ),
            );
          },
          routerConfig: appRouter,
        );
      },
    );
  }
}
