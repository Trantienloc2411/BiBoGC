import 'package:bibogc/core/di/injection.dart';
import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/core/widgets/app_button.dart';
import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:bibogc/features/product/presentation/bloc/product_bloc.dart';
import 'package:bibogc/features/sales_order/presentation/bloc/sales_order_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AddItemBottomSheet extends StatefulWidget {
  final String orderId;

  const AddItemBottomSheet({super.key, required this.orderId});

  @override
  State<AddItemBottomSheet> createState() => _AddItemBottomSheetState();
}

class _AddItemBottomSheetState extends State<AddItemBottomSheet> {
  Product? _selectedProduct;
  ProductVariant? _selectedVariant;
  int _quantity = 1;
  int _step = 0; // 0 = select product, 1 = select variant & quantity

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<ProductBloc>()..add(const ProductsStarted()),
      child: _buildSheet(context),
    );
  }

  Widget _buildSheet(BuildContext context) {
    final theme = Theme.of(context);

    return DraggableScrollableSheet(
      initialChildSize: 0.8,
      minChildSize: 0.5,
      maxChildSize: 0.95,
      expand: false,
      builder: (_, scrollController) {
        return Column(
          children: [
            Container(
              margin: const EdgeInsets.only(top: 8),
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
              child: Row(
                children: [
                  if (_step == 1) ...[
                    IconButton(
                      icon: const Icon(Icons.arrow_back),
                      onPressed: () => setState(() {
                        _step = 0;
                        _selectedVariant = null;
                      }),
                    ),
                    const SizedBox(width: 4),
                  ],
                  Text(
                    _step == 0 ? 'Chọn sản phẩm' : 'Chọn phiên bản & số lượng',
                    style: theme.textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const Spacer(),
                  IconButton(
                    onPressed: () => Navigator.of(context).pop(),
                    icon: const Icon(Icons.close),
                  ),
                ],
              ),
            ),
            const Divider(height: 1),
            if (_step == 0)
              _buildStep1(context, scrollController)
            else
              _buildStep2(context, scrollController),
          ],
        );
      },
    );
  }

  Widget _buildStep1(BuildContext context, ScrollController scrollController) {
    return Expanded(
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(12),
            child: Builder(
              builder: (ctx) {
                return TextField(
                  decoration: const InputDecoration(
                    prefixIcon: Icon(Icons.search),
                    hintText: 'Tìm sản phẩm...',
                    border: OutlineInputBorder(),
                  ),
                  onChanged: (value) =>
                      ctx.read<ProductBloc>().add(ProductsSearchChanged(value)),
                );
              },
            ),
          ),
          Expanded(
            child: BlocBuilder<ProductBloc, ProductState>(
              builder: (context, state) {
                if (state.status == ProductStatus.loading &&
                    state.products.isEmpty) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.products.isEmpty) {
                  return const Center(child: Text('Không tìm thấy sản phẩm'));
                }
                return ListView.builder(
                  controller: scrollController,
                  itemCount: state.products.length,
                  itemBuilder: (context, index) {
                    final product = state.products[index];
                    return ListTile(
                      leading: Container(
                        width: 40,
                        height: 40,
                        decoration: BoxDecoration(
                          color: Colors.blue.shade50,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: const Icon(
                          Icons.inventory_2_outlined,
                          color: Colors.blue,
                        ),
                      ),
                      title: Text(
                        product.name,
                        style: const TextStyle(fontWeight: FontWeight.w600),
                      ),
                      subtitle: Text('Tồn kho: ${product.totalStock}', style: TextStyle(color: Colors.grey[600], fontSize: 12)),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () {
                        context.read<ProductBloc>().add(
                          ProductVariantsRequested(product.id),
                        );
                        setState(() {
                          _selectedProduct = product;
                          _selectedVariant = null;
                          _quantity = 1;
                          _step = 1;
                        });
                      },
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStep2(BuildContext context, ScrollController scrollController) {
    final product = _selectedProduct!;
    final theme = Theme.of(context);

    return BlocListener<SalesOrderBloc, SalesOrderState>(
      listenWhen: (prev, curr) =>
          curr.actionStatus != prev.actionStatus &&
          curr.actionStatus != SalesOrderStatus.initial,
      listener: (context, state) {
        if (state.actionStatus == SalesOrderStatus.success) {
          Navigator.of(context).pop();
          DialogUtils.showSuccessDialog(
            context,
            message: 'Đã thêm sản phẩm vào đơn hàng',
          );
        } else if (state.actionStatus == SalesOrderStatus.failure) {
          DialogUtils.showErrorDialog(
            context,
            message: state.errorMessage ?? 'Thêm sản phẩm thất bại',
          );
        }
      },
      child: BlocBuilder<ProductBloc, ProductState>(
        builder: (context, productState) {
          return BlocBuilder<SalesOrderBloc, SalesOrderState>(
            buildWhen: (prev, curr) => curr.actionStatus != prev.actionStatus,
            builder: (context, state) {
              final isLoading = state.actionStatus == SalesOrderStatus.loading;

              return Expanded(
            child: ListView(
              controller: scrollController,
              padding: const EdgeInsets.all(16),
              children: [
                Text(
                  product.name,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 16),
                Text('Chọn phiên bản:', style: theme.textTheme.labelLarge),
                const SizedBox(height: 8),
                _buildVariantSection(context, theme, productState),
                 if (_selectedVariant != null) ...[
                   const SizedBox(height: 12),
                   _buildPriceSummary(theme),
                 ],
                const SizedBox(height: 20),
                Text('Số lượng:', style: theme.textTheme.labelLarge),
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    IconButton.filled(
                      icon: const Icon(Icons.remove),
                      onPressed: _quantity > 1
                          ? () => setState(() => _quantity--)
                          : null,
                    ),
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 24),
                      child: Text(
                        '$_quantity',
                        style: theme.textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    IconButton.filled(
                      icon: const Icon(Icons.add),
                      onPressed: () => setState(() => _quantity++),
                    ),
                  ],
                ),
                const SizedBox(height: 24),
                AppButton(
                  isLoading: isLoading,
                  onPressed: _selectedVariant == null ||
                          isLoading ||
                          productState.variantStatus == ProductStatus.loading
                      ? null
                      : () => context.read<SalesOrderBloc>().add(
                          SalesOrderItemAdded(
                            orderId: widget.orderId,
                            productId: product.id,
                            productVariantId: _selectedVariant!.id,
                            quantity: _quantity,
                          ),
                        ),
                  child: const Text('Thêm vào đơn'),
                ),
                SizedBox(height: MediaQuery.of(context).viewInsets.bottom + 16),
              ],
            ),
          );
        },
      );
        },
      ),
    );
  }

  Widget _buildVariantSection(
    BuildContext context,
    ThemeData theme,
    ProductState state,
  ) {
    if (state.variantStatus == ProductStatus.loading) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.symmetric(vertical: 24),
          child: CircularProgressIndicator(),
        ),
      );
    }
    if (state.variantStatus == ProductStatus.failure) {
      return Center(
        child: Column(
          children: [
            Text(
              state.errorMessage ?? 'Lỗi tải phiên bản',
              style: TextStyle(color: theme.colorScheme.error),
            ),
            TextButton.icon(
              icon: const Icon(Icons.refresh),
              label: const Text('Thử lại'),
              onPressed: () => context.read<ProductBloc>().add(
                ProductVariantsRequested(_selectedProduct!.id),
              ),
            ),
          ],
        ),
      );
    }
    if (state.variants.isEmpty && state.variantStatus == ProductStatus.success) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 12),
          child: Text(
            'Sản phẩm này chưa có phiên bản nào',
            style: TextStyle(color: theme.colorScheme.onSurfaceVariant),
          ),
        ),
      );
    }
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: state.variants.map((variant) {
        final selected = _selectedVariant?.id == variant.id;
        final stock = (_selectedProduct!.totalStock / variant.quantityBaseUnit).floor();
        return FilterChip(
          label: Text(
            '${variant.name} — ${_formatPrice(variant.salePrice)} ($stock kho)',
          ),
          selected: selected,
          onSelected: stock > 0 ? (_) => setState(() => _selectedVariant = variant) : null,
        );
      }).toList(),
    );
  }

  Widget _buildPriceSummary(ThemeData theme) {
    final variant = _selectedVariant!;
    final total = variant.salePrice * _quantity;
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.primaryContainer.withAlpha(80),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            '${_formatPrice(variant.salePrice)} × $_quantity',
            style: theme.textTheme.bodyMedium,
          ),
          Text(
            _formatPrice(total),
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
              color: theme.colorScheme.primary,
            ),
          ),
        ],
      ),
    );
  }

  String _formatPrice(double price) {
    final n = price.toStringAsFixed(0).replaceAllMapped(
      RegExp(r'(\d)(?=(\d{3})+$)'),
      (m) => '${m[1]}.',
    );
    return '$n ₫';
  }
}
