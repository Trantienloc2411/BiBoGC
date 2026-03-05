import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:json_annotation/json_annotation.dart';

part 'sales_order_model.g.dart';

@JsonSerializable(explicitToJson: true)
class SalesOrderModel {
  final String id;
  final String orderNumber;
  final String? customerName;
  final String? customerPhone;
  final DateTime orderDate;
  final double subTotal;
  final double discountAmount;
  final double taxAmount;
  final double totalAmount;
  final double amountPaid;
  final double changeAmount;
  final String paymentMethod;
  final String status;
  final String? notes;
  final String? invoiceId;
  final String? invoiceNumber;
  final int itemCount;
  final List<SalesOrderItemModel> items;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const SalesOrderModel({
    required this.id,
    required this.orderNumber,
    this.customerName,
    this.customerPhone,
    required this.orderDate,
    required this.subTotal,
    required this.discountAmount,
    required this.taxAmount,
    required this.totalAmount,
    required this.amountPaid,
    required this.changeAmount,
    required this.paymentMethod,
    required this.status,
    this.notes,
    this.invoiceId,
    this.invoiceNumber,
    required this.itemCount,
    required this.items,
    required this.createdAt,
    this.updatedAt,
  });

  factory SalesOrderModel.fromJson(Map<String, dynamic> json) =>
      _$SalesOrderModelFromJson(json);

  Map<String, dynamic> toJson() => _$SalesOrderModelToJson(this);

  SalesOrder toEntity() => SalesOrder(
    id: id,
    orderNumber: orderNumber,
    customerName: customerName,
    customerPhone: customerPhone,
    orderDate: orderDate,
    subTotal: subTotal,
    discountAmount: discountAmount,
    taxAmount: taxAmount,
    totalAmount: totalAmount,
    amountPaid: amountPaid,
    changeAmount: changeAmount,
    paymentMethod: paymentMethod,
    status: OrderStatus.fromString(status),
    notes: notes,
    invoiceId: invoiceId,
    invoiceNumber: invoiceNumber,
    itemCount: itemCount,
    items: items.map((i) => i.toEntity()).toList(),
    createdAt: createdAt,
    updatedAt: updatedAt,
  );
}

@JsonSerializable()
class SalesOrderItemModel {
  final String id;
  final String productId;
  final String productVariantId;
  final String? productBatchId;
  final String productName;
  final String variantName;
  final String sku;
  final String unit;
  final int quantity;
  final double unitPrice;
  final double lineTotal;

  const SalesOrderItemModel({
    required this.id,
    required this.productId,
    required this.productVariantId,
    this.productBatchId,
    required this.productName,
    required this.variantName,
    required this.sku,
    required this.unit,
    required this.quantity,
    required this.unitPrice,
    required this.lineTotal,
  });

  factory SalesOrderItemModel.fromJson(Map<String, dynamic> json) =>
      _$SalesOrderItemModelFromJson(json);

  Map<String, dynamic> toJson() => _$SalesOrderItemModelToJson(this);

  SalesOrderItem toEntity() => SalesOrderItem(
    id: id,
    productId: productId,
    productVariantId: productVariantId,
    productBatchId: productBatchId,
    productName: productName,
    variantName: variantName,
    sku: sku,
    unit: unit,
    quantity: quantity,
    unitPrice: unitPrice,
    lineTotal: lineTotal,
  );
}
