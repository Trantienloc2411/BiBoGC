import 'package:bibogc/core/common/paged_result.dart';
import 'package:bibogc/core/constants/api_constants.dart';
import 'package:bibogc/core/network/dio_client.dart';
import 'package:bibogc/features/invoice/data/models/invoice_model.dart';
import 'package:bibogc/features/sales_order/data/models/sales_order_model.dart';
import 'package:injectable/injectable.dart';

abstract class SalesOrderRemoteDataSource {
  Future<PagedResult<SalesOrderModel>> getSalesOrders({
    int page = 1,
    int pageSize = 20,
    int? status,
    DateTime? dateFrom,
    DateTime? dateTo,
    String? search,
  });

  Future<SalesOrderModel> getSalesOrderById(String id);

  Future<SalesOrderModel> createSalesOrder({
    required int paymentMethod,
    String? customerName,
    String? customerPhone,
    String? notes,
  });

  Future<SalesOrderModel> addItem({
    required String orderId,
    required String productId,
    required String productVariantId,
    String? productBatchId,
    required int quantity,
  });

  Future<SalesOrderModel> updateItemQuantity({
    required String orderId,
    required String itemId,
    required int quantity,
  });

  Future<SalesOrderModel> removeItem({
    required String orderId,
    required String itemId,
  });

  Future<SalesOrderModel> applyDiscount({
    required String orderId,
    required double discountAmount,
  });

  Future<SalesOrderModel> completeOrder({
    required String orderId,
    required double amountPaid,
  });

  Future<SalesOrderModel> cancelOrder({
    required String orderId,
    String? reason,
  });

  Future<InvoiceModel> generateInvoice(String orderId);
}

@LazySingleton(as: SalesOrderRemoteDataSource)
class SalesOrderRemoteDataSourceImpl implements SalesOrderRemoteDataSource {
  final DioClient _dioClient;

  SalesOrderRemoteDataSourceImpl(this._dioClient);

  @override
  Future<PagedResult<SalesOrderModel>> getSalesOrders({
    int page = 1,
    int pageSize = 20,
    int? status,
    DateTime? dateFrom,
    DateTime? dateTo,
    String? search,
  }) async {
    final params = <String, dynamic>{
      'page': page,
      'pageSize': pageSize,
      if (status != null) 'status': status,
      if (dateFrom != null) 'dateFrom': dateFrom.toIso8601String(),
      if (dateTo != null) 'dateTo': dateTo.toIso8601String(),
      if (search != null && search.isNotEmpty) 'search': search,
    };

    final response = await _dioClient.dio.get(
      ApiConstants.salesOrders,
      queryParameters: params,
    );

    final data = response.data['data'] as Map<String, dynamic>;
    return PagedResult.fromJson(
      data,
      (item) => SalesOrderModel.fromJson(item as Map<String, dynamic>),
    );
  }

  @override
  Future<SalesOrderModel> getSalesOrderById(String id) async {
    final response = await _dioClient.dio.get(
      '${ApiConstants.salesOrders}/$id',
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> createSalesOrder({
    required int paymentMethod,
    String? customerName,
    String? customerPhone,
    String? notes,
  }) async {
    final response = await _dioClient.dio.post(
      ApiConstants.salesOrders,
      data: {
        'paymentMethod': paymentMethod,
        if (customerName != null && customerName.isNotEmpty)
          'customerName': customerName,
        if (customerPhone != null && customerPhone.isNotEmpty)
          'customerPhone': customerPhone,
        if (notes != null && notes.isNotEmpty) 'notes': notes,
      },
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> addItem({
    required String orderId,
    required String productId,
    required String productVariantId,
    String? productBatchId,
    required int quantity,
  }) async {
    final response = await _dioClient.dio.post(
      '${ApiConstants.salesOrders}/$orderId/items',
      data: {
        'productId': productId,
        'productVariantId': productVariantId,
        if (productBatchId != null) 'productBatchId': productBatchId,
        'quantity': quantity,
      },
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> updateItemQuantity({
    required String orderId,
    required String itemId,
    required int quantity,
  }) async {
    final response = await _dioClient.dio.put(
      '${ApiConstants.salesOrders}/$orderId/items/$itemId',
      data: {'quantity': quantity},
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> removeItem({
    required String orderId,
    required String itemId,
  }) async {
    final response = await _dioClient.dio.delete(
      '${ApiConstants.salesOrders}/$orderId/items/$itemId',
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> applyDiscount({
    required String orderId,
    required double discountAmount,
  }) async {
    final response = await _dioClient.dio.post(
      '${ApiConstants.salesOrders}/$orderId/discount',
      data: {'discountAmount': discountAmount},
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> completeOrder({
    required String orderId,
    required double amountPaid,
  }) async {
    final response = await _dioClient.dio.post(
      '${ApiConstants.salesOrders}/$orderId/complete',
      data: {'amountPaid': amountPaid},
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<SalesOrderModel> cancelOrder({
    required String orderId,
    String? reason,
  }) async {
    final response = await _dioClient.dio.post(
      '${ApiConstants.salesOrders}/$orderId/cancel',
      data: {if (reason != null && reason.isNotEmpty) 'reason': reason},
    );
    return SalesOrderModel.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  @override
  Future<InvoiceModel> generateInvoice(String orderId) async {
    final response = await _dioClient.dio.post(
      '${ApiConstants.salesOrders}/$orderId/invoice',
    );
    return InvoiceModel.fromJson(response.data['data'] as Map<String, dynamic>);
  }
}
