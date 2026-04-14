import 'package:flutter/material.dart';

/// Centralized color tokens (Design System).
///
/// Keep colors here to avoid hard-coding values across the UI.
class AppColors {
  // ── Brand ──────────────────────────────────────────────────────────────────
  static const primary = Color(0xFF0069D2);
  static const primaryDark = Color(0xFF4DA3FF);
  static const onPrimaryLight = Color(0xFFFFFFFF);
  static const onPrimaryDark = Color(0xFF003366);

  // ── Background / Surface (Light) ───────────────────────────────────────────
  static const backgroundLight = Color(0xFFF5F7FA);
  static const surfaceLight = Color(0xFFFFFFFF);
  static const surfaceContainerLight = Color(0xFFEEF1F8);

  // ── Background / Surface (Dark) ────────────────────────────────────────────
  // Three distinct layers: background < surface < card (contrast steps)
  static const backgroundDark = Color(0xFF0E1117); // Darkest – scaffold bg
  static const surfaceDark = Color(0xFF161C27); // Slightly lighter – drawers
  static const cardDark = Color(0xFF1E2535); // Cards & containers

  // ── Text ───────────────────────────────────────────────────────────────────
  static const textPrimaryLight = Color(0xFF1A1C1E);
  static const textPrimaryDark = Color(0xFFE2E6F0);
  static const textSecondaryLight = Color(0xFF444746);
  static const textSecondaryDark = Color(0xFFA0A8BA);

  // ── Semantic ───────────────────────────────────────────────────────────────
  static const errorLight = Color(0xFFBA1A1A);
  static const errorDark = Color(0xFFFFB4AB);
  static const successLight = Color(0xFF1AA260);
  static const successDark = Color(0xFF6DD58C);
  static const warningLight = Color(0xFFD97706);
  static const warningDark = Color(0xFFFFB74D);

  // ── Utility ────────────────────────────────────────────────────────────────
  static const outlineLight = Color(0xFFCAC4D0);
  static const outlineDark = Color(0xFF3A4255);

  // ── Status chip helpers ────────────────────────────────────────────────────
  /// Returns (background, foreground) for the given semantic state.
  static (Color bg, Color fg) statusDraft(bool isDark) => isDark
      ? (const Color(0xFF2E2000), const Color(0xFFFFB74D))
      : (const Color(0xFFFFF3E0), const Color(0xFFE65100));

  static (Color bg, Color fg) statusSuccess(bool isDark) => isDark
      ? (const Color(0xFF003820), const Color(0xFF69F0AE))
      : (const Color(0xFFE8F5E9), const Color(0xFF1B5E20));

  static (Color bg, Color fg) statusError(bool isDark) => isDark
      ? (const Color(0xFF3B0000), const Color(0xFFFF8A80))
      : (const Color(0xFFFFEBEE), const Color(0xFFB71C1C));

  // ── Adaptive card decoration ───────────────────────────────────────────────
  /// Returns a [BoxDecoration] that uses shadows in light mode and a subtle
  /// border in dark mode — keeps cards readable without blending into the bg.
  static BoxDecoration cardDecoration(
    BuildContext context, {
    double radius = 16,
    Color? color,
  }) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return BoxDecoration(
      color: color ?? (isDark ? cardDark : cs.surface),
      borderRadius: BorderRadius.circular(radius),
      boxShadow: isDark
          ? null
          : [
              BoxShadow(
                color: Colors.black.withAlpha(12),
                blurRadius: 12,
                offset: const Offset(0, 3),
              ),
            ],
      border: isDark
          ? Border.all(color: outlineDark.withAlpha(100), width: 1)
          : null,
    );
  }

  const AppColors._();
}
