import 'package:bibogc/core/di/injection.dart';
import 'package:bibogc/features/invoice/data/datasources/invoice_remote_data_source.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:bibogc/features/invoice/presentation/bloc/invoice_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import 'package:printing/printing.dart';

/// Full-page version (used from InvoicesPage).
/// Also exposes [InvoiceDetailContent] as a widget reusable in a bottom sheet.
class InvoiceDetailPage extends StatefulWidget {
  final String id;

  const InvoiceDetailPage({super.key, required this.id});

  @override
  State<InvoiceDetailPage> createState() => _InvoiceDetailPageState();
}

class _InvoiceDetailPageState extends State<InvoiceDetailPage> {
  @override
  void initState() {
    super.initState();
    context.read<InvoiceBloc>().add(InvoiceDetailRequested(widget.id));
  }

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<InvoiceBloc, InvoiceState>(
      buildWhen: (prev, curr) =>
          prev.detailStatus != curr.detailStatus ||
          prev.selectedInvoice != curr.selectedInvoice,
      builder: (context, state) {
        if (state.detailStatus == InvoiceStatus.loading &&
            state.selectedInvoice == null) {
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }

        final invoice = state.selectedInvoice;
        if (invoice == null) {
          return Scaffold(
            appBar: AppBar(title: const Text('Hóa đơn')),
            body: Center(
              child: Text(state.errorMessage ?? 'Không tìm thấy hóa đơn'),
            ),
          );
        }

        return Scaffold(
          appBar: AppBar(
            leading: const BackButton(),
            title: Text(invoice.invoiceNumber),
            centerTitle: true,
          ),
          body: InvoiceDetailContent(invoice: invoice),
        );
      },
    );
  }
}

/// Reusable content widget – used both in the page and as a bottom sheet from SalesOrderDetailPage.
class InvoiceDetailContent extends StatefulWidget {
  final Invoice invoice;

  const InvoiceDetailContent({super.key, required this.invoice});

  @override
  State<InvoiceDetailContent> createState() => _InvoiceDetailContentState();
}

class _InvoiceDetailContentState extends State<InvoiceDetailContent> {
  bool _isPrinting = false;

  Future<void> _handlePrint() async {
    setState(() => _isPrinting = true);
    try {
      final bytes = await getIt<InvoiceRemoteDataSource>()
          .exportInvoicePdf(widget.invoice.id);
      await Printing.sharePdf(
        bytes: bytes,
        filename: '${widget.invoice.invoiceNumber}.pdf',
      );
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Không thể xuất hóa đơn. Vui lòng thử lại.')),
        );
      }
    } finally {
      if (mounted) setState(() => _isPrinting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final invoice = widget.invoice;
    final currencyFormat = NumberFormat.currency(locale: 'vi_VN', symbol: '₫');
    final dateFormat = DateFormat('dd/MM/yyyy HH:mm', 'vi_VN');
    final theme = Theme.of(context);

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Handle for bottom sheet
          Center(
            child: Container(
              width: 40,
              height: 4,
              margin: const EdgeInsets.only(bottom: 16),
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
          // Store info
          _SectionCard(
            title: 'Cửa hàng',
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  invoice.storeName,
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(invoice.storeAddress, style: theme.textTheme.bodySmall),
                const SizedBox(height: 4),
                Text(invoice.storePhone, style: theme.textTheme.bodySmall),
                if (invoice.storeTaxCode != null) ...[
                  const SizedBox(height: 4),
                  Text(
                    'MST: ${invoice.storeTaxCode}',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ],
            ),
          ),
          const SizedBox(height: 12),
          // Invoice meta
          _SectionCard(
            title: invoice.invoiceNumber,
            subtitle: dateFormat.format(invoice.invoiceDate.toLocal()),
            child: Column(
              children: [
                _InfoRow(label: 'Đơn hàng', value: invoice.orderNumber),
                if (invoice.customerName != null) ...[
                  const SizedBox(height: 6),
                  _InfoRow(label: 'Khách hàng', value: invoice.customerName!),
                ],
                if (invoice.customerPhone != null) ...[
                  const SizedBox(height: 6),
                  _InfoRow(label: 'SĐT', value: invoice.customerPhone!),
                ],
              ],
            ),
          ),
          const SizedBox(height: 12),
          // Items table
          _SectionCard(
            title: 'Sản phẩm',
            child: Column(
              children: [
                // Header
                Row(
                  children: [
                    const SizedBox(
                      width: 28,
                      child: Text(
                        'STT',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const Expanded(
                      flex: 3,
                      child: Text(
                        'Tên SP',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const SizedBox(
                      width: 30,
                      child: Text(
                        'SL',
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const SizedBox(
                      width: 70,
                      child: Text(
                        'Đơn giá',
                        textAlign: TextAlign.right,
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const SizedBox(
                      width: 72,
                      child: Text(
                        'Thành tiền',
                        textAlign: TextAlign.right,
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                  ],
                ),
                const Divider(),
                ...invoice.items.asMap().entries.map((entry) {
                  final i = entry.key;
                  final item = entry.value;
                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 4),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        SizedBox(
                          width: 28,
                          child: Text(
                            '${i + 1}',
                            style: const TextStyle(fontSize: 12),
                          ),
                        ),
                        Expanded(
                          flex: 3,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                item.productName,
                                style: const TextStyle(
                                  fontSize: 12,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                              Text(
                                item.variantName,
                                style: const TextStyle(
                                  fontSize: 11,
                                  color: Colors.grey,
                                ),
                              ),
                            ],
                          ),
                        ),
                        SizedBox(
                          width: 30,
                          child: Text(
                            '${item.quantity}',
                            textAlign: TextAlign.center,
                            style: const TextStyle(fontSize: 12),
                          ),
                        ),
                        SizedBox(
                          width: 70,
                          child: Text(
                            currencyFormat.format(item.unitPrice),
                            textAlign: TextAlign.right,
                            style: const TextStyle(fontSize: 11),
                          ),
                        ),
                        SizedBox(
                          width: 72,
                          child: Text(
                            currencyFormat.format(item.lineTotal),
                            textAlign: TextAlign.right,
                            style: const TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ],
                    ),
                  );
                }),
              ],
            ),
          ),
          const SizedBox(height: 12),
          // Totals
          _SectionCard(
            title: 'Tổng kết',
            child: Column(
              children: [
                _TotalRow(
                  label: 'Tạm tính',
                  value: currencyFormat.format(invoice.subTotal),
                ),
                const SizedBox(height: 6),
                _TotalRow(
                  label: 'Giảm giá',
                  value: '- ${currencyFormat.format(invoice.discountAmount)}',
                  valueColor: Colors.green,
                ),
                const SizedBox(height: 6),
                _TotalRow(
                  label: 'Thuế',
                  value: currencyFormat.format(invoice.taxAmount),
                ),
                const Divider(height: 16),
                _TotalRow(
                  label: 'Tổng cộng',
                  value: currencyFormat.format(invoice.grandTotal),
                  bold: true,
                  valueColor: theme.colorScheme.primary,
                ),
                const SizedBox(height: 6),
                _TotalRow(
                  label: 'Tiền nhận',
                  value: currencyFormat.format(invoice.amountPaid),
                ),
                const SizedBox(height: 6),
                _TotalRow(
                  label: 'Tiền thối',
                  value: currencyFormat.format(invoice.changeAmount),
                  valueColor: Colors.green,
                ),
                const SizedBox(height: 6),
                _TotalRow(
                  label: 'Thanh toán',
                  value: invoice.paymentMethodLabel,
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              icon: _isPrinting
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.print),
              label: Text(_isPrinting ? 'Đang xuất...' : 'In / Chia sẻ hóa đơn'),
              onPressed: _isPrinting ? null : _handlePrint,
            ),
          ),
          const SizedBox(height: 32),
        ],
      ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  final String title;
  final String? subtitle;
  final Widget child;

  const _SectionCard({required this.title, this.subtitle, required this.child});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: theme.textTheme.titleSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            if (subtitle != null)
              Text(
                subtitle!,
                style: theme.textTheme.bodySmall?.copyWith(color: Colors.grey),
              ),
            const Divider(height: 16),
            child,
          ],
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
          width: 85,
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
              ? theme.textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                )
              : theme.textTheme.bodyMedium?.copyWith(color: Colors.grey[700]),
        ),
        Text(
          value,
          style: bold
              ? theme.textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: valueColor,
                )
              : theme.textTheme.bodyMedium?.copyWith(color: valueColor),
        ),
      ],
    );
  }
}
