enum OrderStatus {
  draft, // 1 — Đang soạn
  completed, // 2 — Hoàn thành
  cancelled; // 3 — Đã hủy

  static OrderStatus fromString(String value) {
    return switch (value.toLowerCase()) {
      'completed' => OrderStatus.completed,
      'cancelled' => OrderStatus.cancelled,
      _ => OrderStatus.draft,
    };
  }

  static OrderStatus fromInt(int value) {
    return switch (value) {
      2 => OrderStatus.completed,
      3 => OrderStatus.cancelled,
      _ => OrderStatus.draft,
    };
  }

  String get label => switch (this) {
    OrderStatus.draft => 'Đang soạn',
    OrderStatus.completed => 'Hoàn thành',
    OrderStatus.cancelled => 'Đã hủy',
  };

  int get value => switch (this) {
    OrderStatus.draft => 1,
    OrderStatus.completed => 2,
    OrderStatus.cancelled => 3,
  };
}

class SalesOrder {
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
  final OrderStatus status;
  final String? notes;
  final String? invoiceId;
  final String? invoiceNumber;
  final int itemCount;
  final List<SalesOrderItem> items;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const SalesOrder({
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

  String get paymentMethodLabel => switch (paymentMethod) {
    'Cash' => 'Tiền mặt',
    'BankTransfer' => 'Chuyển khoản',
    _ => paymentMethod,
  };
}

class SalesOrderItem {
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

  const SalesOrderItem({
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
}
