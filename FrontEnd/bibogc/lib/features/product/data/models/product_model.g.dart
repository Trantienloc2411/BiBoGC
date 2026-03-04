// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'product_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ProductModel _$ProductModelFromJson(Map<String, dynamic> json) => ProductModel(
      id: json['id'] as String,
      name: json['name'] as String,
      description: json['description'] as String?,
      status: json['status'] as String? ?? 'Active',
      requiresBatchTracking: json['requiresBatchTracking'] as bool? ?? false,
      categoryId: json['categoryId'] as String?,
      categoryName: json['categoryName'] as String?,
      totalStock: (json['totalStock'] as num?)?.toInt(),
      lowStockThreshold: (json['lowStockThreshold'] as num?)?.toInt(),
      variants: (json['variants'] as List<dynamic>?)
          ?.map((e) => ProductVariantModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$ProductModelToJson(ProductModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'description': instance.description,
      'status': instance.status,
      'requiresBatchTracking': instance.requiresBatchTracking,
      'categoryId': instance.categoryId,
      'categoryName': instance.categoryName,
      'totalStock': instance.totalStock,
      'lowStockThreshold': instance.lowStockThreshold,
      'variants': instance.variants?.map((e) => e.toJson()).toList(),
    };

ProductVariantModel _$ProductVariantModelFromJson(Map<String, dynamic> json) =>
    ProductVariantModel(
      id: json['id'] as String,
      productId: json['productId'] as String,
      sku: json['sku'] as String,
      name: json['name'] as String,
      unit: json['unit'] as String,
      quantityBaseUnit: (json['quantityBaseUnit'] as num).toInt(),
      salePrice: (json['salePrice'] as num).toDouble(),
      barcode: json['barcode'] as String?,
      costPrice: (json['costPrice'] as num?)?.toDouble(),
      stockQuantity: (json['stockQuantity'] as num?)?.toInt(),
      nearestExpiryDate: json['nearestExpiryDate'] == null
          ? null
          : DateTime.parse(json['nearestExpiryDate'] as String),
      batchNumber: json['batchNumber'] as String?,
      isActive: json['isActive'] as bool? ?? true,
    );

Map<String, dynamic> _$ProductVariantModelToJson(
        ProductVariantModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'productId': instance.productId,
      'sku': instance.sku,
      'barcode': instance.barcode,
      'name': instance.name,
      'unit': instance.unit,
      'quantityBaseUnit': instance.quantityBaseUnit,
      'salePrice': instance.salePrice,
      'costPrice': instance.costPrice,
      'stockQuantity': instance.stockQuantity,
      'nearestExpiryDate': instance.nearestExpiryDate?.toIso8601String(),
      'batchNumber': instance.batchNumber,
      'isActive': instance.isActive,
    };
