import 'package:bibogc/core/constants/api_constants.dart';
import 'package:bibogc/core/network/dio_client.dart';
import 'package:bibogc/features/product/data/models/product_model.dart';
import 'package:injectable/injectable.dart';

abstract class ProductRemoteDataSource {
  Future<List<ProductModel>> getProducts({
    int pageNumber = 1,
    int pageSize = 20,
    String? searchTerm,
  });

  Future<ProductModel> getProductById(String id);

  Future<void> createProduct(ProductModel product);

  Future<void> updateProduct(ProductModel product);

  Future<void> deleteProduct(String id);
}

@LazySingleton(as: ProductRemoteDataSource)
class ProductRemoteDataSourceImpl implements ProductRemoteDataSource {
  final DioClient _dioClient;

  ProductRemoteDataSourceImpl(this._dioClient);

  @override
  Future<List<ProductModel>> getProducts({
    int pageNumber = 1,
    int pageSize = 20,
    String? searchTerm,
  }) async {
    final queryParameters = <String, dynamic>{
      'pageNumber': pageNumber,
      'pageSize': pageSize,
      'searchTerm': (searchTerm != null && searchTerm.isNotEmpty)
          ? searchTerm
          : null,
    }..removeWhere((_, value) => value == null);

    final response = await _dioClient.dio.get(
      ApiConstants.products,
      queryParameters: queryParameters,
    );

    if (response.data != null && response.data['items'] != null) {
      return (response.data['items'] as List)
          .map((e) => ProductModel.fromJson(e as Map<String, dynamic>))
          .toList();
    }
    return [];
  }

  @override
  Future<ProductModel> getProductById(String id) async {
    final response = await _dioClient.dio.get('${ApiConstants.products}/$id');
    return ProductModel.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> createProduct(ProductModel product) async {
    await _dioClient.dio.post(ApiConstants.products, data: product.toJson());
  }

  @override
  Future<void> updateProduct(ProductModel product) async {
    await _dioClient.dio.put(
      '${ApiConstants.products}/${product.id}',
      data: product.toJson(),
    );
  }

  @override
  Future<void> deleteProduct(String id) async {
    await _dioClient.dio.delete('${ApiConstants.products}/$id');
  }
}
