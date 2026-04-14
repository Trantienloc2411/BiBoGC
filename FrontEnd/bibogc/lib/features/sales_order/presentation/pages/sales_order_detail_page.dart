import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/core/utils/thousands_separator_formatter.dart';
import 'package:bibogc/core/widgets/denomination_grid.dart';
import 'package:bibogc/core/config/app_routes.dart';
import 'package:bibogc/core/events/home_refresh_bus.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:bibogc/features/sales_order/presentation/bloc/sales_order_bloc.dart';
import 'package:bibogc/features/sales_order/presentation/widgets/add_item_bottom_sheet.dart';
import 'package:bibogc/features/sales_order/presentation/widgets/complete_order_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class SalesOrderDetailPage extends StatefulWidget {
  final String id;

  const SalesOrderDetailPage({super.key, required this.id});

  @override
  State<SalesOrderDetailPage> createState() => _SalesOrderDetailPageState();
}

class _SalesOrderDetailPageState extends State<SalesOrderDetailPage> {
  final Set<String> _knownItemIds = {};
  bool _initialLoadDone = false;

  @override
  void initState() {
    super.initState();
    context.read<SalesOrderBloc>().add(SalesOrderDetailRequested(widget.id));
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<SalesOrderBloc, SalesOrderState>(
      listenWhen: (prev, curr) {
        final hasFailure =
            curr.actionStatus != prev.actionStatus &&
            curr.actionStatus == SalesOrderStatus.failure;
        final hasNewGeneratedInvoice =
            curr.actionStatus == SalesOrderStatus.success &&
            curr.generatedInvoice != null &&
            curr.generatedInvoice != prev.generatedInvoice;
        return hasFailure || hasNewGeneratedInvoice;
      },
      listener: (context, state) {
        if (state.actionStatus == SalesOrderStatus.failure) {
          DialogUtils.showErrorDialog(
            context,
            message: state.errorMessage ?? 'Thao tác thất bại',
          );
        }
        if (state.actionStatus == SalesOrderStatus.success &&
            state.generatedInvoice != null) {
          DialogUtils.showSuccessDialog(
            context,
            message: 'Xuất hóa đơn thành công!',
            onPressed: () {
              HomeRefreshBus.trigger();
              context.go(AppRoutes.invoiceDetail(state.generatedInvoice!.id));
            },
          );
        }
      },
      builder: (context, state) {
        if (state.detailStatus == SalesOrderStatus.loading &&
            state.selectedOrder == null) {
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }

        final order = state.selectedOrder;
        if (order == null) {
          return Scaffold(
            appBar: AppBar(leading: const BackButton(), title: const Text('Chi tiết đơn hàng')),
            body: const Center(child: Text('Không tìm thấy đơn hàng')),
          );
        }

        return Scaffold(
          appBar: AppBar(
            leading: BackButton(onPressed: () => _handleBack(context)),
            title: Text(order.orderNumber),
            centerTitle: true,
          ),
          body: _buildBody(context, state, order),
        );
      },
    );
  }

  Widget _buildBody(
    BuildContext context,
    SalesOrderState state,
    SalesOrder order,
  ) {
    final isLoading = state.actionStatus == SalesOrderStatus.loading;
    final isDraft = order.status == OrderStatus.draft;

    return RefreshIndicator(
      onRefresh: () async {
        context.read<SalesOrderBloc>().add(SalesOrderDetailRequested(order.id));
      },
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16),
        child: LayoutBuilder(
          builder: (context, constraints) {
            final isWide = constraints.maxWidth >= 600;
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildOrderInfoCard(context, order),
                const SizedBox(height: 16),
                if (isWide)
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        flex: 3,
                        child: _buildItemsSection(
                          context, state, order, isDraft,
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        flex: 2,
                        child: _buildTotalsSection(context, order),
                      ),
                    ],
                  )
                else ...[
                  _buildItemsSection(context, state, order, isDraft),
                  const SizedBox(height: 16),
                  _buildTotalsSection(context, order),
                ],
                const SizedBox(height: 16),
                if (isDraft) _buildDraftActions(context, state, order, isLoading),
                const SizedBox(height: 12),
                _buildInvoiceButton(context, state, order, isLoading),
                const SizedBox(height: 32),
              ],
            );
          },
        ),
      ),
    );
  }

  Widget _buildOrderInfoCard(BuildContext context, SalesOrder order) {
    final theme = Theme.of(context);
    final dateFormat = DateFormat('dd/MM/yyyy HH:mm', 'vi_VN');

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    'Thông tin đơn hàng',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                _StatusChip(status: order.status),
              ],
            ),
            const Divider(height: 20),
            _InfoRow(
              label: 'Ngày tạo',
              value: dateFormat.format(order.orderDate.toLocal()),
            ),
            const SizedBox(height: 6),
            _InfoRow(
              label: 'Khách hàng',
              value: order.customerName ?? 'Khách lẻ',
            ),
            if (order.customerPhone != null) ...[
              const SizedBox(height: 6),
              _InfoRow(label: 'SĐT', value: order.customerPhone!),
            ],
            const SizedBox(height: 6),
            _InfoRow(label: 'Thanh toán', value: order.paymentMethodLabel),
            if (order.notes != null && order.notes!.isNotEmpty) ...[
              const SizedBox(height: 6),
              _InfoRow(label: 'Ghi chú', value: order.notes!),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildItemsSection(
    BuildContext context,
    SalesOrderState state,
    SalesOrder order,
    bool isDraft,
  ) {
    final theme = Theme.of(context);
    final isLoading = state.actionStatus == SalesOrderStatus.loading;

    final currentIds = order.items.map((e) => e.id).toSet();
    final newIds = _initialLoadDone
        ? currentIds.difference(_knownItemIds)
        : <String>{};
    _knownItemIds
      ..clear()
      ..addAll(currentIds);
    _initialLoadDone = true;

    final reversedItems = order.items.reversed.toList();

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    'Sản phẩm (${order.itemCount})',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                if (isDraft)
                  TextButton.icon(
                    icon: const Icon(Icons.add_shopping_cart, size: 18),
                    label: const Text('Thêm'),
                    style: TextButton.styleFrom(
                      padding: const EdgeInsets.symmetric(horizontal: 8),
                      visualDensity: VisualDensity.compact,
                    ),
                    onPressed: isLoading
                        ? null
                        : () => showModalBottomSheet(
                              context: context,
                              isScrollControlled: true,
                              shape: const RoundedRectangleBorder(
                                borderRadius: BorderRadius.vertical(
                                  top: Radius.circular(24),
                                ),
                              ),
                              builder: (ctx) => BlocProvider.value(
                                value: context.read<SalesOrderBloc>(),
                                child: AddItemBottomSheet(orderId: order.id),
                              ),
                            ),
                  ),
              ],
            ),
            const Divider(height: 20),
            if (order.items.isEmpty)
              const Center(
                child: Padding(
                  padding: EdgeInsets.all(16),
                  child: Text(
                    'Chưa có sản phẩm nào',
                    style: TextStyle(color: Colors.grey),
                  ),
                ),
              )
            else
              ConstrainedBox(
                constraints: const BoxConstraints(maxHeight: 300),
                child: ListView.builder(
                  shrinkWrap: true,
                  itemCount: reversedItems.length,
                  itemBuilder: (context, index) {
                    final item = reversedItems[index];
                    final isNew = newIds.contains(item.id);
                    Widget tile = _buildItemTile(
                      context, item, order, isDraft,
                    );
                    if (isDraft) {
                      tile = Dismissible(
                        key: Key(item.id),
                        direction: DismissDirection.endToStart,
                        background: Container(
                          color: Colors.red,
                          alignment: Alignment.centerRight,
                          padding: const EdgeInsets.only(right: 16),
                          child:
                              const Icon(Icons.delete, color: Colors.white),
                        ),
                        onDismissed: (_) {
                          context.read<SalesOrderBloc>().add(
                            SalesOrderItemRemoved(
                              orderId: order.id,
                              itemId: item.id,
                            ),
                          );
                        },
                        child: tile,
                      );
                    }
                    return _BlinkingTile(
                      key: ValueKey('blink_${item.id}'),
                      isNew: isNew,
                      child: tile,
                    );
                  },
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildItemTile(
    BuildContext context,
    SalesOrderItem item,
    SalesOrder order,
    bool isDraft,
  ) {
    return ListTile(
      contentPadding: EdgeInsets.zero,
      title: Text(
        item.productName,
        style: const TextStyle(fontWeight: FontWeight.w600),
      ),
      subtitle: Text(
        '${item.variantName} · ${item.sku}',
        style: const TextStyle(fontSize: 12),
      ),
      trailing: isDraft
          ? Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                GestureDetector(
                  onTap: () => _editQuantity(context, item, order),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        '${item.quantity} ${item.unit}',
                        style: const TextStyle(
                          fontWeight: FontWeight.w600,
                          color: Colors.blue,
                        ),
                      ),
                      Text(
                        CurrencyUtils.formatCurrency(item.lineTotal),
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 4),
                IconButton(
                  icon: const Icon(
                    Icons.remove_circle_outline,
                    color: Colors.red,
                  ),
                  iconSize: 20,
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(),
                  tooltip: 'Xóa',
                  onPressed: () {
                    context.read<SalesOrderBloc>().add(
                      SalesOrderItemRemoved(
                        orderId: order.id,
                        itemId: item.id,
                      ),
                    );
                  },
                ),
              ],
            )
          : Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text('${item.quantity} ${item.unit}'),
                Text(
                  CurrencyUtils.formatCurrency(item.lineTotal),
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
              ],
            ),
    );
  }

  void _editQuantity(
    BuildContext context,
    SalesOrderItem item,
    SalesOrder order,
  ) {
    int qty = item.quantity;
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(item.productName),
        content: StatefulBuilder(
          builder: (ctx, setState) => Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              IconButton(
                icon: const Icon(Icons.remove),
                onPressed: qty > 1 ? () => setState(() => qty--) : null,
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 20),
                child: Text(
                  '$qty',
                  style: Theme.of(ctx).textTheme.headlineSmall,
                ),
              ),
              IconButton(
                icon: const Icon(Icons.add),
                onPressed: () => setState(() => qty++),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(ctx).pop();
              context.read<SalesOrderBloc>().add(
                SalesOrderItemQuantityUpdated(
                  orderId: order.id,
                  itemId: item.id,
                  quantity: qty,
                ),
              );
            },
            child: const Text('Cập nhật'),
          ),
        ],
      ),
    );
  }

  Widget _buildTotalsSection(BuildContext context, SalesOrder order) {
    final theme = Theme.of(context);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            _TotalRow(
              label: 'Tạm tính',
              value: CurrencyUtils.formatCurrency(order.subTotal),
            ),
            const SizedBox(height: 8),
            _TotalRow(
              label: 'Giảm giá',
              value: '- ${CurrencyUtils.formatCurrency(order.discountAmount)}',
              valueColor: Colors.green,
            ),
            const SizedBox(height: 8),
            _TotalRow(
              label: 'Thuế',
              value: CurrencyUtils.formatCurrency(order.taxAmount),
            ),
            const Divider(height: 20),
            _TotalRow(
              label: 'Tổng cộng',
              value: CurrencyUtils.formatCurrency(order.totalAmount),
              bold: true,
              valueColor: theme.colorScheme.primary,
            ),
            if (order.status == OrderStatus.completed) ...[
              const SizedBox(height: 8),
              _TotalRow(
                label: 'Tiền nhận',
                value: CurrencyUtils.formatCurrency(order.amountPaid),
              ),
              const SizedBox(height: 8),
              _TotalRow(
                label: 'Tiền thối',
                value: CurrencyUtils.formatCurrency(order.changeAmount),
                valueColor: Colors.green,
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildDraftActions(
    BuildContext context,
    SalesOrderState state,
    SalesOrder order,
    bool isLoading,
  ) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        OutlinedButton.icon(
          icon: const Icon(Icons.discount_outlined),
          label: const Text('Áp dụng giảm giá'),
          onPressed: isLoading ? null : () => _applyDiscount(context, order),
        ),
        const SizedBox(height: 8),
        ElevatedButton.icon(
          icon: const Icon(Icons.check_circle_outline),
          label: const Text('Hoàn thành đơn'),
          style: ElevatedButton.styleFrom(backgroundColor: Colors.green),
          onPressed: isLoading || order.items.isEmpty
              ? null
              : () => showDialog(
                  context: context,
                  builder: (ctx) => BlocProvider.value(
                    value: context.read<SalesOrderBloc>(),
                    child: CompleteOrderDialog(order: order),
                  ),
                ),
        ),
        const SizedBox(height: 8),
        OutlinedButton.icon(
          icon: const Icon(Icons.cancel_outlined, color: Colors.red),
          label: const Text('Hủy đơn', style: TextStyle(color: Colors.red)),
          style: OutlinedButton.styleFrom(
            side: const BorderSide(color: Colors.red),
          ),
          onPressed: isLoading ? null : () => _cancelOrder(context, order),
        ),
      ],
    );
  }

  Widget _buildInvoiceButton(
    BuildContext context,
    SalesOrderState state,
    SalesOrder order,
    bool isLoading,
  ) {
    if (order.invoiceNumber != null) {
      return OutlinedButton.icon(
        icon: const Icon(Icons.receipt_long),
        label: Text('Xem hóa đơn ${order.invoiceNumber}'),
        onPressed: order.invoiceId == null
            ? null
            : () => context.push(AppRoutes.invoiceDetail(order.invoiceId!)),
      );
    }

    if (order.status == OrderStatus.completed) {
      return ElevatedButton.icon(
        icon: const Icon(Icons.receipt),
        label: const Text('Xuất hóa đơn'),
        onPressed: isLoading
            ? null
            : () => context.read<SalesOrderBloc>().add(
                SalesOrderInvoiceGenerated(order.id),
              ),
      );
    }

    return const SizedBox.shrink();
  }

  void _handleBack(BuildContext context) {
    if (context.canPop()) {
      context.pop();
      return;
    }
    context.go(AppRoutes.home);
  }

  void _applyDiscount(BuildContext context, SalesOrder order) {
    final controller = TextEditingController();

    void addDenomination(int denom) {
      final current =
          double.tryParse(controller.text.replaceAll(',', '')) ?? 0;
      final newValue = current + denom;
      final formatted = CurrencyUtils.formatNumber(newValue);
      controller.text = formatted;
      controller.selection =
          TextSelection.collapsed(offset: formatted.length);
    }

    void resetAmount() {
      controller.text = '';
    }

    showDialog(
      context: context,
      builder: (ctx) {
        final theme = Theme.of(ctx);
        return Dialog(
          insetPadding: const EdgeInsets.symmetric(
            horizontal: 32,
            vertical: 40,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          child: ConstrainedBox(
            constraints: const BoxConstraints(minWidth: 460, maxWidth: 520),
            child: SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(28, 24, 28, 20),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      'Áp dụng giảm giá',
                      style: theme.textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                        fontSize: 20,
                      ),
                    ),
                    const SizedBox(height: 20),
                    TextField(
                      controller: controller,
                      keyboardType: TextInputType.number,
                      inputFormatters: [ThousandsSeparatorFormatter()],
                      autofocus: true,
                      decoration: InputDecoration(
                        labelText: 'Số tiền giảm',
                        suffixText: '₫',
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
                    DenominationGrid(
                      onTap: addDenomination,
                      onReset: resetAmount,
                    ),
                    const SizedBox(height: 20),
                    Row(
                      children: [
                        Expanded(
                          child: TextButton(
                            onPressed: () => Navigator.of(ctx).pop(),
                            style: TextButton.styleFrom(
                              minimumSize: const Size(0, 52),
                              foregroundColor: theme.colorScheme.onSurface
                                  .withAlpha(150),
                            ),
                            child: const Text('Hủy'),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          flex: 2,
                          child: FilledButton(
                            style: FilledButton.styleFrom(
                              minimumSize: const Size(0, 52),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(10),
                              ),
                            ),
                            onPressed: () {
                              final amount = double.tryParse(
                                    controller.text.replaceAll(',', ''),
                                  ) ??
                                  0;
                              Navigator.of(ctx).pop();
                              context.read<SalesOrderBloc>().add(
                                SalesOrderDiscountApplied(
                                  orderId: order.id,
                                  discountAmount: amount,
                                ),
                              );
                            },
                            child: const Text('Áp dụng'),
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
    );
  }

  void _cancelOrder(BuildContext context, SalesOrder order) {
    final controller = TextEditingController();
    DialogUtils.showConfirmationDialog(
      context,
      title: 'Hủy đơn hàng',
      message: 'Bạn có chắc chắn muốn hủy đơn hàng ${order.orderNumber}?',
      confirmText: 'Hủy đơn',
      onConfirm: () {
        showDialog(
          context: context,
          builder: (ctx) => AlertDialog(
            title: const Text('Lý do hủy (tùy chọn)'),
            content: TextField(
              controller: controller,
              decoration: const InputDecoration(
                hintText: 'Nhập lý do...',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(ctx).pop(),
                child: const Text('Bỏ qua'),
              ),
              ElevatedButton(
                style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
                onPressed: () {
                  Navigator.of(ctx).pop();
                  context.read<SalesOrderBloc>().add(
                    SalesOrderCancelled(
                      orderId: order.id,
                      reason: controller.text.trim(),
                    ),
                  );
                },
                child: const Text('Xác nhận hủy'),
              ),
            ],
          ),
        );
      },
    );
  }

}

class _StatusChip extends StatelessWidget {
  final OrderStatus status;
  const _StatusChip({required this.status});

  @override
  Widget build(BuildContext context) {
    final (label, bgColor, textColor) = switch (status) {
      OrderStatus.draft => (
        'Đang soạn',
        Colors.orange.shade100,
        Colors.orange.shade800,
      ),
      OrderStatus.completed => (
        'Hoàn thành',
        Colors.green.shade100,
        Colors.green.shade800,
      ),
      OrderStatus.cancelled => (
        'Đã hủy',
        Colors.red.shade100,
        Colors.red.shade800,
      ),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: textColor,
          fontWeight: FontWeight.w600,
          fontSize: 12,
        ),
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  final String label;
  final String value;

  const _InfoRow({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 90,
          child: Text(
            label,
            style: theme.textTheme.bodySmall?.copyWith(color: Colors.grey[600]),
          ),
        ),
        Expanded(child: Text(value, style: theme.textTheme.bodyMedium)),
      ],
    );
  }
}

class _TotalRow extends StatelessWidget {
  final String label;
  final String value;
  final bool bold;
  final Color? valueColor;

  const _TotalRow({
    required this.label,
    required this.value,
    this.bold = false,
    this.valueColor,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: bold
              ? theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                )
              : theme.textTheme.bodyMedium,
        ),
        Text(
          value,
          style: bold
              ? theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: valueColor,
                )
              : theme.textTheme.bodyMedium?.copyWith(color: valueColor),
        ),
      ],
    );
  }
}

class _BlinkingTile extends StatefulWidget {
  final Widget child;
  final bool isNew;

  const _BlinkingTile({super.key, required this.child, this.isNew = false});

  @override
  State<_BlinkingTile> createState() => _BlinkingTileState();
}

class _BlinkingTileState extends State<_BlinkingTile>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<Color?> _colorAnimation;
  bool _shouldAnimate = false;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    );
    _colorAnimation = ColorTween(
      begin: Colors.green.withValues(alpha: 0.3),
      end: Colors.green.withValues(alpha: 0.0),
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));

    if (widget.isNew) {
      _shouldAnimate = true;
      _controller.repeat(reverse: true);
      Future.delayed(const Duration(seconds: 3), _stopAnimation);
    }
  }

  void _stopAnimation() {
    if (!mounted) return;
    _controller.forward().then((_) {
      if (mounted) setState(() => _shouldAnimate = false);
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (!_shouldAnimate) return widget.child;
    return AnimatedBuilder(
      animation: _colorAnimation,
      builder: (context, child) => DecoratedBox(
        decoration: BoxDecoration(
          color: _colorAnimation.value,
          borderRadius: BorderRadius.circular(8),
        ),
        child: child,
      ),
      child: widget.child,
    );
  }
}
