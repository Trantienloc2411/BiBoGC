import 'package:equatable/equatable.dart';

import '../../domain/entities/stock_batch.dart';

enum StockImportStatus { initial, loading, success, failure }

class StockImportState extends Equatable {
  final int stepIndex;

  final String? supplierId;
  final String? supplierName;

  final String? productId;
  final String? productName;

  final StockBatch? createdBatch;

  final StockImportStatus status;
  final String? errorMessage;

  const StockImportState({
    this.stepIndex = 0,
    this.supplierId,
    this.supplierName,
    this.productId,
    this.productName,
    this.createdBatch,
    this.status = StockImportStatus.initial,
    this.errorMessage,
  });

  StockImportState copyWith({
    int? stepIndex,
    String? supplierId,
    String? supplierName,
    String? productId,
    String? productName,
    StockBatch? createdBatch,
    StockImportStatus? status,
    String? errorMessage,
  }) {
    return StockImportState(
      stepIndex: stepIndex ?? this.stepIndex,
      supplierId: supplierId ?? this.supplierId,
      supplierName: supplierName ?? this.supplierName,
      productId: productId ?? this.productId,
      productName: productName ?? this.productName,
      createdBatch: createdBatch ?? this.createdBatch,
      status: status ?? this.status,
      errorMessage: errorMessage,
    );
  }

  @override
  List<Object?> get props => [
        stepIndex,
        supplierId,
        supplierName,
        productId,
        productName,
        createdBatch,
        status,
        errorMessage,
      ];
}

