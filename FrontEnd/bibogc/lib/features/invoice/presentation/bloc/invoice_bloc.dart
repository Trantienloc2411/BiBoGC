import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:bibogc/features/invoice/domain/repositories/invoice_repository.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:injectable/injectable.dart';

part 'invoice_event.dart';
part 'invoice_state.dart';

@injectable
class InvoiceBloc extends Bloc<InvoiceEvent, InvoiceState> {
  final InvoiceRepository _repository;

  InvoiceBloc(this._repository) : super(const InvoiceState()) {
    on<InvoicesStarted>(_onStarted);
    on<InvoicesFilterChanged>(_onFilterChanged);
    on<InvoicesLoadMore>(_onLoadMore);
    on<InvoiceDetailRequested>(_onDetailRequested);
  }

  Future<void> _onStarted(
    InvoicesStarted event,
    Emitter<InvoiceState> emit,
  ) async {
    emit(
      state.copyWith(
        listStatus: InvoiceStatus.loading,
        currentPage: 1,
        hasMore: false,
      ),
    );
    await _loadInvoices(emit, page: 1, append: false);
  }

  Future<void> _onFilterChanged(
    InvoicesFilterChanged event,
    Emitter<InvoiceState> emit,
  ) async {
    emit(
      state.copyWith(
        filterDateFrom: event.dateFrom,
        filterDateTo: event.dateTo,
        listStatus: InvoiceStatus.loading,
        currentPage: 1,
        hasMore: false,
      ),
    );
    await _loadInvoices(emit, page: 1, append: false);
  }

  Future<void> _onLoadMore(
    InvoicesLoadMore event,
    Emitter<InvoiceState> emit,
  ) async {
    if (!state.hasMore) return;
    if (state.listStatus == InvoiceStatus.loading) return;
    final nextPage = state.currentPage + 1;
    emit(state.copyWith(listStatus: InvoiceStatus.loading));
    await _loadInvoices(emit, page: nextPage, append: true);
  }

  Future<void> _loadInvoices(
    Emitter<InvoiceState> emit, {
    required int page,
    required bool append,
  }) async {
    final result = await _repository.getInvoices(
      page: page,
      pageSize: 20,
      dateFrom: state.filterDateFrom,
      dateTo: state.filterDateTo,
    );

    result.fold(
      (failure) => emit(
        state.copyWith(
          listStatus: InvoiceStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (tuple) {
        final (invoices, hasMore) = tuple;
        final merged = append ? [...state.invoices, ...invoices] : invoices;
        emit(
          state.copyWith(
            listStatus: InvoiceStatus.success,
            invoices: merged,
            hasMore: hasMore,
            currentPage: page,
          ),
        );
      },
    );
  }

  Future<void> _onDetailRequested(
    InvoiceDetailRequested event,
    Emitter<InvoiceState> emit,
  ) async {
    emit(state.copyWith(detailStatus: InvoiceStatus.loading));
    final result = await _repository.getInvoiceById(event.id);
    result.fold(
      (failure) => emit(
        state.copyWith(
          detailStatus: InvoiceStatus.failure,
          errorMessage: _mapFailure(failure),
        ),
      ),
      (invoice) => emit(
        state.copyWith(
          detailStatus: InvoiceStatus.success,
          selectedInvoice: invoice,
        ),
      ),
    );
  }

  String _mapFailure(Failure failure) {
    if (failure is ServerFailure) return failure.message;
    return 'Đã xảy ra lỗi không xác định';
  }
}
