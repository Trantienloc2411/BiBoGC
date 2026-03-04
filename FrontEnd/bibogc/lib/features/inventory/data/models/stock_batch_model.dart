import 'package:json_annotation/json_annotation.dart';
import '../../domain/entities/stock_batch.dart';

part 'stock_batch_model.g.dart';

@JsonSerializable()
class StockBatchModel {
  final String id;
  final String productId;
  final String batchNumber;
  final int quantity;
  final DateTime manufacturingDate;
  final DateTime expirationDate;
  final double costPrice;

  const StockBatchModel({
    required this.id,
    required this.productId,
    required this.batchNumber,
    required this.quantity,
    required this.manufacturingDate,
    required this.expirationDate,
    required this.costPrice,
  });

  factory StockBatchModel.fromJson(Map<String, dynamic> json) =>
      _$StockBatchModelFromJson(json);

  Map<String, dynamic> toJson() => _$StockBatchModelToJson(this);

  StockBatch toEntity() => StockBatch(
        id: id,
        productId: productId,
        batchNumber: batchNumber,
        quantity: quantity,
        manufacturingDate: manufacturingDate,
        expirationDate: expirationDate,
        costPrice: costPrice,
      );

  static StockBatchModel fromEntity(StockBatch batch) => StockBatchModel(
        id: batch.id,
        productId: batch.productId,
        batchNumber: batch.batchNumber,
        quantity: batch.quantity,
        manufacturingDate: batch.manufacturingDate,
        expirationDate: batch.expirationDate,
        costPrice: batch.costPrice,
      );
}

