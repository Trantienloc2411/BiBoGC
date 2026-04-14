import 'package:flutter/material.dart';

const _kDenominations = [
  1000,
  2000,
  5000,
  10000,
  20000,
  50000,
  100000,
  200000,
  500000,
];

/// Reusable 3×3 denomination button grid with a reset button.
///
/// Used in both the payment confirmation dialog and the discount dialog.
/// - [onTap]: called with the denomination value to ADD to the current input.
/// - [onReset]: called to clear the current input back to zero.
class DenominationGrid extends StatelessWidget {
  final void Function(int denomination) onTap;
  final VoidCallback onReset;

  const DenominationGrid({
    super.key,
    required this.onTap,
    required this.onReset,
  });

  String _label(int value) {
    if (value >= 1000000) return '${value ~/ 1000000}tr';
    if (value >= 1000) return '${value ~/ 1000}k';
    return '$value';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    final buttonBg = isDark
        ? Colors.white.withAlpha(20)
        : theme.colorScheme.surfaceContainerHighest;
    final borderColor = isDark
        ? Colors.white.withAlpha(38)
        : theme.colorScheme.outline.withAlpha(80);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        GridView.builder(
          physics: const NeverScrollableScrollPhysics(),
          shrinkWrap: true,
          itemCount: _kDenominations.length,
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 3,
            mainAxisSpacing: 8,
            crossAxisSpacing: 8,
            childAspectRatio: 2.6,
          ),
          itemBuilder: (context, index) {
            final denom = _kDenominations[index];
            return Material(
              color: buttonBg,
              borderRadius: BorderRadius.circular(8),
              child: InkWell(
                borderRadius: BorderRadius.circular(8),
                onTap: () => onTap(denom),
                child: Container(
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    border: Border.all(color: borderColor),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    '+${_label(denom)}',
                    style: theme.textTheme.labelMedium?.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            );
          },
        ),
        const SizedBox(height: 8),
        OutlinedButton.icon(
          onPressed: onReset,
          icon: const Icon(Icons.backspace_outlined, size: 16),
          label: const Text('Đặt lại'),
          style: OutlinedButton.styleFrom(
            minimumSize: const Size(0, 36),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            foregroundColor: theme.colorScheme.error,
            side: BorderSide(
              color: theme.colorScheme.error.withAlpha(isDark ? 120 : 160),
            ),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
            textStyle: theme.textTheme.labelMedium?.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ],
    );
  }
}
