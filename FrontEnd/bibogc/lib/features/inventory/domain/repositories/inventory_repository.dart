import 'package:bibogc/core/error/failures.dart';
import 'package:dartz/dartz.dart';

import '../entities/stock_batch.dart';

abstract class InventoryRepository {
  Future<Either<Failure, StockBatch>> createBatch({
    required String productId,
    required String batchNumber,
    required int quantity,
    required DateTime manufacturingDate,
    required DateTime expirationDate,
    required double costPrice,
  });

  Future<Either<Failure, void>> createPurchaseTransaction({
    required String productId,
    required String productBatchId,
    required String supplierId,
    required int quantity,
    required double unitPrice,
    String? notes,
  });
}

