import 'package:bibogc/core/constants/api_constants.dart';
import 'package:bibogc/core/network/dio_client.dart';
import 'package:injectable/injectable.dart';

import '../models/stock_batch_model.dart';

abstract class InventoryRemoteDataSource {
  /// Create a new batch for the given product.
  Future<StockBatchModel> createBatch({
    required String productId,
    required String batchNumber,
    required int quantity,
    required DateTime manufacturingDate,
    required DateTime expirationDate,
    required double costPrice,
  });

  /// Create a purchase stock transaction for the given product & batch.
  Future<void> createPurchaseTransaction({
    required String productId,
    required String productBatchId,
    required String supplierId,
    required int quantity,
    required double unitPrice,
    String? notes,
  });
}

@LazySingleton(as: InventoryRemoteDataSource)
class InventoryRemoteDataSourceImpl implements InventoryRemoteDataSource {
  final DioClient _dioClient;

  InventoryRemoteDataSourceImpl(this._dioClient);

  @override
  Future<StockBatchModel> createBatch({
    required String productId,
    required String batchNumber,
    required int quantity,
    required DateTime manufacturingDate,
    required DateTime expirationDate,
    required double costPrice,
  }) async {
    final response = await _dioClient.dio.post(
      '${ApiConstants.products}/$productId${ApiConstants.productBatches}',
      data: {
        'batchNumber': batchNumber,
        'quantity': quantity,
        'manufacturingDate': manufacturingDate.toIso8601String(),
        'expirationDate': expirationDate.toIso8601String(),
        'costPrice': costPrice,
      },
    );

    return StockBatchModel.fromJson(
      response.data as Map<String, dynamic>,
    );
  }

  @override
  Future<void> createPurchaseTransaction({
    required String productId,
    required String productBatchId,
    required String supplierId,
    required int quantity,
    required double unitPrice,
    String? notes,
  }) async {
    await _dioClient.dio.post(
      ApiConstants.purchase,
      data: {
        'productId': productId,
        'productBatchId': productBatchId,
        'supplierId': supplierId,
        'quantity': quantity,
        'unitPrice': unitPrice,
        if (notes != null && notes.isNotEmpty) 'notes': notes,
      },
    );
  }
}

