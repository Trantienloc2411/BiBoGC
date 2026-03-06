import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/core/widgets/app_button.dart';
import 'package:bibogc/features/sales_order/presentation/bloc/sales_order_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class CreateOrderBottomSheet extends StatefulWidget {
  const CreateOrderBottomSheet({super.key});

  @override
  State<CreateOrderBottomSheet> createState() => _CreateOrderBottomSheetState();
}

class _CreateOrderBottomSheetState extends State<CreateOrderBottomSheet> {
  final _customerNameController = TextEditingController();
  final _customerPhoneController = TextEditingController();
  final _notesController = TextEditingController();
  int _paymentMethod = 1; // 1=Cash, 2=QRPayment
  String? _lastCreatedId;

  @override
  void dispose() {
    _customerNameController.dispose();
    _customerPhoneController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return BlocListener<SalesOrderBloc, SalesOrderState>(
      listenWhen: (prev, curr) =>
          curr.actionStatus != prev.actionStatus &&
          curr.actionStatus != SalesOrderStatus.initial,
      listener: (context, state) {
        if (state.actionStatus == SalesOrderStatus.success &&
            state.selectedOrder != null &&
            state.selectedOrder!.id != _lastCreatedId) {
          _lastCreatedId = state.selectedOrder!.id;
          Navigator.of(context).pop(state.selectedOrder);
        } else if (state.actionStatus == SalesOrderStatus.failure) {
          DialogUtils.showErrorDialog(
            context,
            message: state.errorMessage ?? 'Tạo đơn thất bại',
          );
        }
      },
      child: BlocBuilder<SalesOrderBloc, SalesOrderState>(
        buildWhen: (prev, curr) => curr.actionStatus != prev.actionStatus,
        builder: (context, state) {
          final isLoading = state.actionStatus == SalesOrderStatus.loading;

          return DraggableScrollableSheet(
            initialChildSize: 0.7,
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
                        Text(
                          'Tạo đơn hàng mới',
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
                  Expanded(
                    child: ListView(
                      controller: scrollController,
                      padding: const EdgeInsets.all(16),
                      children: [
                        // Payment method
                        Text(
                          'Phương thức thanh toán',
                          style: theme.textTheme.labelLarge,
                        ),
                        const SizedBox(height: 8),
                        Row(
                          children: [
                            Expanded(
                              child: _PaymentChip(
                                label: 'Tiền mặt',
                                icon: Icons.money,
                                value: 1,
                                selected: _paymentMethod == 1,
                                onTap: () =>
                                    setState(() => _paymentMethod = 1),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: _PaymentChip(
                                label: 'Chuyển khoản',
                                icon: Icons.account_balance,
                                value: 2,
                                selected: _paymentMethod == 2,
                                onTap: () => setState(
                                  () => _paymentMethod = 2,
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 20),
                        TextField(
                          controller: _customerNameController,
                          decoration: const InputDecoration(
                            labelText: 'Tên khách hàng (tùy chọn)',
                            prefixIcon: Icon(Icons.person_outline),
                            border: OutlineInputBorder(),
                          ),
                        ),
                        const SizedBox(height: 14),
                        TextField(
                          controller: _customerPhoneController,
                          keyboardType: TextInputType.phone,
                          decoration: const InputDecoration(
                            labelText: 'Số điện thoại (tùy chọn)',
                            prefixIcon: Icon(Icons.phone_outlined),
                            border: OutlineInputBorder(),
                          ),
                        ),
                        const SizedBox(height: 14),
                        TextField(
                          controller: _notesController,
                          maxLines: 3,
                          decoration: const InputDecoration(
                            labelText: 'Ghi chú (tùy chọn)',
                            prefixIcon: Icon(Icons.note_outlined),
                            alignLabelWithHint: true,
                            border: OutlineInputBorder(),
                          ),
                        ),
                        const SizedBox(height: 24),
                        AppButton(
                          isLoading: isLoading,
                          onPressed: isLoading
                              ? null
                              : () => context.read<SalesOrderBloc>().add(
                                  SalesOrderCreateRequested(
                                    paymentMethod: _paymentMethod,
                                    customerName: _customerNameController.text
                                        .trim(),
                                    customerPhone: _customerPhoneController.text
                                        .trim(),
                                    notes: _notesController.text.trim(),
                                  ),
                                ),
                          child: const Text('Tạo đơn'),
                        ),
                        SizedBox(
                          height: MediaQuery.of(context).viewInsets.bottom + 16,
                        ),
                      ],
                    ),
                  ),
                ],
              );
            },
          );
        },
      ),
    );
  }
}

class _PaymentChip extends StatelessWidget {
  final String label;
  final IconData icon;
  final int value;
  final bool selected;
  final VoidCallback onTap;

  const _PaymentChip({
    required this.label,
    required this.icon,
    required this.value,
    required this.selected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 150),
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
        decoration: BoxDecoration(
          color: selected
              ? theme.colorScheme.primaryContainer
              : Colors.grey[100],
          borderRadius: BorderRadius.circular(10),
          border: Border.all(
            color: selected ? theme.colorScheme.primary : Colors.transparent,
            width: 2,
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              icon,
              size: 18,
              color: selected ? theme.colorScheme.primary : Colors.grey[600],
            ),
            const SizedBox(width: 8),
            Text(
              label,
              style: TextStyle(
                fontWeight: FontWeight.w600,
                color: selected ? theme.colorScheme.primary : Colors.grey[700],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
