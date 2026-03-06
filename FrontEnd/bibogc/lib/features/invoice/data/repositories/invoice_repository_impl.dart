import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/invoice/data/datasources/invoice_remote_data_source.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:bibogc/features/invoice/domain/repositories/invoice_repository.dart';
import 'package:dartz/dartz.dart';
import 'package:injectable/injectable.dart';

@LazySingleton(as: InvoiceRepository)
class InvoiceRepositoryImpl implements InvoiceRepository {
  final InvoiceRemoteDataSource _remoteDataSource;

  InvoiceRepositoryImpl(this._remoteDataSource);

  @override
  Future<Either<Failure, (List<Invoice>, bool)>> getInvoices({
    int page = 1,
    int pageSize = 20,
    DateTime? dateFrom,
    DateTime? dateTo,
  }) async {
    try {
      final result = await _remoteDataSource.getInvoices(
        page: page,
        pageSize: pageSize,
        dateFrom: dateFrom,
        dateTo: dateTo,
      );
      final invoices = result.items.map((m) => m.toEntity()).toList();
      return Right((invoices, result.hasMore));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Invoice>> getInvoiceById(String id) async {
    try {
      final model = await _remoteDataSource.getInvoiceById(id);
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }
}
