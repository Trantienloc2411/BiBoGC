import 'package:equatable/equatable.dart';

/// Domain entity: Product (parent).
///
/// A product is the "logical" item the shop owner thinks about
/// (Coca Cola, Mì Hảo Hảo, Sữa Vinamilk...), which can have many
/// concrete [ProductVariant]s (lon, thùng, lốc...).
class Product extends Equatable {
  final String id;
  final String name;
  final String description;

  /// e.g. Active / Discontinued (string from backend for now).
  final String status;

  /// Whether this product is tracked by batch/expiry in inventory.
  final bool requiresBatchTracking;

  final String? categoryId;
  final String? categoryName;

  /// Total stock in base units (sum of all variants/batches).
  final int totalStock;

  /// When totalStock < lowStockThreshold → show low-stock warning.
  final int? lowStockThreshold;

  /// Child variants of this product.
  final List<ProductVariant> variants;

  const Product({
    required this.id,
    required this.name,
    this.description = '',
    this.status = 'Active',
    this.requiresBatchTracking = false,
    this.categoryId,
    this.categoryName,
    this.totalStock = 0,
    this.lowStockThreshold,
    this.variants = const [],
  });

  @override
  List<Object?> get props => [
    id,
    name,
    description,
    status,
    requiresBatchTracking,
    categoryId,
    categoryName,
    totalStock,
    lowStockThreshold,
    variants,
  ];
}

/// Domain entity: ProductVariant (child).
///
/// Một Product có nhiều ProductVariant.
/// Ví dụ:
/// - Product: "Coca Cola"
/// - Variants: "Lon 330ml", "Thùng 24 lon", "Chai 1.5L"
class ProductVariant extends Equatable {
  final String id;
  final String productId;

  /// Business SKU (unique per variant).
  final String sku;
  final String? barcode;

  /// Human-friendly name for this variant (e.g. "Lon 330ml").
  final String name;

  /// Unit label (e.g. "Lon", "Thùng", "Chai").
  final String unit;

  /// How many base units this variant represents (e.g. 24 lon/thùng).
  final int quantityBaseUnit;

  /// Selling price (per variant).
  final double salePrice;

  /// Optional cost price (for margin/reporting).
  final double? costPrice;

  /// Current stock quantity for this variant (if backend provides it).
  final int? stockQuantity;

  /// Nearest expiry date for this variant (derived from batches).
  final DateTime? nearestExpiryDate;

  /// Optional batch number when this variant is tied to a specific batch.
  final String? batchNumber;

  final bool isActive;

  const ProductVariant({
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

  @override
  List<Object?> get props => [
    id,
    productId,
    sku,
    barcode,
    name,
    unit,
    quantityBaseUnit,
    salePrice,
    costPrice,
    stockQuantity,
    nearestExpiryDate,
    batchNumber,
    isActive,
  ];
}
