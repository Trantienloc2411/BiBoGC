import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:bibogc/features/product/presentation/widgets/product_variant_chip.dart';
import 'package:flutter/material.dart';

/// Card UI to visualize a Product and its ProductVariants.
///
/// This is where we make the parent/child relationship very clear for users:
/// - Parent row: Product name + category + total stock.
/// - Children row: list of [ProductVariantChip]s.
class ProductCard extends StatelessWidget {
  final Product product;
  final VoidCallback? onTap;
  final Widget? trailing;

  const ProductCard({
    super.key,
    required this.product,
    this.onTap,
    this.trailing,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          product.name,
                          style: textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        if (product.categoryName != null) ...[
                          const SizedBox(height: 4),
                          Text(
                            product.categoryName!,
                            style: textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                  const SizedBox(width: 12),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        'Tồn: ${product.totalStock}',
                        style: textTheme.bodyMedium?.copyWith(
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      if (product.lowStockThreshold != null &&
                          product.totalStock < product.lowStockThreshold!) ...[
                        const SizedBox(height: 4),
                        Chip(
                          label: const Text(
                            'Tồn thấp',
                            style: TextStyle(fontSize: 11),
                          ),
                          avatar: const Icon(
                            Icons.warning_amber_rounded,
                            size: 16,
                            color: Colors.orange,
                          ),
                          backgroundColor: Colors.orange.withAlpha(32),
                          side: BorderSide.none,
                        ),
                      ],
                      if (trailing != null) ...[
                        const SizedBox(height: 4),
                        trailing!,
                      ],
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 12),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: product.variants.isEmpty
                    ? [
                        Text(
                          'Chưa cấu hình biến thể',
                          style: textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                            fontStyle: FontStyle.italic,
                          ),
                        ),
                      ]
                    : product.variants
                          .map((v) => ProductVariantChip(variant: v))
                          .toList(),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
