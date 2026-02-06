import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/suppliers/data/datasources/supplier_remote_data_source.dart';
import 'package:bibogc/features/suppliers/data/models/supplier_model.dart';
import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:bibogc/features/suppliers/domain/repositories/supplier_repository.dart';
import 'package:dartz/dartz.dart';
import 'package:injectable/injectable.dart';

@LazySingleton(as: SupplierRepository)
class SupplierRepositoryImpl implements SupplierRepository {
  final SupplierRemoteDataSource _remoteDataSource;

  SupplierRepositoryImpl(this._remoteDataSource);

  @override
  Future<Either<Failure, List<Supplier>>> getSuppliers({
    int pageNumber = 1,
    int pageSize = 10,
    String? searchTerm,
    bool? isActive,
    String sortBy = 'Name',
    bool sortDescending = false,
  }) async {
    try {
      final supplierModels = await _remoteDataSource.getSuppliers(
        pageNumber: pageNumber,
        pageSize: pageSize,
        searchTerm: searchTerm,
        isActive: isActive,
        sortBy: sortBy,
        sortDescending: sortDescending,
      );
      return Right(supplierModels.map((m) => m.toEntity()).toList());
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Supplier>> getSupplierById(String id) async {
    try {
      final supplierModel = await _remoteDataSource.getSupplierById(id);
      return Right(supplierModel.toEntity());
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> createSupplier(Supplier supplier) async {
    try {
      // Backend typically generates the ID for new suppliers.
      // We still keep `id` in the model for consistency; an empty string is acceptable here
      // because the create API does not send `id` in the request body.
      final supplierModel = SupplierModel.fromEntity(supplier);
      await _remoteDataSource.createSupplier(supplierModel);
      return const Right(null);
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> updateSupplier(Supplier supplier) async {
    try {
      final supplierModel = SupplierModel.fromEntity(supplier);
      await _remoteDataSource.updateSupplier(supplierModel);
      return const Right(null);
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteSupplier(String id) async {
    try {
      await _remoteDataSource.deleteSupplier(id);
      return const Right(null);
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> activateSupplier(String id) async {
    try {
      await _remoteDataSource.activateSupplier(id);
      return const Right(null);
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deactivateSupplier(String id) async {
    try {
      await _remoteDataSource.deactivateSupplier(id);
      return const Right(null);
    } catch (e) {
      if (e is ServerFailure) {
        return Left(e);
      }
      return Left(ServerFailure(e.toString()));
    }
  }
}
