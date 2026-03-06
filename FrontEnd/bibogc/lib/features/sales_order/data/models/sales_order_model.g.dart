// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'sales_order_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SalesOrderModel _$SalesOrderModelFromJson(Map<String, dynamic> json) =>
    SalesOrderModel(
      id: json['id'] as String,
      orderNumber: json['orderNumber'] as String,
      customerName: json['customerName'] as String?,
      customerPhone: json['customerPhone'] as String?,
      orderDate: DateTime.parse(json['orderDate'] as String),
      subTotal: (json['subTotal'] as num).toDouble(),
      discountAmount: (json['discountAmount'] as num).toDouble(),
      taxAmount: (json['taxAmount'] as num).toDouble(),
      totalAmount: (json['totalAmount'] as num).toDouble(),
      amountPaid: (json['amountPaid'] as num).toDouble(),
      changeAmount: (json['changeAmount'] as num).toDouble(),
      paymentMethod: json['paymentMethod'] as String,
      status: json['status'] as String,
      notes: json['notes'] as String?,
      invoiceId: json['invoiceId'] as String?,
      invoiceNumber: json['invoiceNumber'] as String?,
      itemCount: (json['itemCount'] as num).toInt(),
      items: (json['items'] as List<dynamic>)
          .map((e) => SalesOrderItemModel.fromJson(e as Map<String, dynamic>))
          .toList(),
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
    );

Map<String, dynamic> _$SalesOrderModelToJson(SalesOrderModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'orderNumber': instance.orderNumber,
      'customerName': instance.customerName,
      'customerPhone': instance.customerPhone,
      'orderDate': instance.orderDate.toIso8601String(),
      'subTotal': instance.subTotal,
      'discountAmount': instance.discountAmount,
      'taxAmount': instance.taxAmount,
      'totalAmount': instance.totalAmount,
      'amountPaid': instance.amountPaid,
      'changeAmount': instance.changeAmount,
      'paymentMethod': instance.paymentMethod,
      'status': instance.status,
      'notes': instance.notes,
      'invoiceId': instance.invoiceId,
      'invoiceNumber': instance.invoiceNumber,
      'itemCount': instance.itemCount,
      'items': instance.items.map((e) => e.toJson()).toList(),
      'createdAt': instance.createdAt.toIso8601String(),
      'updatedAt': instance.updatedAt?.toIso8601String(),
    };

SalesOrderItemModel _$SalesOrderItemModelFromJson(Map<String, dynamic> json) =>
    SalesOrderItemModel(
      id: json['id'] as String,
      productId: json['productId'] as String,
      productVariantId: json['productVariantId'] as String,
      productBatchId: json['productBatchId'] as String?,
      productName: json['productName'] as String,
      variantName: json['variantName'] as String,
      sku: json['sku'] as String,
      unit: json['unit'] as String,
      quantity: (json['quantity'] as num).toInt(),
      unitPrice: (json['unitPrice'] as num).toDouble(),
      lineTotal: (json['lineTotal'] as num).toDouble(),
    );

Map<String, dynamic> _$SalesOrderItemModelToJson(
        SalesOrderItemModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'productId': instance.productId,
      'productVariantId': instance.productVariantId,
      'productBatchId': instance.productBatchId,
      'productName': instance.productName,
      'variantName': instance.variantName,
      'sku': instance.sku,
      'unit': instance.unit,
      'quantity': instance.quantity,
      'unitPrice': instance.unitPrice,
      'lineTotal': instance.lineTotal,
    };
