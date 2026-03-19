import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:bibogc/features/suppliers/presentation/bloc/supplier_bloc.dart';
import 'package:bibogc/features/suppliers/presentation/widgets/supplier_form_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';

class SupplierDetailPage extends StatelessWidget {
  final String id;

  const SupplierDetailPage({super.key, required this.id});

  @override
  Widget build(BuildContext context) {
    // Bloc is provided by suppliers route ShellRoute.
    // Request detail when the page is pushed.
    return SupplierDetailView(id: id);
  }
}

class SupplierDetailView extends StatefulWidget {
  final String id;

  const SupplierDetailView({super.key, required this.id});

  @override
  State<SupplierDetailView> createState() => _SupplierDetailViewState();
}

class _SupplierDetailViewState extends State<SupplierDetailView> {
  @override
  void initState() {
    super.initState();
    context.read<SupplierBloc>().add(SupplierDetailRequested(widget.id));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: BackButton(onPressed: () => Navigator.of(context).pop()),
        title: const Text('Chi tiết nhà cung cấp'),
        centerTitle: true,
        actions: [
          BlocBuilder<SupplierBloc, SupplierState>(
            builder: (context, state) {
              if (state.selectedSupplier != null) {
                return IconButton(
                  icon: const Icon(Icons.edit),
                  onPressed: () =>
                      _editSupplier(context, state.selectedSupplier!),
                );
              }
              return const SizedBox.shrink();
            },
          ),
        ],
      ),
      body: BlocListener<SupplierBloc, SupplierState>(
        listenWhen: (previous, current) =>
            previous.actionStatus != current.actionStatus,
        listener: (context, state) {
          if (state.actionStatus == SupplierActionStatus.failure) {
            DialogUtils.showErrorDialog(
              context,
              message: state.errorMessage ?? "Có lỗi xảy ra",
            );
          } else if (state.actionStatus == SupplierActionStatus.success) {
            DialogUtils.showSuccessDialog(
              context,
              message: state.actionMessage ?? "Thành công",
            );
          }
        },
        child: BlocBuilder<SupplierBloc, SupplierState>(
          builder: (context, state) {
            if (state.detailStatus == SupplierStatus.loading) {
              return const Center(child: CircularProgressIndicator());
            } else if (state.detailStatus == SupplierStatus.failure) {
              return Center(
                child: Text(state.errorMessage ?? 'Lỗi tải dữ liệu'),
              );
            } else if (state.selectedSupplier == null) {
              return const Center(child: Text('Không tìm thấy nhà cung cấp'));
            }

            final supplier = state.selectedSupplier!;
            return SingleChildScrollView(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildHeader(context, supplier),
                  const SizedBox(height: 24),
                  const Text(
                    'Lịch sử giao dịch',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 8),
                  _buildTransactionHistory(supplier),
                ],
              ),
            );
          },
        ),
      ),
    );
  }

  Widget _buildHeader(BuildContext context, Supplier supplier) {
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    return Card(
      elevation: 1,
      color: theme.colorScheme.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Row(
              children: [
                CircleAvatar(
                  radius: 30,
                  backgroundColor: theme.colorScheme.primary.withAlpha(32),
                  child: Text(
                    supplier.name.isNotEmpty
                        ? supplier.name[0].toUpperCase()
                        : '?',
                    style: TextStyle(
                      fontSize: 24,
                      color: theme.colorScheme.primary,
                    ),
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        supplier.name,
                        style: textTheme.titleLarge?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: supplier.isActive
                              ? theme.colorScheme.primary.withAlpha(26)
                              : theme.colorScheme.error.withAlpha(26),
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(
                            color: supplier.isActive
                                ? theme.colorScheme.primary
                                : theme.colorScheme.error,
                            width: 0.5,
                          ),
                        ),
                        child: Text(
                          supplier.isActive ? 'Hoạt động' : 'Ngưng hoạt động',
                          style: TextStyle(
                            color: supplier.isActive
                                ? theme.colorScheme.primary
                                : theme.colorScheme.error,
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const Divider(height: 32),
            _buildInfoRow(Icons.person, "Liên hệ", supplier.contactName ?? "-"),
            const SizedBox(height: 12),
            _buildInfoRow(
              Icons.phone,
              "Điện thoại",
              supplier.contactPhone ?? "-",
            ),
            const SizedBox(height: 12),
            _buildInfoRow(
              Icons.location_on,
              "Địa chỉ",
              supplier.address ?? "-",
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 20, color: Colors.grey[600]),
        const SizedBox(width: 12),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              label,
              style: const TextStyle(fontSize: 12, color: Colors.grey),
            ),
            Text(value, style: const TextStyle(fontSize: 16)),
          ],
        ),
      ],
    );
  }

  Widget _buildTransactionHistory(Supplier supplier) {
    if (supplier.transactions == null || supplier.transactions!.isEmpty) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: Text(
            "Chưa có giao dịch nào",
            style: TextStyle(color: Colors.grey),
          ),
        ),
      );
    }

    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: supplier.transactions!.length,
      itemBuilder: (context, index) {
        final transaction = supplier.transactions![index];
        final isImport =
            transaction.transactionType == 'Import' ||
            transaction.transactionType == 'Purchase'; // Adjust based on API

        final typeLabel = isImport ? 'Nhập hàng' : 'Bán hàng';
        final typeColor = isImport ? Colors.green : Colors.orange;
        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          child: ListTile(
            leading: CircleAvatar(
              backgroundColor: typeColor.withAlpha(30),
              child: Icon(
                isImport ? Icons.arrow_downward : Icons.arrow_upward,
                color: typeColor,
                size: 20,
              ),
            ),
            title: Text(
              transaction.productName ?? 'Sản phẩm không xác định',
              style: const TextStyle(fontWeight: FontWeight.w600),
            ),
            subtitle: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (transaction.batchNumber != null)
                  Text(
                    'Lô: ${transaction.batchNumber}',
                    style: const TextStyle(fontSize: 12),
                  ),
                Text(
                  DateFormat('dd/MM/yyyy HH:mm').format(transaction.transactionDate.toLocal()),
                  style: const TextStyle(fontSize: 12, color: Colors.grey),
                ),
              ],
            ),
            isThreeLine: transaction.batchNumber != null,
            trailing: Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: typeColor.withAlpha(30),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: typeColor.withAlpha(100)),
              ),
              child: Text(
                typeLabel,
                style: TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                  color: typeColor,
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  Future<void> _editSupplier(BuildContext context, Supplier supplier) async {
    final supplierBloc = context.read<SupplierBloc>();
    final result = await showDialog<Supplier>(
      context: context,
      builder: (ctx) => SupplierFormDialog(supplier: supplier),
    );

    if (result != null) {
      supplierBloc.add(SupplierUpdated(result));
    }
  }
}
