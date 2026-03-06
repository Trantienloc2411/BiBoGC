import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/invoice/domain/entities/invoice.dart';
import 'package:dartz/dartz.dart';

abstract class InvoiceRepository {
  Future<Either<Failure, (List<Invoice> invoices, bool hasMore)>> getInvoices({
    int page = 1,
    int pageSize = 20,
    DateTime? dateFrom,
    DateTime? dateTo,
  });

  Future<Either<Failure, Invoice>> getInvoiceById(String id);
}
