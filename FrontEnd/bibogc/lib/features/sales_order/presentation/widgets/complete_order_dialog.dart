import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/core/utils/thousands_separator_formatter.dart';
import 'package:bibogc/core/widgets/app_button.dart';
import 'package:bibogc/core/widgets/denomination_grid.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:bibogc/features/sales_order/presentation/bloc/sales_order_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

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
    _amountPaid = 0;
    _amountController = TextEditingController(text: '');
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

  /// Positive = change to give back. Negative = shortfall (customer underpaid).
  double get _changeAmount => _amountPaid - widget.order.totalAmount;

  void _addDenomination(int denomination) {
    final current =
        double.tryParse(_amountController.text.replaceAll(',', '')) ?? 0;
    final newValue = current + denomination;
    final formatted = CurrencyUtils.formatNumber(newValue);
    _amountController.text = formatted;
    _amountController.selection =
        TextSelection.collapsed(offset: formatted.length);
  }

  void _resetAmount() {
    _amountController.text = '';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

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

          return Dialog(
            insetPadding: const EdgeInsets.symmetric(
              horizontal: 24,
              vertical: 32,
            ),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            child: ConstrainedBox(
              constraints: const BoxConstraints(minWidth: 520, maxWidth: 560),
              child: SingleChildScrollView(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(28, 24, 28, 24),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      // Title
                      Text(
                        'Xác nhận thanh toán',
                        style: theme.textTheme.titleLarge?.copyWith(
                          fontWeight: FontWeight.bold,
                          fontSize: 20,
                        ),
                      ),
                      const SizedBox(height: 20),

                      // Total row
                      _InfoRow(
                        label: 'Tổng cộng',
                        value: CurrencyUtils.formatCurrency(
                          widget.order.totalAmount,
                        ),
                        valueStyle: theme.textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: theme.colorScheme.primary,
                        ),
                      ),
                      const SizedBox(height: 16),

                      // Tiền nhận input
                      TextField(
                        controller: _amountController,
                        keyboardType: TextInputType.number,
                        inputFormatters: [ThousandsSeparatorFormatter()],
                        style: theme.textTheme.titleMedium,
                        decoration: InputDecoration(
                          labelText: 'Tiền nhận',
                          suffixText: 'đ',
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 18,
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),

                      // Denomination grid + reset button
                      DenominationGrid(
                        onTap: _addDenomination,
                        onReset: _resetAmount,
                      ),
                      const SizedBox(height: 16),

                      // Tiền thối
                      _ChangeDisplay(
                        changeAmount: _changeAmount,
                        isDark: isDark,
                      ),
                      const SizedBox(height: 24),

                      // Action buttons
                      Row(
                        children: [
                          Expanded(
                            child: TextButton(
                              onPressed: isLoading
                                  ? null
                                  : () => Navigator.of(context).pop(),
                              style: TextButton.styleFrom(
                                minimumSize: const Size(0, 52),
                                foregroundColor: theme.colorScheme.onSurface
                                    .withAlpha(isDark ? 140 : 160),
                              ),
                              child: const Text('Hủy'),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            flex: 2,
                            child: AppButton(
                              isLoading: isLoading,
                              height: 52,
                              onPressed:
                                  _amountPaid < widget.order.totalAmount ||
                                          isLoading
                                      ? null
                                      : () =>
                                          context.read<SalesOrderBloc>().add(
                                            SalesOrderCompleted(
                                              orderId: widget.order.id,
                                              amountPaid: _amountPaid,
                                            ),
                                          ),
                              child: const Text('Xác nhận thanh toán'),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

/// Displays "Tiền thối" with theme-aware colors.
/// Green for positive change, red for shortfall.
class _ChangeDisplay extends StatelessWidget {
  final double changeAmount;
  final bool isDark;

  const _ChangeDisplay({required this.changeAmount, required this.isDark});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isShortfall = changeAmount < 0;
    final isExact = changeAmount == 0;

    final Color valueColor;
    final Color bgColor;

    if (isShortfall) {
      valueColor = theme.colorScheme.error;
      bgColor = theme.colorScheme.errorContainer.withAlpha(isDark ? 60 : 80);
    } else if (isExact) {
      valueColor = theme.colorScheme.onSurface.withAlpha(isDark ? 140 : 160);
      bgColor = theme.colorScheme.surfaceContainerHighest.withAlpha(
        isDark ? 60 : 80,
      );
    } else {
      valueColor = isDark ? const Color(0xFF81C784) : Colors.green.shade700;
      bgColor = isDark
          ? Colors.green.withAlpha(40)
          : Colors.green.shade50;
    }

    final displayAmount = changeAmount.abs();
    final label = isShortfall ? 'Còn thiếu' : 'Tiền thối';
    final valueText = isShortfall
        ? '- ${CurrencyUtils.formatCurrency(displayAmount)}'
        : CurrencyUtils.formatCurrency(displayAmount);

    return AnimatedContainer(
      duration: const Duration(milliseconds: 200),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(10),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: theme.textTheme.bodyMedium),
          Text(
            valueText,
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
              color: valueColor,
            ),
          ),
        ],
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
