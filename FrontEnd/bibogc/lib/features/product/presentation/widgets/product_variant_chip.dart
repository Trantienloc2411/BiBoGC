import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:flutter/material.dart';

/// Small chip-style widget to display a ProductVariant inside a product card.
///
/// Shows variant name, unit and price; optional stock/expiry if available.
class ProductVariantChip extends StatelessWidget {
  final ProductVariant variant;

  const ProductVariantChip({super.key, required this.variant});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: theme.colorScheme.outlineVariant),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            variant.name,
            style: textTheme.labelLarge?.copyWith(fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 4),
          Row(
            children: [
              Text(
                '${variant.salePrice.toStringAsFixed(0)}₫',
                style: textTheme.bodyMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: theme.colorScheme.primary,
                ),
              ),
              const SizedBox(width: 8),
              Text(
                variant.unit,
                style: textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ),
          if (variant.stockQuantity != null) ...[
            const SizedBox(height: 2),
            Text(
              'Tồn: ${variant.stockQuantity}',
              style: textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ],
      ),
    );
  }
}
