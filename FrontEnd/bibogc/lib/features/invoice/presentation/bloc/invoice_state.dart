part of 'invoice_bloc.dart';

enum InvoiceStatus { initial, loading, success, failure }

class InvoiceState extends Equatable {
  final InvoiceStatus listStatus;
  final List<Invoice> invoices;
  final bool hasMore;
  final int currentPage;
  final Invoice? selectedInvoice;
  final InvoiceStatus detailStatus;
  final String? errorMessage;
  final DateTime? filterDateFrom;
  final DateTime? filterDateTo;

  const InvoiceState({
    this.listStatus = InvoiceStatus.initial,
    this.invoices = const [],
    this.hasMore = false,
    this.currentPage = 1,
    this.selectedInvoice,
    this.detailStatus = InvoiceStatus.initial,
    this.errorMessage,
    this.filterDateFrom,
    this.filterDateTo,
  });

  InvoiceState copyWith({
    InvoiceStatus? listStatus,
    List<Invoice>? invoices,
    bool? hasMore,
    int? currentPage,
    Invoice? selectedInvoice,
    InvoiceStatus? detailStatus,
    String? errorMessage,
    DateTime? filterDateFrom,
    DateTime? filterDateTo,
    bool clearFilterDateFrom = false,
    bool clearFilterDateTo = false,
  }) {
    return InvoiceState(
      listStatus: listStatus ?? this.listStatus,
      invoices: invoices ?? this.invoices,
      hasMore: hasMore ?? this.hasMore,
      currentPage: currentPage ?? this.currentPage,
      selectedInvoice: selectedInvoice ?? this.selectedInvoice,
      detailStatus: detailStatus ?? this.detailStatus,
      errorMessage: errorMessage,
      filterDateFrom: clearFilterDateFrom
          ? null
          : (filterDateFrom ?? this.filterDateFrom),
      filterDateTo: clearFilterDateTo
          ? null
          : (filterDateTo ?? this.filterDateTo),
    );
  }

  @override
  List<Object?> get props => [
    listStatus,
    invoices,
    hasMore,
    currentPage,
    selectedInvoice,
    detailStatus,
    errorMessage,
    filterDateFrom,
    filterDateTo,
  ];
}
