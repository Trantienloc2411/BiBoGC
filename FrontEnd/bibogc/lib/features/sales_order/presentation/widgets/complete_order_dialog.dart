import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/core/widgets/app_button.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:bibogc/features/sales_order/presentation/bloc/sales_order_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';

class CompleteOrderDialog extends StatefulWidget {
  final SalesOrder order;

  const CompleteOrderDialog({super.key, required this.order});

  @override
  State<CompleteOrderDialog> createState() => _CompleteOrderDialogState();
}

class _CompleteOrderDialogState extends State<CompleteOrderDialog> {
  late final TextEditingController _amountController;
  double _amountPaid = 0;

  @override
  void initState() {
    super.initState();
    _amountPaid = widget.order.totalAmount;
    _amountController = TextEditingController(
      text: widget.order.totalAmount.toStringAsFixed(0),
    );
    _amountController.addListener(() {
      final value =
          double.tryParse(_amountController.text.replaceAll(',', '')) ?? 0;
      if (value != _amountPaid) {
        setState(() => _amountPaid = value);
      }
    });
  }

  @override
  void dispose() {
    _amountController.dispose();
    super.dispose();
  }

  double get _changeAmount =>
      (_amountPaid - widget.order.totalAmount).clamp(0, double.infinity);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final currencyFormat = NumberFormat.currency(locale: 'vi_VN', symbol: '₫');

    return BlocListener<SalesOrderBloc, SalesOrderState>(
      listenWhen: (prev, curr) =>
          curr.actionStatus != prev.actionStatus &&
          curr.actionStatus != SalesOrderStatus.initial,
      listener: (context, state) {
        if (state.actionStatus == SalesOrderStatus.success) {
          DialogUtils.showSuccessDialog(
            context,
            message: 'Hoàn thành đơn hàng thành công. Bạn có thể xuất hóa đơn.',
            onPressed: () => Navigator.of(context).pop(true),
          );
        } else if (state.actionStatus == SalesOrderStatus.failure) {
          DialogUtils.showErrorDialog(
            context,
            message: state.errorMessage ?? 'Thao tác thất bại',
          );
        }
      },
      child: BlocBuilder<SalesOrderBloc, SalesOrderState>(
        buildWhen: (prev, curr) => curr.actionStatus != prev.actionStatus,
        builder: (context, state) {
          final isLoading = state.actionStatus == SalesOrderStatus.loading;

          return AlertDialog(
            title: const Text('Xác nhận thanh toán'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _InfoRow(
                  label: 'Tổng cộng',
                  value: currencyFormat.format(widget.order.totalAmount),
                  valueStyle: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: theme.colorScheme.primary,
                  ),
                ),
                const SizedBox(height: 16),
                TextField(
                  controller: _amountController,
                  keyboardType: const TextInputType.numberWithOptions(
                    decimal: true,
                  ),
                  decoration: const InputDecoration(
                    labelText: 'Tiền nhận',
                    suffixText: '₫',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                AnimatedContainer(
                  duration: const Duration(milliseconds: 200),
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.green.shade50,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: _InfoRow(
                    label: 'Tiền thối',
                    value: currencyFormat.format(_changeAmount),
                    valueStyle: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: Colors.green.shade700,
                    ),
                  ),
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: isLoading ? null : () => Navigator.of(context).pop(),
                child: const Text('Hủy'),
              ),
              AppButton(
                isLoading: isLoading,
                height: 44,
                onPressed: _amountPaid < widget.order.totalAmount || isLoading
                    ? null
                    : () => context.read<SalesOrderBloc>().add(
                        SalesOrderCompleted(
                          orderId: widget.order.id,
                          amountPaid: _amountPaid,
                        ),
                      ),
                child: const Text('Xác nhận thanh toán'),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  final String label;
  final String value;
  final TextStyle? valueStyle;

  const _InfoRow({required this.label, required this.value, this.valueStyle});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: Theme.of(context).textTheme.bodyMedium),
        Text(value, style: valueStyle),
      ],
    );
  }
}
