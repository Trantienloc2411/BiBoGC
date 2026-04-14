import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/config/app_routes.dart';
import '../../../../core/constants/app_colors.dart';

class QuickActionsGrid extends StatelessWidget {
  const QuickActionsGrid({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        _buildLargeSaleButton(context),
        const SizedBox(height: 16),
        Row(
          children: [
            Expanded(
              child: _buildActionCard(
                context,
                icon: Icons.receipt_long,
                containerColor: _containerColor(context, isSecondary: false),
                iconColor: Theme.of(context).colorScheme.onPrimaryContainer,
                label: 'Đơn hàng',
                subLabel: 'Bán hàng',
                onTap: () => context.push(AppRoutes.salesOrders),
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: _buildActionCard(
                context,
                icon: Icons.description,
                containerColor: _containerColor(context, isSecondary: true),
                iconColor: Theme.of(context).colorScheme.onSecondaryContainer,
                label: 'Hóa đơn',
                subLabel: 'Lịch sử',
                onTap: () => context.push(AppRoutes.invoices),
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: _buildActionCard(
                context,
                icon: Icons.inventory_2_outlined,
                containerColor: _containerColor(context, isTertiary: true),
                iconColor: Theme.of(context).colorScheme.onTertiaryContainer,
                label: 'Sản phẩm',
                subLabel: 'Kho hàng',
                onTap: () => context.push(AppRoutes.products),
              ),
            ),
          ],
        ),
      ],
    );
  }

  /// Returns a tinted container color that adapts to dark/light mode.
  Color _containerColor(
    BuildContext context, {
    bool isSecondary = false,
    bool isTertiary = false,
  }) {
    final cs = Theme.of(context).colorScheme;
    if (isTertiary) return cs.tertiaryContainer;
    if (isSecondary) return cs.secondaryContainer;
    return cs.primaryContainer;
  }

  Widget _buildLargeSaleButton(BuildContext context) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: () => context.push(AppRoutes.salesOrders),
      borderRadius: BorderRadius.circular(20),
      child: Container(
        height: 100,
        padding: const EdgeInsets.symmetric(horizontal: 24),
        decoration: BoxDecoration(
          color: theme.colorScheme.primaryContainer.withAlpha(
            theme.brightness == Brightness.dark ? 80 : 100,
          ),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: theme.colorScheme.primary.withAlpha(
              theme.brightness == Brightness.dark ? 60 : 26,
            ),
          ),
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: theme.colorScheme.primary,
                borderRadius: BorderRadius.circular(16),
                boxShadow: theme.brightness == Brightness.dark
                    ? null
                    : [
                        BoxShadow(
                          color: theme.colorScheme.primary.withAlpha(100),
                          blurRadius: 10,
                          offset: const Offset(0, 4),
                        ),
                      ],
              ),
              child: Icon(
                Icons.point_of_sale,
                color: theme.colorScheme.onPrimary,
                size: 32,
              ),
            ),
            const SizedBox(width: 24),
            Expanded(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Bán hàng',
                    style: theme.textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.onSurface,
                    ),
                  ),
                  Text(
                    'Tạo hóa đơn nhanh',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            Icon(
              Icons.arrow_forward_ios,
              color: theme.colorScheme.primary,
              size: 20,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActionCard(
    BuildContext context, {
    required IconData icon,
    required Color containerColor,
    required Color iconColor,
    required String label,
    required String subLabel,
    required VoidCallback onTap,
  }) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: AppColors.cardDecoration(context, radius: 20),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: isDark ? containerColor.withAlpha(60) : containerColor,
                shape: BoxShape.circle,
              ),
              child: Icon(icon, color: iconColor, size: 22),
            ),
            const SizedBox(height: 6),
            Text(
              label,
              style: theme.textTheme.titleSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 2),
            Text(
              subLabel,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
