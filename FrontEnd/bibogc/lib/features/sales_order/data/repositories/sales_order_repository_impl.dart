import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:bibogc/features/sales_order/data/datasources/sales_order_remote_data_source.dart';
import 'package:bibogc/features/sales_order/domain/entities/sales_order.dart';
import 'package:bibogc/features/sales_order/domain/repositories/sales_order_repository.dart';
import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:injectable/injectable.dart';

@LazySingleton(as: SalesOrderRepository)
class SalesOrderRepositoryImpl implements SalesOrderRepository {
  final SalesOrderRemoteDataSource _remoteDataSource;

  SalesOrderRepositoryImpl(this._remoteDataSource);

  /// Extracts a user-readable message from any exception.
  /// For Dio 4xx/5xx responses the server's `message` field is used;
  /// for everything else a generic string is returned.
  String _serverMessage(Object e) {
    if (e is DioException) {
      final data = e.response?.data;
      if (data is Map) {
        final msg = data['message'] ?? data['error'] ?? data['title'];
        if (msg != null) return msg.toString();
      }
    }
    return 'Đã xảy ra lỗi không xác định';
  }

  @override
  Future<Either<Failure, (List<SalesOrder>, bool)>> getSalesOrders({
    int page = 1,
    int pageSize = 20,
    OrderStatus? status,
    DateTime? dateFrom,
    DateTime? dateTo,
    String? search,
  }) async {
    try {
      final result = await _remoteDataSource.getSalesOrders(
        page: page,
        pageSize: pageSize,
        status: status?.value,
        dateFrom: dateFrom,
        dateTo: dateTo,
        search: search,
      );
      final orders = result.items.map((m) => m.toEntity()).toList();
      return Right((orders, result.hasMore));
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> getSalesOrderById(String id) async {
    try {
      final model = await _remoteDataSource.getSalesOrderById(id);
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> createSalesOrder({
    required int paymentMethod,
    String? customerName,
    String? customerPhone,
    String? notes,
  }) async {
    try {
      final model = await _remoteDataSource.createSalesOrder(
        paymentMethod: paymentMethod,
        customerName: customerName,
        customerPhone: customerPhone,
        notes: notes,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> addItem({
    required String orderId,
    required String productId,
    required String productVariantId,
    String? productBatchId,
    required int quantity,
  }) async {
    try {
      final model = await _remoteDataSource.addItem(
        orderId: orderId,
        productId: productId,
        productVariantId: productVariantId,
        productBatchId: productBatchId,
        quantity: quantity,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> updateItemQuantity({
    required String orderId,
    required String itemId,
    required int quantity,
  }) async {
    try {
      final model = await _remoteDataSource.updateItemQuantity(
        orderId: orderId,
        itemId: itemId,
        quantity: quantity,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> removeItem({
    required String orderId,
    required String itemId,
  }) async {
    try {
      final model = await _remoteDataSource.removeItem(
        orderId: orderId,
        itemId: itemId,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> applyDiscount({
    required String orderId,
    required double discountAmount,
  }) async {
    try {
      final model = await _remoteDataSource.applyDiscount(
        orderId: orderId,
        discountAmount: discountAmount,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> completeOrder({
    required String orderId,
    required double amountPaid,
  }) async {
    try {
      final model = await _remoteDataSource.completeOrder(
        orderId: orderId,
        amountPaid: amountPaid,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, SalesOrder>> cancelOrder({
    required String orderId,
    String? reason,
  }) async {
    try {
      final model = await _remoteDataSource.cancelOrder(
        orderId: orderId,
        reason: reason,
      );
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }

  @override
  Future<Either<Failure, Invoice>> generateInvoice(String orderId) async {
    try {
      final model = await _remoteDataSource.generateInvoice(orderId);
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(_serverMessage(e)));
    }
  }
}
