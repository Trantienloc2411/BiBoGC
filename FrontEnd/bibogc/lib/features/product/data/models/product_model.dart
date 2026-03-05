import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:json_annotation/json_annotation.dart';

part 'product_model.g.dart';

/// DTO for Product (API layer).
///
/// Mirrors `ProductDto` from the backend but we only map the fields
/// needed by the mobile UI.
@JsonSerializable(explicitToJson: true)
class ProductModel {
  final String id;
  final String name;
  final String? description;
  final String status;
  final bool requiresBatchTracking;
  final String? categoryId;
  final String? categoryName;
  final int? totalStock;
  final int? availableStock;
  final int? lowStockThreshold;
  final List<ProductVariantModel>? variants;

  const ProductModel({
    required this.id,
    required this.name,
    this.description,
    this.status = 'Active',
    this.requiresBatchTracking = false,
    this.categoryId,
    this.categoryName,
    this.totalStock,
    this.availableStock,
    this.lowStockThreshold,
    this.variants,
  });

  factory ProductModel.fromJson(Map<String, dynamic> json) =>
      _$ProductModelFromJson(json);

  Map<String, dynamic> toJson() => _$ProductModelToJson(this);

  Product toEntity() => Product(
    id: id,
    name: name,
    description: description ?? '',
    status: status,
    requiresBatchTracking: requiresBatchTracking,
    categoryId: categoryId,
    categoryName: categoryName,
    totalStock: availableStock ?? totalStock ?? 0,
    lowStockThreshold: lowStockThreshold,
    variants: variants?.map((v) => v.toEntity()).toList() ?? const [],
  );

  factory ProductModel.fromEntity(Product product) => ProductModel(
    id: product.id,
    name: product.name,
    description: product.description,
    status: product.status,
    requiresBatchTracking: product.requiresBatchTracking,
    categoryId: product.categoryId,
    categoryName: product.categoryName,
    totalStock: product.totalStock,
    availableStock: product.totalStock,
    lowStockThreshold: product.lowStockThreshold,
    variants: product.variants.map(ProductVariantModel.fromEntity).toList(),
  );
}

/// DTO for ProductVariant (child) in API.
/// Manually deserialised because the backend uses nested value objects
/// (e.g. salePrice: { value: 175000 }, skuUnique: { value: "..." }).
class ProductVariantModel {
  final String id;
  final String productId;
  final String sku;
  final String? barcode;
  final String name;
  final String unit;
  final int quantityBaseUnit;
  final double salePrice;
  final double? costPrice;
  final int? stockQuantity;
  final DateTime? nearestExpiryDate;
  final String? batchNumber;
  final bool isActive;

  const ProductVariantModel({
    required this.id,
    required this.productId,
    required this.sku,
    required this.name,
    required this.unit,
    required this.quantityBaseUnit,
    required this.salePrice,
    this.barcode,
    this.costPrice,
    this.stockQuantity,
    this.nearestExpiryDate,
    this.batchNumber,
    this.isActive = true,
  });

  factory ProductVariantModel.fromJson(Map<String, dynamic> json) {
    final skuUnique = json['skuUnique'] as Map<String, dynamic>?;
    final salePriceObj = json['salePrice'] as Map<String, dynamic>?;
    final costPriceObj = json['costPrice'] as Map<String, dynamic>?;
    return ProductVariantModel(
      id: json['id'] as String,
      productId: json['productId'] as String,
      sku: skuUnique?['value'] as String? ?? '',
      barcode: json['barcode'] as String?,
      name: json['variantName'] as String? ?? json['name'] as String? ?? '',
      unit: json['unitName'] as String? ?? json['unit']?.toString() ?? '',
      quantityBaseUnit: (json['quantityBaseUnit'] as num?)?.toInt() ?? 1,
      salePrice: (salePriceObj?['value'] as num?)?.toDouble() ??
          (json['salePrice'] as num?)?.toDouble() ?? 0,
      costPrice: (costPriceObj?['value'] as num?)?.toDouble() ??
          (json['costPrice'] as num?)?.toDouble(),
      stockQuantity: (json['stockQuantity'] as num?)?.toInt(),
      nearestExpiryDate: json['nearestExpiryDate'] == null
          ? null
          : DateTime.tryParse(json['nearestExpiryDate'] as String),
      batchNumber: json['batchNumber'] as String?,
      isActive: json['isActive'] as bool? ?? true,
    );
  }

  Map<String, dynamic> toJson() => _$ProductVariantModelToJson(this);

  ProductVariant toEntity() => ProductVariant(
    id: id,
    productId: productId,
    sku: sku,
    name: name,
    unit: unit,
    quantityBaseUnit: quantityBaseUnit,
    salePrice: salePrice,
    barcode: barcode,
    costPrice: costPrice,
    stockQuantity: stockQuantity,
    nearestExpiryDate: nearestExpiryDate,
    batchNumber: batchNumber,
    isActive: isActive,
  );

  factory ProductVariantModel.fromEntity(ProductVariant variant) =>
      ProductVariantModel(
        id: variant.id,
        productId: variant.productId,
        sku: variant.sku,
        name: variant.name,
        unit: variant.unit,
        quantityBaseUnit: variant.quantityBaseUnit,
        salePrice: variant.salePrice,
        barcode: variant.barcode,
        costPrice: variant.costPrice,
        stockQuantity: variant.stockQuantity,
        nearestExpiryDate: variant.nearestExpiryDate,
        batchNumber: variant.batchNumber,
        isActive: variant.isActive,
      );
}
