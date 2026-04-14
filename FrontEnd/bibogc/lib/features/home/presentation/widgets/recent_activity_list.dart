import 'package:bibogc/core/constants/app_colors.dart';
import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/config/app_routes.dart';
import '../../../invoice/domain/entities/invoice.dart';
import '../../../invoice/presentation/bloc/invoice_bloc.dart';
import '../../../sales_order/domain/entities/sales_order.dart';
import '../../../sales_order/presentation/bloc/sales_order_bloc.dart';

class RecentActivityList extends StatelessWidget {
  const RecentActivityList({super.key});

  String _formatCurrency(double amount) => CurrencyUtils.formatCurrency(amount);

  String _formatTime(DateTime dt) {
    final h = dt.hour.toString().padLeft(2, '0');
    final m = dt.minute.toString().padLeft(2, '0');
    final d = dt.day.toString().padLeft(2, '0');
    final mo = dt.month.toString().padLeft(2, '0');
    return '$h:$m • $d/$mo';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Gần đây',
          style: theme.textTheme.titleLarge?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        _buildOrdersSection(context, theme),
        const SizedBox(height: 16),
        _buildInvoicesSection(context, theme),
      ],
    );
  }

  Widget _buildOrdersSection(BuildContext context, ThemeData theme) {
    return BlocBuilder<SalesOrderBloc, SalesOrderState>(
      builder: (context, state) {
        final orders = state.orders.take(5).toList();
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Đơn hàng',
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.w600,
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
                TextButton(
                  onPressed: () => context.push(AppRoutes.salesOrders),
                  child: const Text('Xem tất cả'),
                ),
              ],
            ),
            if (state.listStatus == SalesOrderStatus.loading && orders.isEmpty)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 16),
                child: Center(child: CircularProgressIndicator()),
              )
            else if (orders.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 12),
                child: Text(
                  'Chưa có đơn hàng nào',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              )
            else
              ListView.separated(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: orders.length,
                separatorBuilder: (_, i) => const SizedBox(height: 10),
                itemBuilder: (context, index) =>
                    _buildOrderItem(context, theme, orders[index]),
              ),
          ],
        );
      },
    );
  }

  Widget _buildInvoicesSection(BuildContext context, ThemeData theme) {
    return BlocBuilder<InvoiceBloc, InvoiceState>(
      builder: (context, state) {
        final invoices = state.invoices.take(5).toList();
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Hóa đơn',
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.w600,
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
                TextButton(
                  onPressed: () => context.push(AppRoutes.invoices),
                  child: const Text('Xem tất cả'),
                ),
              ],
            ),
            if (state.listStatus == InvoiceStatus.loading && invoices.isEmpty)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 16),
                child: Center(child: CircularProgressIndicator()),
              )
            else if (invoices.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 12),
                child: Text(
                  'Chưa có hóa đơn nào',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              )
            else
              ListView.separated(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: invoices.length,
                separatorBuilder: (_, i) => const SizedBox(height: 10),
                itemBuilder: (context, index) =>
                    _buildInvoiceItem(context, theme, invoices[index]),
              ),
          ],
        );
      },
    );
  }

  Widget _buildOrderItem(
      BuildContext context, ThemeData theme, SalesOrder order) {
    final isDark = theme.brightness == Brightness.dark;
    final (Color statusBg, Color statusFg) = switch (order.status) {
      OrderStatus.completed => AppColors.statusSuccess(isDark),
      OrderStatus.cancelled => AppColors.statusError(isDark),
      OrderStatus.draft => AppColors.statusDraft(isDark),
    };
    return InkWell(
      onTap: () => context.push(AppRoutes.salesOrderDetail(order.id)),
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: AppColors.cardDecoration(context),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: theme.colorScheme.primaryContainer.withAlpha(
                  isDark ? 60 : 255,
                ),
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.receipt_long,
                color: theme.colorScheme.onPrimaryContainer,
                size: 22,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    order.orderNumber,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    '${_formatTime(order.createdAt)} • ${order.itemCount} sản phẩm',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  _formatCurrency(order.totalAmount),
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Container(
                  margin: const EdgeInsets.only(top: 4),
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: statusBg,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    order.status.label,
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: statusFg,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInvoiceItem(
      BuildContext context, ThemeData theme, Invoice invoice) {
    final isDark = theme.brightness == Brightness.dark;
    return InkWell(
      onTap: () => context.push(AppRoutes.invoiceDetail(invoice.id)),
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: AppColors.cardDecoration(context),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: theme.colorScheme.secondaryContainer.withAlpha(
                  isDark ? 60 : 255,
                ),
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.description,
                color: theme.colorScheme.onSecondaryContainer,
                size: 22,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    invoice.invoiceNumber,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    '${_formatTime(invoice.createdAt)} • ${invoice.paymentMethodLabel}',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            Text(
              _formatCurrency(invoice.grandTotal),
              style: theme.textTheme.titleSmall?.copyWith(
                fontWeight: FontWeight.bold,
                color: theme.colorScheme.secondary,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
