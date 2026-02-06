import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/config/app_routes.dart';

class QuickActionsGrid extends StatelessWidget {
  const QuickActionsGrid({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        _buildLargeSaleButton(context),
        const SizedBox(height: 16),
        GridView.count(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          crossAxisCount: 2,
          mainAxisSpacing: 16,
          crossAxisSpacing: 16,
          childAspectRatio: 1.5,
          children: [
            _buildActionCard(
              context,
              icon: Icons.inventory_2,
              color: Colors.orange.shade100,
              iconColor: Colors.orange.shade800,
              label: 'Nhập kho',
              subLabel: 'Quản lý hàng',
              onTap: () => context.push(AppRoutes.importStock),
            ),
            _buildActionCard(
              context,
              icon: Icons.local_shipping,
              color: Colors.purple.shade100,
              iconColor: Colors.purple.shade800,
              label: 'Đối tác',
              subLabel: 'Nhà cung cấp',
              onTap: () => context.push(AppRoutes.suppliers),
            ),
            _buildActionCard(
              context,
              icon: Icons.analytics,
              color: Colors.green.shade100,
              iconColor: Colors.green.shade800,
              label: 'Báo cáo',
              subLabel: 'Xem kết quả',
              onTap: () {
                // TODO: Navigate to reports page when implemented
              },
            ),
            _buildActionCard(
              context,
              icon: Icons.settings,
              color: Colors.grey.shade200,
              iconColor: Colors.grey.shade800,
              label: 'Cài đặt',
              subLabel: 'Cửa hàng',
              onTap: () {
                // TODO: Navigate to settings page when implemented
              },
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildLargeSaleButton(BuildContext context) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: () {
        // TODO: Navigate to POS page when implemented
      },
      borderRadius: BorderRadius.circular(20),
      child: Container(
        height: 100,
        padding: const EdgeInsets.symmetric(horizontal: 24),
        decoration: BoxDecoration(
          color: theme.colorScheme.primaryContainer.withAlpha(102),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: theme.colorScheme.primary.withAlpha(26)),
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: theme.colorScheme.primary,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: theme.colorScheme.primary.withAlpha(102),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: const Icon(
                Icons.point_of_sale,
                color: Colors.white,
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
    required Color color,
    required Color iconColor,
    required String label,
    required String subLabel,
    required VoidCallback onTap,
  }) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withAlpha(13),
              blurRadius: 10,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(color: color, shape: BoxShape.circle),
              child: Icon(icon, color: iconColor, size: 28),
            ),
            const SizedBox(height: 12),
            Text(
              label,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 4),
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
