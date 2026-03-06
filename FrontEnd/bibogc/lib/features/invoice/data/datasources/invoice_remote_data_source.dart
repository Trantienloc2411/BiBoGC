import 'package:bibogc/core/common/paged_result.dart';
import 'package:bibogc/core/constants/api_constants.dart';
import 'package:bibogc/core/network/dio_client.dart';
import 'package:bibogc/features/invoice/data/models/invoice_model.dart';
import 'package:injectable/injectable.dart';

abstract class InvoiceRemoteDataSource {
  Future<PagedResult<InvoiceModel>> getInvoices({
    int page = 1,
    int pageSize = 20,
    DateTime? dateFrom,
    DateTime? dateTo,
  });

  Future<InvoiceModel> getInvoiceById(String id);
}

@LazySingleton(as: InvoiceRemoteDataSource)
class InvoiceRemoteDataSourceImpl implements InvoiceRemoteDataSource {
  final DioClient _dioClient;

  InvoiceRemoteDataSourceImpl(this._dioClient);

  @override
  Future<PagedResult<InvoiceModel>> getInvoices({
    int page = 1,
    int pageSize = 20,
    DateTime? dateFrom,
    DateTime? dateTo,
  }) async {
    final params = <String, dynamic>{
      'page': page,
      'pageSize': pageSize,
      if (dateFrom != null) 'dateFrom': dateFrom.toIso8601String(),
      if (dateTo != null) 'dateTo': dateTo.toIso8601String(),
    };

    final response = await _dioClient.dio.get(
      ApiConstants.invoices,
      queryParameters: params,
    );

    final data = response.data['data'] as Map<String, dynamic>;
    return PagedResult.fromJson(
      data,
      (item) => InvoiceModel.fromJson(item as Map<String, dynamic>),
    );
  }

  @override
  Future<InvoiceModel> getInvoiceById(String id) async {
    final response = await _dioClient.dio.get('${ApiConstants.invoices}/$id');
    return InvoiceModel.fromJson(response.data['data'] as Map<String, dynamic>);
  }
}
