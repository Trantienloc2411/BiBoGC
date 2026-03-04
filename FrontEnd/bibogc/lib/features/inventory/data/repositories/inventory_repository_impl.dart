import 'package:bibogc/core/error/failures.dart';
import 'package:dartz/dartz.dart';
import 'package:injectable/injectable.dart';

import '../../domain/entities/stock_batch.dart';
import '../../domain/repositories/inventory_repository.dart';
import '../datasources/inventory_remote_data_source.dart';

@LazySingleton(as: InventoryRepository)
class InventoryRepositoryImpl implements InventoryRepository {
  final InventoryRemoteDataSource _remoteDataSource;

  InventoryRepositoryImpl(this._remoteDataSource);

  @override
  Future<Either<Failure, StockBatch>> createBatch({
    required String productId,
    required String batchNumber,
    required int quantity,
    required DateTime manufacturingDate,
    required DateTime expirationDate,
    required double costPrice,
  }) async {
    try {
      final model = await _remoteDataSource.createBatch(
        productId: productId,
        batchNumber: batchNumber,
        quantity: quantity,
        manufacturingDate: manufacturingDate,
        expirationDate: expirationDate,
        costPrice: costPrice,
      );
      return Right(model.toEntity());
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> createPurchaseTransaction({
    required String productId,
    required String productBatchId,
    required String supplierId,
    required int quantity,
    required double unitPrice,
    String? notes,
  }) async {
    try {
      await _remoteDataSource.createPurchaseTransaction(
        productId: productId,
        productBatchId: productBatchId,
        supplierId: supplierId,
        quantity: quantity,
        unitPrice: unitPrice,
        notes: notes,
      );
      return const Right(null);
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }
}

