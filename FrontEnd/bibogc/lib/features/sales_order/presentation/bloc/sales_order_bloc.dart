import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:bibogc/features/sales_order/domain/repositories/sales_order_repository.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:injectable/injectable.dart';
import 'package:rxdart/rxdart.dart';

part 'sales_order_event.dart';
part 'sales_order_state.dart';

@injectable
class SalesOrderBloc extends Bloc<SalesOrderEvent, SalesOrderState> {
  final SalesOrderRepository _repository;

  SalesOrderBloc(this._repository) : super(const SalesOrderState()) {
    on<SalesOrdersStarted>(_onStarted);
    on<SalesOrdersFilterChanged>(_onFilterChanged);
    on<SalesOrdersLoadMore>(_onLoadMore);
    on<SalesOrderDetailRequested>(_onDetailRequested);
    on<SalesOrderCreateRequested>(_onCreateRequested);
    on<SalesOrderItemAdded>(_onItemAdded);
    on<SalesOrderItemQuantityUpdated>(_onItemQuantityUpdated);
    on<SalesOrderItemRemoved>(_onItemRemoved);
    on<SalesOrderDiscountApplied>(_onDiscountApplied);
    on<SalesOrderCompleted>(_onCompleted);
    on<SalesOrderCancelled>(_onCancelled);
    on<SalesOrderInvoiceGenerated>(_onInvoiceGenerated);
    on<SalesOrdersSearchChanged>(
      _onSearchChanged,
      transformer: (events, mapper) => events
          .debounceTime(const Duration(milliseconds: 400))
          .distinct()
          .switchMap(mapper),
    );
  }

  Future<void> _onStarted(
    SalesOrdersStarted event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(
      state.copyWith(
        listStatus: SalesOrderStatus.loading,
        currentPage: 1,
        hasMore: false,
      ),
    );
    await _loadOrders(emit, page: 1, append: false);
  }

  Future<void> _onFilterChanged(
    SalesOrdersFilterChanged event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(
      state.copyWith(
        filterStatus: event.status,
        filterDateFrom: event.dateFrom,
        filterDateTo: event.dateTo,
        listStatus: SalesOrderStatus.loading,
        currentPage: 1,
        hasMore: false,
      ),
    );
    await _loadOrders(emit, page: 1, append: false);
  }

  Future<void> _onSearchChanged(
    SalesOrdersSearchChanged event,
    Emitter<SalesOrderState> emit,
  ) async {
    if (state.searchTerm == event.search) return;
    emit(
      state.copyWith(
        searchTerm: event.search,
        listStatus: SalesOrderStatus.loading,
        currentPage: 1,
        hasMore: false,
      ),
    );
    await _loadOrders(emit, page: 1, append: false);
  }

  Future<void> _onLoadMore(
    SalesOrdersLoadMore event,
    Emitter<SalesOrderState> emit,
  ) async {
    if (!state.hasMore) return;
    if (state.listStatus == SalesOrderStatus.loading) return;
    final nextPage = state.currentPage + 1;
    emit(state.copyWith(listStatus: SalesOrderStatus.loading));
    await _loadOrders(emit, page: nextPage, append: true);
  }

  Future<void> _loadOrders(
    Emitter<SalesOrderState> emit, {
    required int page,
    required bool append,
  }) async {
    final result = await _repository.getSalesOrders(
      page: page,
      pageSize: 20,
      status: state.filterStatus,
      dateFrom: state.filterDateFrom,
      dateTo: state.filterDateTo,
      search: state.searchTerm,
    );

    result.fold(
      (failure) => emit(
        state.copyWith(
          listStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (tuple) {
        final (orders, hasMore) = tuple;
        final merged = append ? [...state.orders, ...orders] : orders;
        emit(
          state.copyWith(
            listStatus: SalesOrderStatus.success,
            orders: merged,
            hasMore: hasMore,
            currentPage: page,
          ),
        );
      },
    );
  }

  Future<void> _onDetailRequested(
    SalesOrderDetailRequested event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(detailStatus: SalesOrderStatus.loading));
    final result = await _repository.getSalesOrderById(event.id);
    result.fold(
      (failure) => emit(
        state.copyWith(
          detailStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) => emit(
        state.copyWith(
          detailStatus: SalesOrderStatus.success,
          selectedOrder: order,
        ),
      ),
    );
  }

  Future<void> _onCreateRequested(
    SalesOrderCreateRequested event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.createSalesOrder(
      paymentMethod: event.paymentMethod,
      customerName: event.customerName,
      customerPhone: event.customerPhone,
      notes: event.notes,
    );
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) async {
        emit(
          state.copyWith(
            actionStatus: SalesOrderStatus.success,
            selectedOrder: order,
          ),
        );
        await _loadOrders(emit, page: 1, append: false);
      },
    );
  }

  Future<void> _onItemAdded(
    SalesOrderItemAdded event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.addItem(
      orderId: event.orderId,
      productId: event.productId,
      productVariantId: event.productVariantId,
      productBatchId: event.productBatchId,
      quantity: event.quantity,
    );
    result.fold(
      (failure) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.success,
          selectedOrder: order,
        ),
      ),
    );
  }

  Future<void> _onItemQuantityUpdated(
    SalesOrderItemQuantityUpdated event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.updateItemQuantity(
      orderId: event.orderId,
      itemId: event.itemId,
      quantity: event.quantity,
    );
    result.fold(
      (failure) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.success,
          selectedOrder: order,
        ),
      ),
    );
  }

  Future<void> _onItemRemoved(
    SalesOrderItemRemoved event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.removeItem(
      orderId: event.orderId,
      itemId: event.itemId,
    );
    result.fold(
      (failure) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.success,
          selectedOrder: order,
        ),
      ),
    );
  }

  Future<void> _onDiscountApplied(
    SalesOrderDiscountApplied event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.applyDiscount(
      orderId: event.orderId,
      discountAmount: event.discountAmount,
    );
    result.fold(
      (failure) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.success,
          selectedOrder: order,
        ),
      ),
    );
  }

  Future<void> _onCompleted(
    SalesOrderCompleted event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.completeOrder(
      orderId: event.orderId,
      amountPaid: event.amountPaid,
    );
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) async {
        emit(
          state.copyWith(
            actionStatus: SalesOrderStatus.success,
            selectedOrder: order,
          ),
        );
        await _loadOrders(emit, page: 1, append: false);
      },
    );
  }

  Future<void> _onCancelled(
    SalesOrderCancelled event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.cancelOrder(
      orderId: event.orderId,
      reason: event.reason,
    );
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (order) async {
        emit(
          state.copyWith(
            actionStatus: SalesOrderStatus.success,
            selectedOrder: order,
          ),
        );
        await _loadOrders(emit, page: 1, append: false);
      },
    );
  }

  Future<void> _onInvoiceGenerated(
    SalesOrderInvoiceGenerated event,
    Emitter<SalesOrderState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SalesOrderStatus.loading));
    final result = await _repository.generateInvoice(event.orderId);
    result.fold(
      (failure) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (invoice) => emit(
        state.copyWith(
          actionStatus: SalesOrderStatus.success,
          generatedInvoice: invoice,
        ),
      ),
    );
  }

  String _mapFailure(Failure failure) {
    if (failure is ServerFailure) return failure.message;
    return 'Đã xảy ra lỗi không xác định';
  }
}
