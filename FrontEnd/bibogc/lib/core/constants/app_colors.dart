import 'package:flutter/material.dart';

/// Centralized color tokens (Design System).
///
/// Keep colors here to avoid hard-coding values across the UI. This makes it
/// easy to:
/// - keep Light/Dark mode consistent
/// - evolve the visual style without hunting for magic numbers
/// - ensure accessibility contrast stays under control
class AppColors {
  // Brand
  static const primary = Color(0xFF0069D2); // Blue
  static const primaryDark = Color(0xFF4DA3FF); // Lighter blue for dark mode
  static const onPrimaryLight = Color(0xFFFFFFFF);
  static const onPrimaryDark = Color(0xFF003366);

  // Background / Surface
  static const backgroundLight = Color(0xFFF5F7FA);
  static const backgroundDark = Color(0xFF000E23);
  static const surfaceLight = Color(0xFFFFFFFF);
  static const surfaceDark = Color(0xFF0F1F38);

  // Text
  static const textPrimaryLight = Color(0xFF1A1C1E);
  static const textPrimaryDark = Color(0xFFE2E2E6);
  static const textSecondaryLight = Color(0xFF444746);
  static const textSecondaryDark = Color(0xFFC4C6D0);

  // Semantic
  static const errorLight = Color(0xFFBA1A1A);
  static const errorDark = Color(0xFFFFB4AB);
  static const successLight = Color(0xFF1AA260);
  static const successDark = Color(0xFF6DD58C);
  static const warningLight = Color(0xFFD97706);
  static const warningDark = Color(0xFFFFB74D);

  // Utility
  static const outlineLight = Color(0xFFCAC4D0);
  static const outlineDark = Color(0xFF444746);

  const AppColors._();
}

