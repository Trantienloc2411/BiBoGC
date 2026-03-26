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
              icon: Icons.receipt_long,
              color: Colors.blue.shade100,
              iconColor: Colors.blue.shade800,
              label: 'Đơn hàng',
              subLabel: 'Bán hàng',
              onTap: () => context.push(AppRoutes.salesOrders),
            ),
            _buildActionCard(
              context,
              icon: Icons.description,
              color: Colors.teal.shade100,
              iconColor: Colors.teal.shade800,
              label: 'Hóa đơn',
              subLabel: 'Lịch sử',
              onTap: () => context.push(AppRoutes.invoices),
            ),
          ],
        ),
      ],
    );
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
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(color: color, shape: BoxShape.circle),
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
