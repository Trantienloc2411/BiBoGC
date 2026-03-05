// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'supplier_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SupplierModel _$SupplierModelFromJson(Map<String, dynamic> json) =>
    SupplierModel(
      id: json['id'] as String,
      name: json['name'] as String,
      contactName: json['contactName'] as String?,
      contactPhone: json['contactPhone'] as String?,
      address: json['address'] as String?,
      isActive: json['isActive'] as bool,
      createAt: json['createAt'] == null
          ? null
          : DateTime.parse(json['createAt'] as String),
      transactions: (json['transactions'] as List<dynamic>?)
          ?.map((e) =>
              SupplierTransactionModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$SupplierModelToJson(SupplierModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'contactName': instance.contactName,
      'contactPhone': instance.contactPhone,
      'address': instance.address,
      'isActive': instance.isActive,
      'createAt': instance.createAt?.toIso8601String(),
      'transactions': instance.transactions?.map((e) => e.toJson()).toList(),
    };

SupplierTransactionModel _$SupplierTransactionModelFromJson(
        Map<String, dynamic> json) =>
    SupplierTransactionModel(
      id: json['id'] as String,
      productId: json['productId'] as String,
      productName: json['productName'] as String?,
      productBatchId: json['productBatchId'] as String?,
      batchNumber: json['batchNumber'] as String?,
      supplierId: json['supplierId'] as String?,
      supplierName: json['supplierName'] as String?,
      sku: json['sku'] as String?,
      transactionType: json['transactionType'] as String,
      quantity: (json['quantity'] as num?)?.toInt() ?? 0,
      unitPrice: (json['unitPrice'] as num).toDouble(),
      totalAmount: (json['totalAmount'] as num).toDouble(),
      transactionDate: DateTime.parse(json['transactionDate'] as String),
      notes: json['notes'] as String?,
      referenceNumber: json['referenceNumber'] as String?,
    );

Map<String, dynamic> _$SupplierTransactionModelToJson(
        SupplierTransactionModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'productId': instance.productId,
      'productName': instance.productName,
      'productBatchId': instance.productBatchId,
      'batchNumber': instance.batchNumber,
      'supplierId': instance.supplierId,
      'supplierName': instance.supplierName,
      'sku': instance.sku,
      'transactionType': instance.transactionType,
      'quantity': instance.quantity,
      'unitPrice': instance.unitPrice,
      'totalAmount': instance.totalAmount,
      'transactionDate': instance.transactionDate.toIso8601String(),
      'notes': instance.notes,
      'referenceNumber': instance.referenceNumber,
    };
