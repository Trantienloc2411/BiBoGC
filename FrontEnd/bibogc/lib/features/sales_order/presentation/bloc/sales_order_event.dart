part of 'sales_order_bloc.dart';

abstract class SalesOrderEvent extends Equatable {
  const SalesOrderEvent();

  @override
  List<Object?> get props => [];
}

class SalesOrdersStarted extends SalesOrderEvent {
  const SalesOrdersStarted();
}

class SalesOrdersFilterChanged extends SalesOrderEvent {
  final OrderStatus? status;
  final DateTime? dateFrom;
  final DateTime? dateTo;

  const SalesOrdersFilterChanged({this.status, this.dateFrom, this.dateTo});

  @override
  List<Object?> get props => [status, dateFrom, dateTo];
}

class SalesOrdersSearchChanged extends SalesOrderEvent {
  final String search;

  const SalesOrdersSearchChanged(this.search);

  @override
  List<Object?> get props => [search];
}

class SalesOrdersLoadMore extends SalesOrderEvent {
  const SalesOrdersLoadMore();
}

class SalesOrderDetailRequested extends SalesOrderEvent {
  final String id;

  const SalesOrderDetailRequested(this.id);

  @override
  List<Object?> get props => [id];
}

class SalesOrderCreateRequested extends SalesOrderEvent {
  final int paymentMethod;
  final String? customerName;
  final String? customerPhone;
  final String? notes;

  const SalesOrderCreateRequested({
    required this.paymentMethod,
    this.customerName,
    this.customerPhone,
    this.notes,
  });

  @override
  List<Object?> get props => [
    paymentMethod,
    customerName,
    customerPhone,
    notes,
  ];
}

class SalesOrderItemAdded extends SalesOrderEvent {
  final String orderId;
  final String productId;
  final String productVariantId;
  final String? productBatchId;
  final int quantity;

  const SalesOrderItemAdded({
    required this.orderId,
    required this.productId,
    required this.productVariantId,
    this.productBatchId,
    required this.quantity,
  });

  @override
  List<Object?> get props => [
    orderId,
    productId,
    productVariantId,
    productBatchId,
    quantity,
  ];
}

class SalesOrderItemQuantityUpdated extends SalesOrderEvent {
  final String orderId;
  final String itemId;
  final int quantity;

  const SalesOrderItemQuantityUpdated({
    required this.orderId,
    required this.itemId,
    required this.quantity,
  });

  @override
  List<Object?> get props => [orderId, itemId, quantity];
}

class SalesOrderItemRemoved extends SalesOrderEvent {
  final String orderId;
  final String itemId;

  const SalesOrderItemRemoved({required this.orderId, required this.itemId});

  @override
  List<Object?> get props => [orderId, itemId];
}

class SalesOrderDiscountApplied extends SalesOrderEvent {
  final String orderId;
  final double discountAmount;

  const SalesOrderDiscountApplied({
    required this.orderId,
    required this.discountAmount,
  });

  @override
  List<Object?> get props => [orderId, discountAmount];
}

class SalesOrderCompleted extends SalesOrderEvent {
  final String orderId;
  final double amountPaid;

  const SalesOrderCompleted({required this.orderId, required this.amountPaid});

  @override
  List<Object?> get props => [orderId, amountPaid];
}

class SalesOrderCancelled extends SalesOrderEvent {
  final String orderId;
  final String? reason;

  const SalesOrderCancelled({required this.orderId, this.reason});

  @override
  List<Object?> get props => [orderId, reason];
}

class SalesOrderInvoiceGenerated extends SalesOrderEvent {
  final String orderId;

  const SalesOrderInvoiceGenerated(this.orderId);

  @override
  List<Object?> get props => [orderId];
}
