part of 'supplier_bloc.dart';

enum SupplierStatus { initial, loading, success, failure }

enum SupplierActionStatus { initial, loading, success, failure }

class SupplierState extends Equatable {
  final SupplierStatus status;
  final List<Supplier> suppliers;
  final Supplier? selectedSupplier;
  final SupplierStatus detailStatus;
  final String? errorMessage;
  final String?
  actionMessage; // For success/error messages of actions (create/update/delete)
  final SupplierActionStatus actionStatus;

  // Filters
  final String searchTerm;
  final bool? filterIsActive;

  // Paging
  final int pageNumber;
  final int pageSize;
  final bool hasReachedEnd;
  final bool isLoadingMore;

  const SupplierState({
    this.status = SupplierStatus.initial,
    this.suppliers = const [],
    this.selectedSupplier,
    this.detailStatus = SupplierStatus.initial,
    this.errorMessage,
    this.actionMessage,
    this.actionStatus = SupplierActionStatus.initial,
    this.searchTerm = '',
    this.filterIsActive,
    this.pageNumber = 1,
    this.pageSize = 20,
    this.hasReachedEnd = false,
    this.isLoadingMore = false,
  });

  SupplierState copyWith({
    SupplierStatus? status,
    List<Supplier>? suppliers,
    Supplier? selectedSupplier,
    SupplierStatus? detailStatus,
    String? errorMessage,
    String? actionMessage,
    SupplierActionStatus? actionStatus,
    String? searchTerm,
    bool? filterIsActive,
    int? pageNumber,
    int? pageSize,
    bool? hasReachedEnd,
    bool? isLoadingMore,
  }) {
    return SupplierState(
      status: status ?? this.status,
      suppliers: suppliers ?? this.suppliers,
      selectedSupplier: selectedSupplier ?? this.selectedSupplier,
      detailStatus: detailStatus ?? this.detailStatus,
      errorMessage:
          errorMessage, // Clear error on new state unless explicitly set (or logic in bloc handles it)
      actionMessage: actionMessage,
      actionStatus: actionStatus ?? this.actionStatus,
      searchTerm: searchTerm ?? this.searchTerm,
      filterIsActive:
          filterIsActive ??
          this.filterIsActive, // Keep filter/search if not provided
      pageNumber: pageNumber ?? this.pageNumber,
      pageSize: pageSize ?? this.pageSize,
      hasReachedEnd: hasReachedEnd ?? this.hasReachedEnd,
      isLoadingMore: isLoadingMore ?? this.isLoadingMore,
    );
  }

  @override
  List<Object?> get props => [
    status,
    suppliers,
    selectedSupplier,
    detailStatus,
    errorMessage,
    actionMessage,
    actionStatus,
    searchTerm,
    filterIsActive,
    pageNumber,
    pageSize,
    hasReachedEnd,
    isLoadingMore,
  ];
}
