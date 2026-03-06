class Invoice {
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
  final List<InvoiceItem> items;
  final DateTime createdAt;

  const Invoice({
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

  String get paymentMethodLabel => switch (paymentMethod) {
    'Cash' => 'Tiền mặt',
    'BankTransfer' => 'Chuyển khoản',
    _ => paymentMethod,
  };
}

class InvoiceItem {
  final String id;
  final String productName;
  final String variantName;
  final String sku;
  final String unit;
  final int quantity;
  final double unitPrice;
  final double lineTotal;

  const InvoiceItem({
    required this.id,
    required this.productName,
    required this.variantName,
    required this.sku,
    required this.unit,
    required this.quantity,
    required this.unitPrice,
    required this.lineTotal,
  });
}
