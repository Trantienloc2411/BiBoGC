import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:injectable/injectable.dart';

import '../../domain/repositories/inventory_repository.dart';
import 'stock_import_event.dart';
import 'stock_import_state.dart';

@injectable
class StockImportBloc extends Bloc<StockImportEvent, StockImportState> {
  final InventoryRepository _repository;

  StockImportBloc(this._repository) : super(const StockImportState()) {
    on<StockImportSupplierSelected>(_onSupplierSelected);
    on<StockImportProductSelected>(_onProductSelected);
    on<StockImportStepChanged>(_onStepChanged);
    on<StockImportSubmitted>(_onSubmitted);
  }

  void _onSupplierSelected(
    StockImportSupplierSelected event,
    Emitter<StockImportState> emit,
  ) {
    emit(
      state.copyWith(
        supplierId: event.supplierId,
        supplierName: event.supplierName,
      ),
    );
  }

  void _onProductSelected(
    StockImportProductSelected event,
    Emitter<StockImportState> emit,
  ) {
    emit(
      state.copyWith(
        productId: event.productId,
        productName: event.productName,
      ),
    );
  }

  void _onStepChanged(
    StockImportStepChanged event,
    Emitter<StockImportState> emit,
  ) {
    emit(state.copyWith(stepIndex: event.stepIndex));
  }

  Future<void> _onSubmitted(
    StockImportSubmitted event,
    Emitter<StockImportState> emit,
  ) async {
    if (state.productId == null || state.supplierId == null) {
      emit(
        state.copyWith(
          status: StockImportStatus.failure,
          errorMessage: 'Vui lòng chọn nhà cung cấp và sản phẩm trước.',
        ),
      );
      return;
    }

    emit(state.copyWith(status: StockImportStatus.loading));

    // 1) Create batch
    final batchResult = await _repository.createBatch(
      productId: state.productId!,
      batchNumber: event.batchNumber,
      quantity: event.quantity,
      manufacturingDate: event.manufacturingDate,
      expirationDate: event.expirationDate,
      costPrice: event.costPrice,
    );

    await batchResult.fold(
      (failure) async {
        emit(
          state.copyWith(
            status: StockImportStatus.failure,
            errorMessage: failure.message,
          ),
        );
      },
      (batch) async {
        // 2) Create purchase transaction
        final txResult = await _repository.createPurchaseTransaction(
          productId: batch.productId,
          productBatchId: batch.id,
          supplierId: state.supplierId!,
          quantity: event.quantity,
          unitPrice: event.unitPrice,
          notes: event.notes,
        );

        txResult.fold(
          (failure) {
            emit(
              state.copyWith(
                createdBatch: batch,
                status: StockImportStatus.failure,
                errorMessage: failure.message,
              ),
            );
          },
          (_) {
            emit(
              state.copyWith(
                createdBatch: batch,
                status: StockImportStatus.success,
                errorMessage: null,
              ),
            );
          },
        );
      },
    );
  }
}

