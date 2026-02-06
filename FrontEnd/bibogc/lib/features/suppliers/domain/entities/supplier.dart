class Supplier {
  final String id;
  final String name;
  final String? contactName;
  final String? contactPhone;
  final String? address;
  final bool isActive;
  final DateTime? createAt;
  final List<SupplierTransaction>? transactions;

  Supplier({
    required this.id,
    required this.name,
    this.contactName,
    this.contactPhone,
    this.address,
    required this.isActive,
    this.createAt,
    this.transactions,
  });
}

class SupplierTransaction {
  final String id;
  final String productId;
  final String? productName;
  final String? productBatchId;
  final String? batchNumber;
  final String? supplierId;
  final String? supplierName;
  final String? sku;
  final String transactionType;
  final int quantity;
  final double unitPrice;
  final double totalAmount;
  final DateTime transactionDate;
  final String? notes;
  final String? referenceNumber;

  SupplierTransaction({
    required this.id,
    required this.productId,
    this.productName,
    this.productBatchId,
    this.batchNumber,
    required this.supplierId,
    this.supplierName,
    this.sku,
    required this.transactionType,
    required this.quantity,
    required this.unitPrice,
    required this.totalAmount,
    required this.transactionDate,
    this.notes,
    this.referenceNumber,
  });
}
