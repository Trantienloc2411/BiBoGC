// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'invoice_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

InvoiceModel _$InvoiceModelFromJson(Map<String, dynamic> json) => InvoiceModel(
  id: json['id'] as String,
  invoiceNumber: json['invoiceNumber'] as String,
  invoiceDate: DateTime.parse(json['invoiceDate'] as String),
  salesOrderId: json['salesOrderId'] as String,
  orderNumber: json['orderNumber'] as String,
  storeName: json['storeName'] as String,
  storeAddress: json['storeAddress'] as String,
  storePhone: json['storePhone'] as String,
  storeTaxCode: json['storeTaxCode'] as String?,
  customerName: json['customerName'] as String?,
  customerPhone: json['customerPhone'] as String?,
  subTotal: (json['subTotal'] as num).toDouble(),
  discountAmount: (json['discountAmount'] as num).toDouble(),
  taxAmount: (json['taxAmount'] as num).toDouble(),
  grandTotal: (json['grandTotal'] as num).toDouble(),
  amountPaid: (json['amountPaid'] as num).toDouble(),
  changeAmount: (json['changeAmount'] as num).toDouble(),
  paymentMethod: json['paymentMethod'] as String,
  items: (json['items'] as List<dynamic>)
      .map((e) => InvoiceItemModel.fromJson(e as Map<String, dynamic>))
      .toList(),
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$InvoiceModelToJson(InvoiceModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'invoiceNumber': instance.invoiceNumber,
      'invoiceDate': instance.invoiceDate.toIso8601String(),
      'salesOrderId': instance.salesOrderId,
      'orderNumber': instance.orderNumber,
      'storeName': instance.storeName,
      'storeAddress': instance.storeAddress,
      'storePhone': instance.storePhone,
      'storeTaxCode': instance.storeTaxCode,
      'customerName': instance.customerName,
      'customerPhone': instance.customerPhone,
      'subTotal': instance.subTotal,
      'discountAmount': instance.discountAmount,
      'taxAmount': instance.taxAmount,
      'grandTotal': instance.grandTotal,
      'amountPaid': instance.amountPaid,
      'changeAmount': instance.changeAmount,
      'paymentMethod': instance.paymentMethod,
      'items': instance.items.map((e) => e.toJson()).toList(),
      'createdAt': instance.createdAt.toIso8601String(),
    };

InvoiceItemModel _$InvoiceItemModelFromJson(Map<String, dynamic> json) =>
    InvoiceItemModel(
      id: json['id'] as String,
      productName: json['productName'] as String,
      variantName: json['variantName'] as String,
      sku: json['sku'] as String,
      unit: json['unit'] as String,
      quantity: (json['quantity'] as num).toInt(),
      unitPrice: (json['unitPrice'] as num).toDouble(),
      lineTotal: (json['lineTotal'] as num).toDouble(),
    );

Map<String, dynamic> _$InvoiceItemModelToJson(InvoiceItemModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'productName': instance.productName,
      'variantName': instance.variantName,
      'sku': instance.sku,
      'unit': instance.unit,
      'quantity': instance.quantity,
      'unitPrice': instance.unitPrice,
      'lineTotal': instance.lineTotal,
    };
