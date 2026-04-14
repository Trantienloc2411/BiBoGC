import 'package:bibogc/core/constants/app_colors.dart';
import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:flutter/material.dart';

class SummaryCard extends StatelessWidget {
  /// Null while loading.
  final double? totalRevenue;
  final int? orderCount;
  final double? growthPercentage;

  const SummaryCard({
    super.key,
    this.totalRevenue,
    this.orderCount,
    this.growthPercentage,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isLoading = totalRevenue == null;
    // On the primary gradient background both states need light, high-contrast colours
    final trendPositive = AppColors.successDark; // light green readable on blue
    final trendNegative = AppColors.errorDark; // light red readable on blue

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            theme.colorScheme.primary,
            theme.colorScheme.primary.withAlpha(204),
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: theme.colorScheme.primary.withAlpha(77),
            blurRadius: 15,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.white.withAlpha(51),
              borderRadius: BorderRadius.circular(16),
            ),
            child: const Icon(
              Icons.account_balance_wallet,
              color: Colors.white,
              size: 32,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'DOANH THU HÔM NAY',
                  style: theme.textTheme.labelMedium?.copyWith(
                    color: Colors.white.withAlpha(204),
                    letterSpacing: 1.1,
                  ),
                ),
                const SizedBox(height: 4),
                isLoading
                    ? _Shimmer(width: 160, height: 28)
                    : Text(
                        CurrencyUtils.formatCurrency(totalRevenue!),
                        style: theme.textTheme.headlineMedium?.copyWith(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              isLoading
                  ? _Shimmer(width: 80, height: 16)
                  : Text(
                      '${orderCount ?? 0} đơn hàng',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: Colors.white,
                      ),
                    ),
              const SizedBox(height: 4),
              isLoading
                  ? _Shimmer(width: 50, height: 16)
                  : Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(
                          (growthPercentage ?? 0) >= 0
                              ? Icons.trending_up
                              : Icons.trending_down,
                          color: (growthPercentage ?? 0) >= 0
                              ? trendPositive
                              : trendNegative,
                          size: 16,
                        ),
                        const SizedBox(width: 4),
                        Text(
                          '${(growthPercentage ?? 0).abs().toStringAsFixed(1)}%',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: (growthPercentage ?? 0) >= 0
                                ? trendPositive
                                : trendNegative,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
            ],
          ),
        ],
      ),
    );
  }
}

/// Simple white shimmer placeholder shown while data loads.
class _Shimmer extends StatelessWidget {
  final double width;
  final double height;
  const _Shimmer({required this.width, required this.height});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: width,
      height: height,
      decoration: BoxDecoration(
        color: Colors.white.withAlpha(60),
        borderRadius: BorderRadius.circular(6),
      ),
    );
  }
}
