import 'package:equatable/equatable.dart';

/// Domain entity representing a product batch used in inventory flows.
class StockBatch extends Equatable {
  final String id;
  final String productId;
  final String batchNumber;
  final int quantity;
  final DateTime manufacturingDate;
  final DateTime expirationDate;
  final double costPrice;

  const StockBatch({
    required this.id,
    required this.productId,
    required this.batchNumber,
    required this.quantity,
    required this.manufacturingDate,
    required this.expirationDate,
    required this.costPrice,
  });

  @override
  List<Object?> get props => [
        id,
        productId,
        batchNumber,
        quantity,
        manufacturingDate,
        expirationDate,
        costPrice,
      ];
}

