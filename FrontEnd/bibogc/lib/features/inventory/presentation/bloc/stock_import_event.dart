import 'package:equatable/equatable.dart';

class StockImportEvent extends Equatable {
  const StockImportEvent();

  @override
  List<Object?> get props => [];
}

class StockImportSupplierSelected extends StockImportEvent {
  final String supplierId;
  final String supplierName;

  const StockImportSupplierSelected({
    required this.supplierId,
    required this.supplierName,
  });

  @override
  List<Object?> get props => [supplierId, supplierName];
}

class StockImportProductSelected extends StockImportEvent {
  final String productId;
  final String productName;

  const StockImportProductSelected({
    required this.productId,
    required this.productName,
  });

  @override
  List<Object?> get props => [productId, productName];
}

class StockImportStepChanged extends StockImportEvent {
  final int stepIndex;

  const StockImportStepChanged(this.stepIndex);

  @override
  List<Object?> get props => [stepIndex];
}

class StockImportSubmitted extends StockImportEvent {
  final String batchNumber;
  final DateTime manufacturingDate;
  final DateTime expirationDate;
  final int quantity;
  final double costPrice;
  final double unitPrice;
  final String? notes;

  const StockImportSubmitted({
    required this.batchNumber,
    required this.manufacturingDate,
    required this.expirationDate,
    required this.quantity,
    required this.costPrice,
    required this.unitPrice,
    this.notes,
  });

  @override
  List<Object?> get props => [
        batchNumber,
        manufacturingDate,
        expirationDate,
        quantity,
        costPrice,
        unitPrice,
        notes,
      ];
}

