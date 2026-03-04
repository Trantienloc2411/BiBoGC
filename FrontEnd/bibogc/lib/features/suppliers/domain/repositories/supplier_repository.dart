import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:dartz/dartz.dart';

abstract class SupplierRepository {
  Future<Either<Failure, List<Supplier>>> getSuppliers({
    int pageNumber = 1,
    int pageSize = 10,
    String? searchTerm,
    bool? isActive,
    String sortBy = 'Name',
    bool sortDescending = false,
  });

  Future<Either<Failure, Supplier>> getSupplierById(String id);

  Future<Either<Failure, void>> createSupplier(Supplier supplier);

  Future<Either<Failure, void>> updateSupplier(Supplier supplier);

  Future<Either<Failure, void>> deleteSupplier(String id);

  Future<Either<Failure, void>> activateSupplier(String id);

  Future<Either<Failure, void>> deactivateSupplier(String id);
}
