import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:dartz/dartz.dart';

abstract class SalesOrderRepository {
  Future<Either<Failure, (List<SalesOrder> orders, bool hasMore)>>
  getSalesOrders({
    int page = 1,
    int pageSize = 20,
    OrderStatus? status,
    DateTime? dateFrom,
    DateTime? dateTo,
    String? search,
  });

  Future<Either<Failure, SalesOrder>> getSalesOrderById(String id);

  Future<Either<Failure, SalesOrder>> createSalesOrder({
    required int paymentMethod,
    String? customerName,
    String? customerPhone,
    String? notes,
  });

  Future<Either<Failure, SalesOrder>> addItem({
    required String orderId,
    required String productId,
    required String productVariantId,
    String? productBatchId,
    required int quantity,
  });

  Future<Either<Failure, SalesOrder>> updateItemQuantity({
    required String orderId,
    required String itemId,
    required int quantity,
  });

  Future<Either<Failure, SalesOrder>> removeItem({
    required String orderId,
    required String itemId,
  });

  Future<Either<Failure, SalesOrder>> applyDiscount({
    required String orderId,
    required double discountAmount,
  });

  Future<Either<Failure, SalesOrder>> completeOrder({
    required String orderId,
    required double amountPaid,
  });

  Future<Either<Failure, SalesOrder>> cancelOrder({
    required String orderId,
    String? reason,
  });

  Future<Either<Failure, Invoice>> generateInvoice(String orderId);
}
