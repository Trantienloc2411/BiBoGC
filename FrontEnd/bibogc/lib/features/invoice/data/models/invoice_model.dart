import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:json_annotation/json_annotation.dart';

part 'invoice_model.g.dart';

@JsonSerializable(explicitToJson: true)
class InvoiceModel {
  final String id;
  final String invoiceNumber;
  final DateTime invoiceDate;
  final String salesOrderId;
  final String orderNumber;
  final String storeName;
  final String storeAddress;
  final String storePhone;
  final String? storeTaxCode;
  final String? customerName;
  final String? customerPhone;
  final double subTotal;
  final double discountAmount;
  final double taxAmount;
  final double grandTotal;
  final double amountPaid;
  final double changeAmount;
  final String paymentMethod;
  final List<InvoiceItemModel> items;
  final DateTime createdAt;

  const InvoiceModel({
    required this.id,
    required this.invoiceNumber,
    required this.invoiceDate,
    required this.salesOrderId,
    required this.orderNumber,
    required this.storeName,
    required this.storeAddress,
    required this.storePhone,
    this.storeTaxCode,
    this.customerName,
    this.customerPhone,
    required this.subTotal,
    required this.discountAmount,
    required this.taxAmount,
    required this.grandTotal,
    required this.amountPaid,
    required this.changeAmount,
    required this.paymentMethod,
    required this.items,
    required this.createdAt,
  });

  factory InvoiceModel.fromJson(Map<String, dynamic> json) =>
      _$InvoiceModelFromJson(json);

  Map<String, dynamic> toJson() => _$InvoiceModelToJson(this);

  Invoice toEntity() => Invoice(
    id: id,
    invoiceNumber: invoiceNumber,
    invoiceDate: invoiceDate,
    salesOrderId: salesOrderId,
    orderNumber: orderNumber,
    storeName: storeName,
    storeAddress: storeAddress,
    storePhone: storePhone,
    storeTaxCode: storeTaxCode,
    customerName: customerName,
    customerPhone: customerPhone,
    subTotal: subTotal,
    discountAmount: discountAmount,
    taxAmount: taxAmount,
    grandTotal: grandTotal,
    amountPaid: amountPaid,
    changeAmount: changeAmount,
    paymentMethod: paymentMethod,
    items: items.map((i) => i.toEntity()).toList(),
    createdAt: createdAt,
  );
}

@JsonSerializable()
class InvoiceItemModel {
  final String id;
  final String productName;
  final String variantName;
  final String sku;
  final String unit;
  final int quantity;
  final double unitPrice;
  final double lineTotal;

  const InvoiceItemModel({
    required this.id,
    required this.productName,
    required this.variantName,
    required this.sku,
    required this.unit,
    required this.quantity,
    required this.unitPrice,
    required this.lineTotal,
  });

  factory InvoiceItemModel.fromJson(Map<String, dynamic> json) =>
      _$InvoiceItemModelFromJson(json);

  Map<String, dynamic> toJson() => _$InvoiceItemModelToJson(this);

  InvoiceItem toEntity() => InvoiceItem(
    id: id,
    productName: productName,
    variantName: variantName,
    sku: sku,
    unit: unit,
    quantity: quantity,
    unitPrice: unitPrice,
    lineTotal: lineTotal,
  );
}
