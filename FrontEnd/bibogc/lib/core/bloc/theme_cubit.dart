import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Manages [ThemeMode] with persistence via [SharedPreferences].
///
/// Usage:
/// ```dart
/// context.read<ThemeCubit>().setTheme(ThemeMode.dark);
/// ```
class ThemeCubit extends Cubit<ThemeMode> {
  static const _prefsKey = 'app_theme_mode';

  ThemeCubit() : super(ThemeMode.system);

  /// Call once at startup to restore the user's last preference.
  Future<void> loadSavedTheme() async {
    final prefs = await SharedPreferences.getInstance();
    final saved = prefs.getString(_prefsKey);
    emit(switch (saved) {
      'light' => ThemeMode.light,
      'dark' => ThemeMode.dark,
      _ => ThemeMode.system,
    });
  }

  /// Persist and apply a new [ThemeMode].
  Future<void> setTheme(ThemeMode mode) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_prefsKey, mode.name);
    emit(mode);
  }

  /// Cycle: system → light → dark → system
  Future<void> cycleTheme() async {
    final next = switch (state) {
      ThemeMode.system => ThemeMode.light,
      ThemeMode.light => ThemeMode.dark,
      ThemeMode.dark => ThemeMode.system,
    };
    await setTheme(next);
  }
}
