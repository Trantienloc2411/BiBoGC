import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:bibogc/features/suppliers/domain/repositories/supplier_repository.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:injectable/injectable.dart';
import 'package:rxdart/rxdart.dart';

part 'supplier_event.dart';
part 'supplier_state.dart';

@injectable
class SupplierBloc extends Bloc<SupplierEvent, SupplierState> {
  final SupplierRepository _repository;

  SupplierBloc(this._repository) : super(const SupplierState()) {
    on<SuppliersStarted>(_onStarted);
    on<SuppliersRefreshed>(_onRefreshed);
    on<SuppliersLoadMoreRequested>(_onLoadMoreRequested);
    on<SuppliersSearchChanged>(
      _onSearchChanged,
      transformer: (events, mapper) => events
          .debounceTime(const Duration(milliseconds: 500))
          .distinct()
          .switchMap(mapper),
    );
    on<SuppliersFilterChanged>(_onFilterChanged);
    on<SupplierDetailRequested>(_onDetailRequested);
    on<SupplierCreated>(_onCreated);
    on<SupplierUpdated>(_onUpdated);
    on<SupplierDeleted>(_onDeleted);
    on<SupplierActivated>(_onActivated);
    on<SupplierDeactivated>(_onDeactivated);
  }

  Future<void> _onStarted(
    SuppliersStarted event,
    Emitter<SupplierState> emit,
  ) async {
    emit(
      state.copyWith(
        status: SupplierStatus.loading,
        pageNumber: 1,
        hasReachedEnd: false,
        isLoadingMore: false,
      ),
    );
    await _loadSuppliers(emit, pageNumber: 1, append: false);
  }

  Future<void> _onRefreshed(
    SuppliersRefreshed event,
    Emitter<SupplierState> emit,
  ) async {
    emit(
      state.copyWith(
        status: SupplierStatus.loading,
        pageNumber: 1,
        hasReachedEnd: false,
        isLoadingMore: false,
      ),
    );
    await _loadSuppliers(emit, pageNumber: 1, append: false);
  }

  Future<void> _onLoadMoreRequested(
    SuppliersLoadMoreRequested event,
    Emitter<SupplierState> emit,
  ) async {
    // Guardrails
    if (state.status == SupplierStatus.loading) return;
    if (state.isLoadingMore) return;
    if (state.hasReachedEnd) return;
    if (state.suppliers.isEmpty) return;

    final nextPage = state.pageNumber + 1;
    emit(state.copyWith(isLoadingMore: true));
    await _loadSuppliers(emit, pageNumber: nextPage, append: true);
  }

  Future<void> _onSearchChanged(
    SuppliersSearchChanged event,
    Emitter<SupplierState> emit,
  ) async {
    // Only reload if search term significantly changed or just update state for filter
    if (state.searchTerm != event.searchTerm) {
      emit(
        state.copyWith(
          searchTerm: event.searchTerm,
          status: SupplierStatus.loading,
          pageNumber: 1,
          hasReachedEnd: false,
          isLoadingMore: false,
        ),
      );
      await _loadSuppliers(emit, pageNumber: 1, append: false);
    }
  }

  Future<void> _onFilterChanged(
    SuppliersFilterChanged event,
    Emitter<SupplierState> emit,
  ) async {
    if (state.filterIsActive != event.isActive) {
      emit(
        state.copyWith(
          filterIsActive: event.isActive,
          status: SupplierStatus.loading,
          pageNumber: 1,
          hasReachedEnd: false,
          isLoadingMore: false,
        ),
      );
      await _loadSuppliers(emit, pageNumber: 1, append: false);
    }
  }

  Future<void> _loadSuppliers(
    Emitter<SupplierState> emit, {
    required int pageNumber,
    required bool append,
  }) async {
    final result = await _repository.getSuppliers(
      pageNumber: pageNumber,
      pageSize: state.pageSize,
      searchTerm: state.searchTerm,
      isActive: state.filterIsActive,
      // Default sort for now
      sortBy: 'Name',
    );

    result.fold(
      (failure) => emit(
        state.copyWith(
          status: SupplierStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
          isLoadingMore: false,
        ),
      ),
      (suppliers) {
        final merged = append ? [...state.suppliers, ...suppliers] : suppliers;
        final reachedEnd = suppliers.length < state.pageSize;

        emit(
          state.copyWith(
            status: SupplierStatus.success,
            suppliers: merged,
            pageNumber: pageNumber,
            hasReachedEnd: reachedEnd,
            isLoadingMore: false,
          ),
        );
      },
    );
  }

  Future<void> _onDetailRequested(
    SupplierDetailRequested event,
    Emitter<SupplierState> emit,
  ) async {
    emit(state.copyWith(detailStatus: SupplierStatus.loading));
    final result = await _repository.getSupplierById(event.id);
    result.fold(
      (failure) => emit(
        state.copyWith(
          detailStatus: SupplierStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (supplier) => emit(
        state.copyWith(
          detailStatus: SupplierStatus.success,
          selectedSupplier: supplier,
        ),
      ),
    );
  }

  Future<void> _onCreated(
    SupplierCreated event,
    Emitter<SupplierState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SupplierActionStatus.loading));
    final result = await _repository.createSupplier(event.supplier);
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SupplierActionStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (_) async {
        emit(
          state.copyWith(
            actionStatus: SupplierActionStatus.success,
            actionMessage: 'Tạo nhà cung cấp thành công',
          ),
        );
        // Reload list to show new item
        await _loadSuppliers(emit, pageNumber: 1, append: false);
      },
    );
  }

  Future<void> _onUpdated(
    SupplierUpdated event,
    Emitter<SupplierState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SupplierActionStatus.loading));
    final result = await _repository.updateSupplier(event.supplier);
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SupplierActionStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (_) async {
        emit(
          state.copyWith(
            actionStatus: SupplierActionStatus.success,
            actionMessage: 'Cập nhật nhà cung cấp thành công',
          ),
        );
        // Reload details if selected
        if (state.selectedSupplier?.id == event.supplier.id) {
          add(SupplierDetailRequested(event.supplier.id));
        }
        // Reload first page so list UI reflects the update immediately.
        await _loadSuppliers(emit, pageNumber: 1, append: false);
      },
    );
  }

  Future<void> _onDeleted(
    SupplierDeleted event,
    Emitter<SupplierState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SupplierActionStatus.loading));
    final result = await _repository.deleteSupplier(event.id);
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SupplierActionStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (_) async {
        emit(
          state.copyWith(
            actionStatus: SupplierActionStatus.success,
            actionMessage: 'Xóa nhà cung cấp thành công',
          ),
        );
        await _loadSuppliers(emit, pageNumber: 1, append: false);
      },
    );
  }

  Future<void> _onActivated(
    SupplierActivated event,
    Emitter<SupplierState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SupplierActionStatus.loading));
    final result = await _repository.activateSupplier(event.id);
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SupplierActionStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (_) async {
        emit(
          state.copyWith(
            actionStatus: SupplierActionStatus.success,
            actionMessage: 'Kích hoạt nhà cung cấp thành công',
          ),
        );
        // Reload details if selected
        if (state.selectedSupplier?.id == event.id) {
          add(SupplierDetailRequested(event.id));
        }
        await _loadSuppliers(emit, pageNumber: 1, append: false);
      },
    );
  }

  Future<void> _onDeactivated(
    SupplierDeactivated event,
    Emitter<SupplierState> emit,
  ) async {
    emit(state.copyWith(actionStatus: SupplierActionStatus.loading));
    final result = await _repository.deactivateSupplier(event.id);
    await result.fold(
      (failure) async => emit(
        state.copyWith(
          actionStatus: SupplierActionStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (_) async {
        emit(
          state.copyWith(
            actionStatus: SupplierActionStatus.success,
            actionMessage: 'Vô hiệu hóa nhà cung cấp thành công',
          ),
        );
        // Reload details if selected
        if (state.selectedSupplier?.id == event.id) {
          add(SupplierDetailRequested(event.id));
        }
        await _loadSuppliers(emit, pageNumber: 1, append: false);
      },
    );
  }

  String _mapFailureToMessage(Failure failure) {
    if (failure is ServerFailure) {
      return failure.message;
    }
    return 'Đã xảy ra lỗi không xác định';
  }
}
