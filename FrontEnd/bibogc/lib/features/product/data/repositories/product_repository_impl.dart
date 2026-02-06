import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/product/data/datasources/product_remote_data_source.dart';
import 'package:bibogc/features/product/data/models/product_model.dart';
import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:bibogc/features/product/domain/repositories/product_repository.dart';
import 'package:dartz/dartz.dart';
import 'package:injectable/injectable.dart';

@LazySingleton(as: ProductRepository)
class ProductRepositoryImpl implements ProductRepository {
  final ProductRemoteDataSource _remoteDataSource;

  ProductRepositoryImpl(this._remoteDataSource);

  @override
  Future<Either<Failure, List<Product>>> getProducts({
    int pageNumber = 1,
    int pageSize = 20,
    String? searchTerm,
  }) async {
    try {
      final models = await _remoteDataSource.getProducts(
        pageNumber: pageNumber,
        pageSize: pageSize,
        searchTerm: searchTerm,
      );
      return Right(models.map((m) => m.toEntity()).toList());
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Product>> getProductById(String id) async {
    try {
      final model = await _remoteDataSource.getProductById(id);
      return Right(model.toEntity());
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> createProduct(Product product) async {
    try {
      final model = ProductModel.fromEntity(product);
      await _remoteDataSource.createProduct(model);
      return const Right(null);
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> updateProduct(Product product) async {
    try {
      final model = ProductModel.fromEntity(product);
      await _remoteDataSource.updateProduct(model);
      return const Right(null);
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteProduct(String id) async {
    try {
      await _remoteDataSource.deleteProduct(id);
      return const Right(null);
    } catch (e) {
      if (e is Failure) return Left(e);
      return Left(ServerFailure(e.toString()));
    }
  }
}
