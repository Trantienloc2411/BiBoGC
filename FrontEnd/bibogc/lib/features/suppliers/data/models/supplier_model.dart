import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:json_annotation/json_annotation.dart';

part 'supplier_model.g.dart';

/// Data Transfer Object for Supplier API payloads.
///
/// Clean Architecture note:
/// - This class stays in the **data layer** and is responsible for JSON (de)serialization.
/// - Convert to/from the **domain entity** via [toEntity] / [fromEntity].
@JsonSerializable(explicitToJson: true)
class SupplierModel {
  final String id;
  final String name;
  final String? contactName;
  final String? contactPhone;
  final String? address;
  final bool isActive;
  final DateTime? createAt;
  final List<SupplierTransactionModel>? transactions;

  const SupplierModel({
    required this.id,
    required this.name,
    this.contactName,
    this.contactPhone,
    this.address,
    required this.isActive,
    this.createAt,
    this.transactions,
  });

  factory SupplierModel.fromJson(Map<String, dynamic> json) =>
      _$SupplierModelFromJson(json);

  Map<String, dynamic> toJson() => _$SupplierModelToJson(this);

  Supplier toEntity() => Supplier(
    id: id,
    name: name,
    contactName: contactName,
    contactPhone: contactPhone,
    address: address,
    isActive: isActive,
    createAt: createAt,
    transactions: transactions?.map((t) => t.toEntity()).toList(),
  );

  factory SupplierModel.fromEntity(Supplier supplier) => SupplierModel(
    id: supplier.id,
    name: supplier.name,
    contactName: supplier.contactName,
    contactPhone: supplier.contactPhone,
    address: supplier.address,
    isActive: supplier.isActive,
    createAt: supplier.createAt,
    transactions: supplier.transactions
        ?.map(SupplierTransactionModel.fromEntity)
        .toList(),
  );
}

@JsonSerializable()
class SupplierTransactionModel {
  final String id;
  final String productId;
  final String? productName;
  final String? productBatchId;
  final String? batchNumber;
  final String? supplierId;
  final String? supplierName;
  final String? sku;
  final String transactionType;
  final int quantity;  // may be absent from API
  final double unitPrice;
  final double totalAmount;
  final DateTime transactionDate;
  final String? notes;
  final String? referenceNumber;

  const SupplierTransactionModel({
    required this.id,
    required this.productId,
    this.productName,
    this.productBatchId,
    this.batchNumber,
    this.supplierId,
    this.supplierName,
    this.sku,
    required this.transactionType,
    this.quantity = 0,
    required this.unitPrice,
    required this.totalAmount,
    required this.transactionDate,
    this.notes,
    this.referenceNumber,
  });

  factory SupplierTransactionModel.fromJson(Map<String, dynamic> json) =>
      _$SupplierTransactionModelFromJson(json);

  Map<String, dynamic> toJson() => _$SupplierTransactionModelToJson(this);

  SupplierTransaction toEntity() => SupplierTransaction(
    id: id,
    productId: productId,
    productName: productName,
    productBatchId: productBatchId,
    batchNumber: batchNumber,
    supplierId: supplierId,
    supplierName: supplierName,
    sku: sku,
    transactionType: transactionType,
    quantity: quantity,
    unitPrice: unitPrice,
    totalAmount: totalAmount,
    transactionDate: transactionDate,
    notes: notes,
    referenceNumber: referenceNumber,
  );

  factory SupplierTransactionModel.fromEntity(
    SupplierTransaction transaction,
  ) => SupplierTransactionModel(
    id: transaction.id,
    productId: transaction.productId,
    productName: transaction.productName,
    productBatchId: transaction.productBatchId,
    batchNumber: transaction.batchNumber,
    supplierId: transaction.supplierId,
    supplierName: transaction.supplierName,
    sku: transaction.sku,
    transactionType: transaction.transactionType,
    quantity: transaction.quantity,
    unitPrice: transaction.unitPrice,
    totalAmount: transaction.totalAmount,
    transactionDate: transaction.transactionDate,
    notes: transaction.notes,
    referenceNumber: transaction.referenceNumber,
  );
}
