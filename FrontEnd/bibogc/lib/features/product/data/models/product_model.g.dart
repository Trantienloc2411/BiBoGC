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
  availableStock: (json['availableStock'] as num?)?.toInt(),
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
      'availableStock': instance.availableStock,
      'lowStockThreshold': instance.lowStockThreshold,
      'variants': instance.variants?.map((e) => e.toJson()).toList(),
    };
