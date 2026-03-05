import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/features/invoice/presentation/bloc/invoice_bloc.dart';
import 'package:bibogc/features/invoice/presentation/widgets/invoice_card.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/config/app_routes.dart';

class InvoicesPage extends StatelessWidget {
  const InvoicesPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<InvoiceBloc, InvoiceState>(
      listenWhen: (prev, curr) => curr.listStatus == InvoiceStatus.failure,
      listener: (context, state) {
        if (state.errorMessage != null) {
          DialogUtils.showErrorDialog(context, message: state.errorMessage!);
        }
      },
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Hóa đơn'),
          centerTitle: true,
          actions: [
            IconButton(
              icon: const Icon(Icons.filter_list),
              tooltip: 'Lọc theo ngày',
              onPressed: () => _openDateFilter(context),
            ),
          ],
        ),
        body: const _InvoiceList(),
      ),
    );
  }

  void _openDateFilter(BuildContext context) async {
    final now = DateTime.now();
    final picked = await showDateRangePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: now,
      initialDateRange: DateTimeRange(
        start: now.subtract(const Duration(days: 30)),
        end: now,
      ),
    );
    if (picked != null && context.mounted) {
      context.read<InvoiceBloc>().add(
        InvoicesFilterChanged(dateFrom: picked.start, dateTo: picked.end),
      );
    }
  }
}

class _InvoiceList extends StatelessWidget {
  const _InvoiceList();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<InvoiceBloc, InvoiceState>(
      builder: (context, state) {
        if (state.listStatus == InvoiceStatus.loading &&
            state.invoices.isEmpty) {
          return const Center(child: CircularProgressIndicator());
        }

        if (state.listStatus == InvoiceStatus.failure &&
            state.invoices.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Icon(Icons.error_outline, size: 48, color: Colors.grey),
                const SizedBox(height: 8),
                Text(state.errorMessage ?? 'Tải dữ liệu thất bại'),
                const SizedBox(height: 16),
                TextButton(
                  onPressed: () =>
                      context.read<InvoiceBloc>().add(const InvoicesStarted()),
                  child: const Text('Thử lại'),
                ),
              ],
            ),
          );
        }

        if (state.invoices.isEmpty) {
          return const Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.description_outlined, size: 64, color: Colors.grey),
                SizedBox(height: 12),
                Text(
                  'Chưa có hóa đơn nào',
                  style: TextStyle(color: Colors.grey),
                ),
              ],
            ),
          );
        }

        return RefreshIndicator(
          onRefresh: () async {
            context.read<InvoiceBloc>().add(const InvoicesStarted());
          },
          child: NotificationListener<ScrollNotification>(
            onNotification: (notification) {
              if (notification is ScrollEndNotification &&
                  notification.metrics.extentAfter < 200) {
                context.read<InvoiceBloc>().add(const InvoicesLoadMore());
              }
              return false;
            },
            child: ListView.builder(
              padding: const EdgeInsets.only(bottom: 16, top: 4),
              itemCount: state.invoices.length + (state.hasMore ? 1 : 0),
              itemBuilder: (context, index) {
                if (index == state.invoices.length) {
                  return const Center(
                    child: Padding(
                      padding: EdgeInsets.all(16),
                      child: CircularProgressIndicator(),
                    ),
                  );
                }
                final invoice = state.invoices[index];
                return InvoiceCard(
                  invoice: invoice,
                  onTap: () =>
                      context.push(AppRoutes.invoiceDetail(invoice.id)),
                );
              },
            ),
          ),
        );
      },
    );
  }
}
