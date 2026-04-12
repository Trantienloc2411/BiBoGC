import 'package:bibogc/core/di/injection.dart';
import 'package:bibogc/core/network/dio_client.dart';
import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/core/widgets/app_button.dart';
import 'package:bibogc/core/widgets/barcode_scanner_sheet.dart';
import 'package:bibogc/features/product/data/models/product_model.dart';
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
  int _step = 0;
  bool _scanLoading = false;
  late final TextEditingController _quantityController;

  @override
  void initState() {
    super.initState();
    _quantityController = TextEditingController(text: '1');
  }

  @override
  void dispose() {
    _quantityController.dispose();
    super.dispose();
  }

  Future<void> _openBarcodeScanner(BuildContext context) async {
    // Capture context-dependent objects before any async gap
    final productBloc = context.read<ProductBloc>();

    final scanned = await showModalBottomSheet<String>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.black,
      builder: (_) => const BarcodeScannerSheet(),
    );

    if (scanned == null || !mounted) return;

    setState(() => _scanLoading = true);
    try {
      final dio = getIt<DioClient>().dio;

      // Look up the variant by barcode
      final variantResponse = await dio.get(
        '/api/products/variants/by-barcode',
        queryParameters: {'code': scanned},
      );
      final variantModel = ProductVariantModel.fromJson(
        variantResponse.data as Map<String, dynamic>,
      );

      // Fetch the full product for stock info
      final productResponse = await dio.get(
        '/api/products/${variantModel.productId}',
      );
      final productModel = ProductModel.fromJson(
        productResponse.data as Map<String, dynamic>,
      );

      final product = productModel.toEntity();
      final variant = variantModel.toEntity();

      if (!mounted) return;

      // Load all variants for this product (for the chip list in step 2)
      productBloc.add(ProductVariantsRequested(product.id));

      setState(() {
        _selectedProduct = product;
        _selectedVariant = variant;
        _quantity = 1;
        _quantityController.text = '1';
        _step = 1;
        _scanLoading = false;
      });
    } catch (_) {
      if (mounted) {
        setState(() => _scanLoading = false);
        if (!context.mounted) return;
        DialogUtils.showErrorDialog(
          context,
          title: 'Quét mã thất bại',
          message: 'Không tìm thấy sản phẩm với mã vạch: $scanned',
        );
      }
    }
  }

  void _setQuantity(int value) {
    final maxStock = _getMaxStock();
    final clamped = value.clamp(1, maxStock > 0 ? maxStock : 1);
    setState(() => _quantity = clamped);
    _quantityController.text = '$clamped';
    _quantityController.selection = TextSelection.fromPosition(
      TextPosition(offset: _quantityController.text.length),
    );
  }

  int _getMaxStock() {
    if (_selectedProduct == null || _selectedVariant == null) return 0;
    return (_selectedProduct!.totalStock / _selectedVariant!.quantityBaseUnit)
        .floor();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<ProductBloc>()..add(const ProductsStarted()),
      child: Builder(
        builder: (providerContext) => _buildSheet(providerContext),
      ),
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
                        _quantity = 1;
                        _quantityController.text = '1';
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
                  if (_step == 0)
                    _scanLoading
                        ? const Padding(
                            padding: EdgeInsets.all(12),
                            child: SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            ),
                          )
                        : IconButton(
                            icon: const Icon(Icons.qr_code_scanner),
                            tooltip: 'Quét mã vạch',
                            onPressed: () => _openBarcodeScanner(context),
                          ),
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
                          ? () => _setQuantity(_quantity - 1)
                          : null,
                    ),
                    const SizedBox(width: 8),
                    SizedBox(
                      width: 80,
                      child: TextField(
                        controller: _quantityController,
                        keyboardType: TextInputType.number,
                        textAlign: TextAlign.center,
                        style: theme.textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                        decoration: const InputDecoration(
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 8,
                            vertical: 8,
                          ),
                          border: OutlineInputBorder(),
                          isDense: true,
                        ),
                        onChanged: (value) {
                          final parsed = int.tryParse(value);
                          if (parsed != null && parsed >= 1) {
                            final maxStock = _getMaxStock();
                            final clamped = maxStock > 0
                                ? parsed.clamp(1, maxStock)
                                : parsed.clamp(1, parsed);
                            setState(() => _quantity = clamped);
                            if (clamped != parsed) {
                              _quantityController.text = '$clamped';
                              _quantityController.selection =
                                  TextSelection.fromPosition(
                                TextPosition(
                                  offset: _quantityController.text.length,
                                ),
                              );
                            }
                          }
                        },
                        onSubmitted: (value) {
                          final parsed = int.tryParse(value);
                          if (parsed == null || parsed < 1) {
                            _setQuantity(1);
                          } else {
                            _setQuantity(parsed);
                          }
                        },
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton.filled(
                      icon: const Icon(Icons.add),
                      onPressed: () {
                        final maxStock = _getMaxStock();
                        if (maxStock <= 0 || _quantity < maxStock) {
                          _setQuantity(_quantity + 1);
                        }
                      },
                    ),
                  ],
                ),
                if (_selectedVariant != null) ...[
                  const SizedBox(height: 4),
                  Text(
                    'Tồn kho: ${_getMaxStock()}',
                    textAlign: TextAlign.center,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
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
