import 'package:flutter/material.dart';
import 'core/config/router.dart';
import 'core/config/theme.dart';

class BiBoApp extends StatelessWidget {
  const BiBoApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: "BiBo's Grocery",
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.system, // Auto switch based on device setting
      routerConfig: appRouter,
    );
  }
}
