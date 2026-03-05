part of 'sales_order_bloc.dart';

enum SalesOrderStatus { initial, loading, success, failure }

class SalesOrderState extends Equatable {
  final SalesOrderStatus listStatus;
  final List<SalesOrder> orders;
  final bool hasMore;
  final int currentPage;
  final SalesOrder? selectedOrder;
  final SalesOrderStatus detailStatus;
  final SalesOrderStatus actionStatus;
  final Invoice? generatedInvoice;
  final String? errorMessage;
  // Filters
  final OrderStatus? filterStatus;
  final DateTime? filterDateFrom;
  final DateTime? filterDateTo;
  final String searchTerm;

  const SalesOrderState({
    this.listStatus = SalesOrderStatus.initial,
    this.orders = const [],
    this.hasMore = false,
    this.currentPage = 1,
    this.selectedOrder,
    this.detailStatus = SalesOrderStatus.initial,
    this.actionStatus = SalesOrderStatus.initial,
    this.generatedInvoice,
    this.errorMessage,
    this.filterStatus,
    this.filterDateFrom,
    this.filterDateTo,
    this.searchTerm = '',
  });

  SalesOrderState copyWith({
    SalesOrderStatus? listStatus,
    List<SalesOrder>? orders,
    bool? hasMore,
    int? currentPage,
    SalesOrder? selectedOrder,
    SalesOrderStatus? detailStatus,
    SalesOrderStatus? actionStatus,
    Invoice? generatedInvoice,
    String? errorMessage,
    OrderStatus? filterStatus,
    DateTime? filterDateFrom,
    DateTime? filterDateTo,
    String? searchTerm,
    bool clearSelectedOrder = false,
    bool clearFilterStatus = false,
    bool clearFilterDateFrom = false,
    bool clearFilterDateTo = false,
    bool clearGeneratedInvoice = false,
  }) {
    return SalesOrderState(
      listStatus: listStatus ?? this.listStatus,
      orders: orders ?? this.orders,
      hasMore: hasMore ?? this.hasMore,
      currentPage: currentPage ?? this.currentPage,
      selectedOrder: clearSelectedOrder
          ? null
          : (selectedOrder ?? this.selectedOrder),
      detailStatus: detailStatus ?? this.detailStatus,
      actionStatus: actionStatus ?? this.actionStatus,
      generatedInvoice: clearGeneratedInvoice
          ? null
          : (generatedInvoice ?? this.generatedInvoice),
      errorMessage: errorMessage,
      filterStatus: clearFilterStatus
          ? null
          : (filterStatus ?? this.filterStatus),
      filterDateFrom: clearFilterDateFrom
          ? null
          : (filterDateFrom ?? this.filterDateFrom),
      filterDateTo: clearFilterDateTo
          ? null
          : (filterDateTo ?? this.filterDateTo),
      searchTerm: searchTerm ?? this.searchTerm,
    );
  }

  @override
  List<Object?> get props => [
    listStatus,
    orders,
    hasMore,
    currentPage,
    selectedOrder,
    detailStatus,
    actionStatus,
    generatedInvoice,
    errorMessage,
    filterStatus,
    filterDateFrom,
    filterDateTo,
    searchTerm,
  ];
}
