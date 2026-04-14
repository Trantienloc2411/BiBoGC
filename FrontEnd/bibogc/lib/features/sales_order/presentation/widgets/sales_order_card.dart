import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class SalesOrderCard extends StatelessWidget {
  final SalesOrder order;
  final VoidCallback? onTap;

  const SalesOrderCard({super.key, required this.order, this.onTap});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final dateFormat = DateFormat('dd/MM/yyyy HH:mm', 'vi_VN');
    final secondary = theme.colorScheme.onSurfaceVariant;

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      order.orderNumber,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  _buildStatusChip(context),
                ],
              ),
              const SizedBox(height: 6),
              Row(
                children: [
                  Icon(Icons.person_outline, size: 14, color: secondary),
                  const SizedBox(width: 4),
                  Text(
                    order.customerName ?? 'Khách lẻ',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: secondary,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 4),
              Row(
                children: [
                  Icon(Icons.access_time, size: 14, color: secondary),
                  const SizedBox(width: 4),
                  Text(
                    dateFormat.format(order.orderDate.toLocal()),
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: secondary,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    '${order.itemCount} sản phẩm',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: secondary,
                    ),
                  ),
                  Text(
                    CurrencyUtils.formatCurrency(order.totalAmount),
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.primary,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildStatusChip(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    final String label;
    final Color bg;
    final Color fg;

    switch (order.status) {
      case OrderStatus.draft:
        label = 'Đang soạn';
        bg = isDark ? const Color(0xFF2E2000) : const Color(0xFFFFF3E0);
        fg = isDark ? const Color(0xFFFFB74D) : const Color(0xFFE65100);
        break;
      case OrderStatus.completed:
        label = 'Hoàn thành';
        bg = isDark ? const Color(0xFF003820) : const Color(0xFFE8F5E9);
        fg = isDark ? const Color(0xFF69F0AE) : const Color(0xFF1B5E20);
        break;
      case OrderStatus.cancelled:
        label = 'Đã hủy';
        bg = isDark ? const Color(0xFF3B0000) : const Color(0xFFFFEBEE);
        fg = isDark ? const Color(0xFFFF8A80) : const Color(0xFFB71C1C);
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: fg,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}
