import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class InvoiceCard extends StatelessWidget {
  final Invoice invoice;
  final VoidCallback? onTap;

  const InvoiceCard({super.key, required this.invoice, this.onTap});

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
                  Icon(
                    Icons.receipt_long,
                    size: 18,
                    color: theme.colorScheme.secondary,
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      invoice.invoiceNumber,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 6),
              Row(
                children: [
                  Icon(Icons.description_outlined, size: 14, color: secondary),
                  const SizedBox(width: 4),
                  Text(
                    'Đơn hàng: ${invoice.orderNumber}',
                    style: theme.textTheme.bodySmall?.copyWith(
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
                    dateFormat.format(invoice.invoiceDate.toLocal()),
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: secondary,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Align(
                alignment: Alignment.centerRight,
                child: Text(
                  CurrencyUtils.formatCurrency(invoice.grandTotal),
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: theme.colorScheme.primary,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
