import 'package:bibogc/core/error/failures.dart';
import 'package:dartz/dartz.dart';

import '../entities/product.dart';

/// Contract for Product data access (domain layer).
///
/// UI and BLoC should depend on this interface instead of concrete
/// implementations so we can swap REST/local DB later without
/// breaking presentation logic.
abstract class ProductRepository {
  Future<Either<Failure, List<Product>>> getProducts({
    int pageNumber = 1,
    int pageSize = 20,
    String? searchTerm,
  });

  Future<Either<Failure, Product>> getProductById(String id);

  Future<Either<Failure, void>> createProduct(Product product);

  Future<Either<Failure, void>> updateProduct(Product product);

  Future<Either<Failure, void>> deleteProduct(String id);

  Future<Either<Failure, List<ProductVariant>>> getVariantsByProductId(String productId);
}
