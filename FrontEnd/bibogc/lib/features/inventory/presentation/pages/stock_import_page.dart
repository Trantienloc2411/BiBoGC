import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/features/inventory/presentation/bloc/stock_import_bloc.dart';
import 'package:bibogc/features/inventory/presentation/bloc/stock_import_event.dart';
import 'package:bibogc/features/inventory/presentation/bloc/stock_import_state.dart';
import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:bibogc/features/product/presentation/bloc/product_bloc.dart';
import 'package:bibogc/features/suppliers/presentation/bloc/supplier_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/di/injection.dart';
import '../../../../core/widgets/app_button.dart';

class StockImportPage extends StatelessWidget {
  final Product? initialProduct;

  const StockImportPage({super.key, this.initialProduct});

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(create: (_) => getIt<StockImportBloc>()),
        BlocProvider(
          create: (_) => getIt<SupplierBloc>()..add(SuppliersStarted()),
        ),
        BlocProvider(
          create: (_) => getIt<ProductBloc>()..add(const ProductsStarted()),
        ),
      ],
      child: StockImportView(initialProduct: initialProduct),
    );
  }
}

class StockImportView extends StatefulWidget {
  final Product? initialProduct;

  const StockImportView({super.key, this.initialProduct});

  @override
  State<StockImportView> createState() => _StockImportViewState();
}

class _StockImportViewState extends State<StockImportView> {
  final _batchFormKey = GlobalKey<FormState>();
  final _batchNumberController = TextEditingController();
  final _quantityController = TextEditingController(text: '0');
  final _costPriceController = TextEditingController(text: '0');
  final _unitPriceController = TextEditingController(text: '0');
  final _notesController = TextEditingController();

  DateTime? _manufacturingDate;
  DateTime? _expirationDate;

  @override
  void initState() {
    super.initState();
    if (widget.initialProduct != null) {
      context.read<StockImportBloc>().add(
        StockImportProductSelected(
          productId: widget.initialProduct!.id,
          productName: widget.initialProduct!.name,
        ),
      );
    }
  }

  @override
  void dispose() {
    _batchNumberController.dispose();
    _quantityController.dispose();
    _costPriceController.dispose();
    _unitPriceController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<StockImportBloc, StockImportState>(
      listener: (context, state) {
        if (state.status == StockImportStatus.success) {
          DialogUtils.showSuccessDialog(
            context,
            message: 'Nhập kho thành công.',
          );
        } else if (state.status == StockImportStatus.failure &&
            state.errorMessage != null) {
          DialogUtils.showErrorDialog(context, message: state.errorMessage!);
        }
      },
      builder: (context, state) {
        return Scaffold(
          appBar: AppBar(
            leading: BackButton(onPressed: () => Navigator.of(context).pop()),
            title: const Text('Nhập kho sản phẩm'),
            centerTitle: true,
          ),
          body: Column(
            children: [
              _buildStepIndicator(context, state.stepIndex),
              const SizedBox(height: 8),
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: _buildStepContent(context, state),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildStepIndicator(BuildContext context, int stepIndex) {
    final theme = Theme.of(context);
    final steps = ['Nhà cung cấp', 'Sản phẩm', 'Lô hàng & giá vốn'];

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Row(
        children: List.generate(steps.length, (index) {
          final isActive = index == stepIndex;
          final isCompleted = index < stepIndex;
          final color = isCompleted
              ? theme.colorScheme.primary
              : (isActive
                    ? theme.colorScheme.primary
                    : theme.colorScheme.outlineVariant);

          return Expanded(
            child: Column(
              children: [
                Row(
                  children: [
                    Container(
                      width: 24,
                      height: 24,
                      decoration: BoxDecoration(
                        color: isCompleted
                            ? color
                            : (isActive
                                  ? theme.colorScheme.primary
                                  : Colors.transparent),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: color, width: 2),
                      ),
                      child: isCompleted
                          ? Icon(
                              Icons.check,
                              size: 16,
                              color: theme.colorScheme.onPrimary,
                            )
                          : isActive
                          ? Center(
                              child: Text(
                                '${index + 1}',
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: theme.colorScheme.onPrimary,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            )
                          : Center(
                              child: Text(
                                '${index + 1}',
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: color,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                    ),
                    if (index < steps.length - 1) ...[
                      const SizedBox(width: 8),
                      Expanded(
                        child: Container(
                          height: 2,
                          color: index < stepIndex
                              ? theme.colorScheme.primary
                              : theme.colorScheme.outlineVariant,
                        ),
                      ),
                    ],
                  ],
                ),
                const SizedBox(height: 4),
                Text(
                  steps[index],
                  textAlign: TextAlign.center,
                  style: theme.textTheme.bodySmall?.copyWith(
                    fontWeight: isActive ? FontWeight.bold : FontWeight.w400,
                  ),
                ),
              ],
            ),
          );
        }),
      ),
    );
  }

  Widget _buildStepContent(BuildContext context, StockImportState state) {
    switch (state.stepIndex) {
      case 0:
        return _buildSupplierStep(context, state);
      case 1:
        return _buildProductStep(context, state);
      default:
        return _buildBatchStep(context, state);
    }
  }

  Widget _buildSupplierStep(BuildContext context, StockImportState state) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Bước 1: Chọn nhà cung cấp',
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Kiểm tra xem nhà cung cấp đã tồn tại hay chưa. Nếu chưa, hãy tạo mới trước khi nhập kho.',
          style: theme.textTheme.bodyMedium,
        ),
        const SizedBox(height: 16),
        Card(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Nhà cung cấp hiện tại',
                  style: theme.textTheme.labelMedium,
                ),
                const SizedBox(height: 8),
                Text(
                  state.supplierName ?? 'Chưa chọn',
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 16),
                AppButton(
                  onPressed: () => _openSupplierPicker(context),
                  child: const Text('Chọn nhà cung cấp'),
                ),
              ],
            ),
          ),
        ),
        const Spacer(),
        AppButton(
          onPressed: state.supplierId == null
              ? null
              : () {
                  context.read<StockImportBloc>().add(
                    const StockImportStepChanged(1),
                  );
                },
          child: const Text('Tiếp tục'),
        ),
      ],
    );
  }

  Widget _buildProductStep(BuildContext context, StockImportState state) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Bước 2: Chọn sản phẩm',
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Nếu sản phẩm chưa tồn tại, hãy tạo mới trước. Sau đó chọn sản phẩm cần nhập kho.',
          style: theme.textTheme.bodyMedium,
        ),
        const SizedBox(height: 16),
        Card(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text('Sản phẩm hiện tại', style: theme.textTheme.labelMedium),
                const SizedBox(height: 8),
                Text(
                  state.productName ?? 'Chưa chọn',
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 16),
                AppButton(
                  onPressed: () => _openProductPicker(context),
                  child: const Text('Chọn sản phẩm'),
                ),
              ],
            ),
          ),
        ),
        const Spacer(),
        Row(
          children: [
            Expanded(
              child: AppButton(
                onPressed: () {
                  context.read<StockImportBloc>().add(
                    const StockImportStepChanged(0),
                  );
                },
                child: const Text('Quay lại'),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: AppButton(
                onPressed: state.productId == null
                    ? null
                    : () {
                        context.read<StockImportBloc>().add(
                          const StockImportStepChanged(2),
                        );
                      },
                child: const Text('Tiếp tục'),
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildBatchStep(BuildContext context, StockImportState state) {
    final theme = Theme.of(context);
    final isLoading = state.status == StockImportStatus.loading;

    return Form(
      key: _batchFormKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Bước 3: Tạo lô hàng & nhập kho',
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Thông tin sẽ được dùng để tạo lô hàng mới (batch) và ghi nhận giao dịch nhập kho.',
            style: theme.textTheme.bodyMedium,
          ),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
              child: Column(
                children: [
                  TextFormField(
                    controller: _batchNumberController,
                    decoration: const InputDecoration(
                      labelText: 'Mã lô hàng (batch number) *',
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Vui lòng nhập mã lô hàng';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: _buildDateField(
                          context,
                          label: 'Ngày sản xuất *',
                          value: _manufacturingDate,
                          onTap: () =>
                              _pickDate(context, isManufacturing: true),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildDateField(
                          context,
                          label: 'Hạn sử dụng *',
                          value: _expirationDate,
                          onTap: () =>
                              _pickDate(context, isManufacturing: false),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _quantityController,
                    decoration: const InputDecoration(
                      labelText: 'Số lượng nhập kho *',
                    ),
                    keyboardType: TextInputType.number,
                    validator: (value) {
                      final parsed = int.tryParse(value ?? '');
                      if (parsed == null || parsed <= 0) {
                        return 'Số lượng phải > 0';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _costPriceController,
                    decoration: const InputDecoration(
                      labelText: 'Giá vốn / đơn vị *',
                    ),
                    keyboardType: const TextInputType.numberWithOptions(
                      decimal: true,
                    ),
                    validator: (value) {
                      final parsed = double.tryParse(
                        (value ?? '').replaceAll(',', '.'),
                      );
                      if (parsed == null || parsed <= 0) {
                        return 'Giá vốn phải > 0';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _unitPriceController,
                    decoration: const InputDecoration(
                      labelText: 'Đơn giá nhập (unit price) *',
                    ),
                    keyboardType: const TextInputType.numberWithOptions(
                      decimal: true,
                    ),
                    validator: (value) {
                      final parsed = double.tryParse(
                        (value ?? '').replaceAll(',', '.'),
                      );
                      if (parsed == null || parsed <= 0) {
                        return 'Đơn giá phải > 0';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _notesController,
                    decoration: const InputDecoration(
                      labelText: 'Ghi chú (tuỳ chọn)',
                    ),
                    maxLines: 2,
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: AppButton(
                  onPressed: isLoading
                      ? null
                      : () {
                          context.read<StockImportBloc>().add(
                            const StockImportStepChanged(1),
                          );
                        },
                  child: const Text('Quay lại'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: AppButton(
                  isLoading: isLoading,
                  onPressed: isLoading ? null : () => _submit(context),
                  child: const Text('Hoàn tất nhập kho'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildDateField(
    BuildContext context, {
    required String label,
    required DateTime? value,
    required VoidCallback onTap,
  }) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: InputDecorator(
        decoration: InputDecoration(labelText: label),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              value == null
                  ? 'Chọn ngày'
                  : '${value.day.toString().padLeft(2, '0')}/${value.month.toString().padLeft(2, '0')}/${value.year}',
              style: theme.textTheme.bodyMedium,
            ),
            const Icon(Icons.calendar_today, size: 18),
          ],
        ),
      ),
    );
  }

  Future<void> _pickDate(
    BuildContext context, {
    required bool isManufacturing,
  }) async {
    final now = DateTime.now();
    final initialDate = isManufacturing
        ? (_manufacturingDate ?? now)
        : (_expirationDate ?? now.add(const Duration(days: 365)));

    final picked = await showDatePicker(
      context: context,
      initialDate: initialDate,
      firstDate: DateTime(now.year - 10),
      lastDate: DateTime(now.year + 10),
    );

    if (picked == null) return;

    setState(() {
      if (isManufacturing) {
        _manufacturingDate = picked;
      } else {
        _expirationDate = picked;
      }
    });
  }

  Future<void> _submit(BuildContext context) async {
    if (_manufacturingDate == null || _expirationDate == null) {
      DialogUtils.showErrorDialog(
        context,
        message: 'Vui lòng chọn ngày sản xuất và hạn sử dụng.',
      );
      return;
    }

    if (!_batchFormKey.currentState!.validate()) return;

    final quantity = int.parse(_quantityController.text);
    final costPrice = double.parse(
      _costPriceController.text.replaceAll(',', '.'),
    );
    final unitPrice = double.parse(
      _unitPriceController.text.replaceAll(',', '.'),
    );

    context.read<StockImportBloc>().add(
      StockImportSubmitted(
        batchNumber: _batchNumberController.text,
        manufacturingDate: _manufacturingDate!,
        expirationDate: _expirationDate!,
        quantity: quantity,
        costPrice: costPrice,
        unitPrice: unitPrice,
        notes: _notesController.text.isNotEmpty ? _notesController.text : null,
      ),
    );
  }

  Future<void> _openSupplierPicker(BuildContext context) async {
    // Capture blocs BEFORE opening the sheet — bottom sheets are separate
    // overlay routes and lose access to ancestor BlocProviders.
    final supplierBloc = context.read<SupplierBloc>();
    final stockImportBloc = context.read<StockImportBloc>();

    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (ctx) {
        final theme = Theme.of(ctx);
        return MultiBlocProvider(
          providers: [
            BlocProvider.value(value: supplierBloc),
            BlocProvider.value(value: stockImportBloc),
          ],
          child: SafeArea(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Chọn nhà cung cấp',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  SizedBox(
                    height: 360,
                    child: BlocBuilder<SupplierBloc, SupplierState>(
                      builder: (context, state) {
                        if (state.suppliers.isEmpty &&
                            state.status == SupplierStatus.loading) {
                          return const Center(
                            child: CircularProgressIndicator(),
                          );
                        }
                        if (state.suppliers.isEmpty) {
                          return const Center(
                            child: Text('Chưa có nhà cung cấp nào'),
                          );
                        }
                        return ListView.builder(
                          itemCount: state.suppliers.length,
                          itemBuilder: (context, index) {
                            final supplier = state.suppliers[index];
                            return ListTile(
                              title: Text(supplier.name),
                              subtitle: supplier.contactName != null
                                  ? Text(supplier.contactName!)
                                  : null,
                              onTap: () {
                                context.read<StockImportBloc>().add(
                                  StockImportSupplierSelected(
                                    supplierId: supplier.id,
                                    supplierName: supplier.name,
                                  ),
                                );
                                Navigator.of(ctx).pop();
                              },
                            );
                          },
                        );
                      },
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }

  Future<void> _openProductPicker(BuildContext context) async {
    // Capture blocs BEFORE opening the sheet.
    final productBloc = context.read<ProductBloc>();
    final stockImportBloc = context.read<StockImportBloc>();

    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (ctx) {
        final theme = Theme.of(ctx);
        return MultiBlocProvider(
          providers: [
            BlocProvider.value(value: productBloc),
            BlocProvider.value(value: stockImportBloc),
          ],
          child: SafeArea(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Chọn sản phẩm',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  SizedBox(
                    height: 360,
                    child: BlocBuilder<ProductBloc, ProductState>(
                      builder: (context, state) {
                        if (state.products.isEmpty &&
                            state.status == ProductStatus.loading) {
                          return const Center(
                            child: CircularProgressIndicator(),
                          );
                        }
                        if (state.products.isEmpty) {
                          return const Center(
                            child: Text('Chưa có sản phẩm nào'),
                          );
                        }
                        return ListView.builder(
                          itemCount: state.products.length,
                          itemBuilder: (context, index) {
                            final product = state.products[index];
                            return ListTile(
                              title: Text(product.name),
                              onTap: () {
                                context.read<StockImportBloc>().add(
                                  StockImportProductSelected(
                                    productId: product.id,
                                    productName: product.name,
                                  ),
                                );
                                Navigator.of(ctx).pop();
                              },
                            );
                          },
                        );
                      },
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }
}
