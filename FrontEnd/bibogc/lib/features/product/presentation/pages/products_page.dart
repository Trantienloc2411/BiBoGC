import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/features/inventory/presentation/pages/stock_import_page.dart';
import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:bibogc/features/product/presentation/bloc/product_bloc.dart';
import 'package:bibogc/features/product/presentation/widgets/product_card.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/di/injection.dart';

class ProductsPage extends StatelessWidget {
  const ProductsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<ProductBloc>()..add(const ProductsStarted()),
      child: const ProductsView(),
    );
  }
}

class ProductsView extends StatelessWidget {
  const ProductsView({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: BackButton(onPressed: () => Navigator.of(context).pop()),
        title: const Text('Sản phẩm'),
        centerTitle: true,
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          // TODO: Navigate to product create screen.
          DialogUtils.showSuccessDialog(
            context,
            message: 'Flow tạo sản phẩm sẽ được triển khai sau.',
          );
        },
        child: const Icon(Icons.add),
      ),
      body: Column(
        children: [
          _buildSearchBar(context),
          Expanded(
            child: BlocBuilder<ProductBloc, ProductState>(
              builder: (context, state) {
                if (state.status == ProductStatus.loading &&
                    state.products.isEmpty) {
                  return const Center(child: CircularProgressIndicator());
                }

                if (state.status == ProductStatus.failure &&
                    state.products.isEmpty) {
                  return Center(
                    child: Text(
                      state.errorMessage ?? 'Lỗi tải danh sách sản phẩm',
                    ),
                  );
                }

                if (state.products.isEmpty) {
                  return const Center(child: Text('Chưa có sản phẩm nào'));
                }

                return ListView.builder(
                  padding: const EdgeInsets.only(bottom: 88),
                  itemCount: state.products.length,
                  itemBuilder: (context, index) {
                    final product = state.products[index];
                    return ProductCard(
                      product: product,
                      onTap: () => _openProductDetail(context, product),
                      trailing: IconButton(
                        icon: const Icon(Icons.inventory_2_outlined),
                        onPressed: () => _openImportStock(context, product),
                      ),
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

  Widget _buildSearchBar(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: TextField(
        decoration: const InputDecoration(
          prefixIcon: Icon(Icons.search),
          hintText: 'Tìm theo tên, SKU...',
        ),
        onChanged: (value) {
          context.read<ProductBloc>().add(ProductsSearchChanged(value));
        },
      ),
    );
  }

  void _openProductDetail(BuildContext context, Product product) {
    // For now just show a bottom sheet preview using the existing card.
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (ctx) {
        return DraggableScrollableSheet(
          expand: false,
          builder: (_, scrollController) {
            return SingleChildScrollView(
              controller: scrollController,
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          product.name,
                          style: Theme.of(ctx).textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      IconButton(
                        onPressed: () => Navigator.of(ctx).pop(),
                        icon: const Icon(Icons.close),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  if (product.description.isNotEmpty) ...[
                    Text(
                      product.description,
                      style: Theme.of(ctx).textTheme.bodyMedium,
                    ),
                    const SizedBox(height: 16),
                  ],
                  ProductCard(product: product),
                ],
              ),
            );
          },
        );
      },
    );
  }

  void _openImportStock(BuildContext context, Product product) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => StockImportPage(initialProduct: product),
      ),
    );
  }
}
