// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'stock_batch_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

StockBatchModel _$StockBatchModelFromJson(Map<String, dynamic> json) =>
    StockBatchModel(
      id: json['id'] as String,
      productId: json['productId'] as String,
      batchNumber: json['batchNumber'] as String,
      quantity: (json['quantity'] as num).toInt(),
      manufacturingDate: DateTime.parse(json['manufacturingDate'] as String),
      expirationDate: DateTime.parse(json['expirationDate'] as String),
      costPrice: (json['costPrice'] as num).toDouble(),
    );

Map<String, dynamic> _$StockBatchModelToJson(StockBatchModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'productId': instance.productId,
      'batchNumber': instance.batchNumber,
      'quantity': instance.quantity,
      'manufacturingDate': instance.manufacturingDate.toIso8601String(),
      'expirationDate': instance.expirationDate.toIso8601String(),
      'costPrice': instance.costPrice,
    };
