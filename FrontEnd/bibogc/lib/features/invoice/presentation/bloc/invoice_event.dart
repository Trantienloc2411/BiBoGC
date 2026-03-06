part of 'invoice_bloc.dart';

abstract class InvoiceEvent extends Equatable {
  const InvoiceEvent();

  @override
  List<Object?> get props => [];
}

class InvoicesStarted extends InvoiceEvent {
  const InvoicesStarted();
}

class InvoicesFilterChanged extends InvoiceEvent {
  final DateTime? dateFrom;
  final DateTime? dateTo;

  const InvoicesFilterChanged({this.dateFrom, this.dateTo});

  @override
  List<Object?> get props => [dateFrom, dateTo];
}

class InvoicesLoadMore extends InvoiceEvent {
  const InvoicesLoadMore();
}

class InvoiceDetailRequested extends InvoiceEvent {
  final String id;

  const InvoiceDetailRequested(this.id);

  @override
  List<Object?> get props => [id];
}
