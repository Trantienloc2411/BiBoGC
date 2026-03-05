import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:bibogc/features/sales_order/presentation/bloc/sales_order_bloc.dart';
import 'package:bibogc/features/sales_order/presentation/widgets/create_order_bottom_sheet.dart';
import 'package:bibogc/features/sales_order/presentation/widgets/sales_order_card.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/config/app_routes.dart';

class SalesOrdersPage extends StatelessWidget {
  const SalesOrdersPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<SalesOrderBloc, SalesOrderState>(
      listenWhen: (prev, curr) => curr.listStatus == SalesOrderStatus.failure,
      listener: (context, state) {
        if (state.errorMessage != null) {
          DialogUtils.showErrorDialog(context, message: state.errorMessage!);
        }
      },
      child: Scaffold(
        appBar: AppBar(title: const Text('Đơn hàng'), centerTitle: true),
        floatingActionButton: FloatingActionButton(
          onPressed: () => _openCreateOrder(context),
          child: const Icon(Icons.add),
        ),
        body: Column(
          children: [
            _SearchBar(),
            _FilterBar(),
            const Expanded(child: _OrderList()),
          ],
        ),
      ),
    );
  }

  void _openCreateOrder(BuildContext context) {
    final bloc = context.read<SalesOrderBloc>();
    showModalBottomSheet<SalesOrder>(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (_) => BlocProvider.value(
        value: bloc,
        child: const CreateOrderBottomSheet(),
      ),
    ).then((newOrder) {
      if (newOrder != null && context.mounted) {
        context.push(AppRoutes.salesOrderDetail(newOrder.id));
      }
    });
  }
}

class _SearchBar extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 4),
      child: TextField(
        decoration: const InputDecoration(
          prefixIcon: Icon(Icons.search),
          hintText: 'Tìm đơn hàng...',
          border: OutlineInputBorder(),
          contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        ),
        onChanged: (value) =>
            context.read<SalesOrderBloc>().add(SalesOrdersSearchChanged(value)),
      ),
    );
  }
}

class _FilterBar extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final filters = <OrderStatus?>[
      null,
      OrderStatus.draft,
      OrderStatus.completed,
      OrderStatus.cancelled,
    ];
    final labels = ['Tất cả', 'Đang soạn', 'Hoàn thành', 'Đã hủy'];

    return BlocBuilder<SalesOrderBloc, SalesOrderState>(
      buildWhen: (prev, curr) => prev.filterStatus != curr.filterStatus,
      builder: (context, state) {
        return SizedBox(
          height: 48,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
            itemCount: filters.length,
            separatorBuilder: (_, __) => const SizedBox(width: 8),
            itemBuilder: (context, index) {
              final filterValue = filters[index];
              final isSelected = state.filterStatus == filterValue;
              return FilterChip(
                label: Text(labels[index]),
                selected: isSelected,
                onSelected: (_) => context.read<SalesOrderBloc>().add(
                  SalesOrdersFilterChanged(status: filterValue),
                ),
              );
            },
          ),
        );
      },
    );
  }
}

class _OrderList extends StatelessWidget {
  const _OrderList();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<SalesOrderBloc, SalesOrderState>(
      builder: (context, state) {
        if (state.listStatus == SalesOrderStatus.loading &&
            state.orders.isEmpty) {
          return const Center(child: CircularProgressIndicator());
        }

        if (state.listStatus == SalesOrderStatus.failure &&
            state.orders.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Icon(Icons.error_outline, size: 48, color: Colors.grey),
                const SizedBox(height: 8),
                Text(state.errorMessage ?? 'Tải dữ liệu thất bại'),
                const SizedBox(height: 16),
                TextButton(
                  onPressed: () => context.read<SalesOrderBloc>().add(
                    const SalesOrdersStarted(),
                  ),
                  child: const Text('Thử lại'),
                ),
              ],
            ),
          );
        }

        if (state.orders.isEmpty) {
          return const Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.receipt_long_outlined, size: 64, color: Colors.grey),
                SizedBox(height: 12),
                Text(
                  'Chưa có đơn hàng nào',
                  style: TextStyle(color: Colors.grey),
                ),
              ],
            ),
          );
        }

        return RefreshIndicator(
          onRefresh: () async {
            context.read<SalesOrderBloc>().add(const SalesOrdersStarted());
          },
          child: NotificationListener<ScrollNotification>(
            onNotification: (notification) {
              if (notification is ScrollEndNotification &&
                  notification.metrics.extentAfter < 200) {
                context.read<SalesOrderBloc>().add(const SalesOrdersLoadMore());
              }
              return false;
            },
            child: ListView.builder(
              padding: const EdgeInsets.only(bottom: 100, top: 4),
              itemCount: state.orders.length + (state.hasMore ? 1 : 0),
              itemBuilder: (context, index) {
                if (index == state.orders.length) {
                  return const Center(
                    child: Padding(
                      padding: EdgeInsets.all(16),
                      child: CircularProgressIndicator(),
                    ),
                  );
                }
                final order = state.orders[index];
                return SalesOrderCard(
                  order: order,
                  onTap: () =>
                      context.push(AppRoutes.salesOrderDetail(order.id)),
                );
              },
            ),
          ),
        );
      },
    );
  }
}
