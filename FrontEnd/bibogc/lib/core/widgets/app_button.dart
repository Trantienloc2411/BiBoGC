import 'package:flutter/material.dart';

/// App-level button with built-in loading/disabled handling.
///
/// Why:
/// - Prevents inconsistent sizes/states across screens
/// - Standardizes "loading" UX (spinner + disabled button)
/// - Encourages icon+text pattern for non-technical users
class AppButton extends StatelessWidget {
  final VoidCallback? onPressed;
  final Widget child;
  final bool isLoading;
  final double height;
  final ButtonStyle? style;

  const AppButton({
    super.key,
    required this.onPressed,
    required this.child,
    this.isLoading = false,
    this.height = 56,
    this.style,
  });

  @override
  Widget build(BuildContext context) {
    final enabled = onPressed != null && !isLoading;

    return SizedBox(
      height: height,
      width: double.infinity,
      child: ElevatedButton(
        onPressed: enabled ? onPressed : null,
        style: style,
        child: AnimatedSwitcher(
          duration: const Duration(milliseconds: 150),
          child: isLoading
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : child,
        ),
      ),
    );
  }
}

