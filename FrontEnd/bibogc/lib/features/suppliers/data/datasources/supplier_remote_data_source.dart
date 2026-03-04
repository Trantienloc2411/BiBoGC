import 'package:bibogc/core/network/dio_client.dart';
import 'package:bibogc/features/suppliers/data/models/supplier_model.dart';
import 'package:injectable/injectable.dart';

abstract class SupplierRemoteDataSource {
  Future<List<SupplierModel>> getSuppliers({
    int pageNumber = 1,
    int pageSize = 10,
    String? searchTerm,
    bool? isActive,
    String sortBy = 'Name',
    bool sortDescending = false,
  });

  Future<SupplierModel> getSupplierById(String id);

  Future<void> createSupplier(SupplierModel supplier);

  Future<void> updateSupplier(SupplierModel supplier);

  Future<void> deleteSupplier(String id);

  Future<void> activateSupplier(String id);

  Future<void> deactivateSupplier(String id);
}

@LazySingleton(as: SupplierRemoteDataSource)
class SupplierRemoteDataSourceImpl implements SupplierRemoteDataSource {
  final DioClient _dioClient;

  SupplierRemoteDataSourceImpl(this._dioClient);

  @override
  Future<List<SupplierModel>> getSuppliers({
    int pageNumber = 1,
    int pageSize = 10,
    String? searchTerm,
    bool? isActive,
    String sortBy = 'Name',
    bool sortDescending = false,
  }) async {
    final queryParameters = <String, dynamic>{
      'pageNumber': pageNumber,
      'pageSize': pageSize,
      'searchTerm':
          (searchTerm != null && searchTerm.isNotEmpty) ? searchTerm : null,
      'isActive': isActive,
      'sortBy': sortBy,
      'sortDescending': sortDescending,
    }..removeWhere((_, value) => value == null);

    final response = await _dioClient.dio.get(
      '/api/suppliers',
      queryParameters: queryParameters,
    );

    if (response.data != null && response.data['items'] != null) {
      return (response.data['items'] as List)
          .map((e) => SupplierModel.fromJson(e as Map<String, dynamic>))
          .toList();
    }
    return [];
  }

  @override
  Future<SupplierModel> getSupplierById(String id) async {
    final response = await _dioClient.dio.get('/api/suppliers/$id');
    return SupplierModel.fromJson(response.data);
  }

  @override
  Future<void> createSupplier(SupplierModel supplier) async {
    await _dioClient.dio.post(
      '/api/suppliers',
      data: {
        'name': supplier.name,
        'contactName': supplier.contactName,
        'contactPhone': supplier.contactPhone,
        'address': supplier.address,
      },
    );
  }

  @override
  Future<void> updateSupplier(SupplierModel supplier) async {
    await _dioClient.dio.put(
      '/api/suppliers/${supplier.id}',
      data: {
        'name': supplier.name,
        'contactName': supplier.contactName,
        'contactPhone': supplier.contactPhone,
        'address': supplier.address,
        'isActive': supplier.isActive,
      },
    );
  }

  @override
  Future<void> deleteSupplier(String id) async {
    await _dioClient.dio.delete('/api/suppliers/$id');
  }

  @override
  Future<void> activateSupplier(String id) async {
    await _dioClient.dio.post('/api/suppliers/$id/activate');
  }

  @override
  Future<void> deactivateSupplier(String id) async {
    await _dioClient.dio.post('/api/suppliers/$id/deactivate');
  }
}
