part of 'supplier_bloc.dart';

abstract class SupplierEvent extends Equatable {
  const SupplierEvent();

  @override
  List<Object?> get props => [];
}

class SuppliersStarted extends SupplierEvent {}

class SuppliersRefreshed extends SupplierEvent {
  const SuppliersRefreshed();
}

class SuppliersLoadMoreRequested extends SupplierEvent {
  const SuppliersLoadMoreRequested();
}

class SuppliersSearchChanged extends SupplierEvent {
  final String searchTerm;

  const SuppliersSearchChanged(this.searchTerm);

  @override
  List<Object?> get props => [searchTerm];
}

class SuppliersFilterChanged extends SupplierEvent {
  final bool? isActive;

  const SuppliersFilterChanged(this.isActive);

  @override
  List<Object?> get props => [isActive];
}

class SupplierDetailRequested extends SupplierEvent {
  final String id;

  const SupplierDetailRequested(this.id);

  @override
  List<Object?> get props => [id];
}

class SupplierCreated extends SupplierEvent {
  final Supplier supplier;

  const SupplierCreated(this.supplier);

  @override
  List<Object?> get props => [supplier];
}

class SupplierUpdated extends SupplierEvent {
  final Supplier supplier;

  const SupplierUpdated(this.supplier);

  @override
  List<Object?> get props => [supplier];
}

class SupplierDeleted extends SupplierEvent {
  final String id;

  const SupplierDeleted(this.id);

  @override
  List<Object?> get props => [id];
}

class SupplierActivated extends SupplierEvent {
  final String id;

  const SupplierActivated(this.id);

  @override
  List<Object?> get props => [id];
}

class SupplierDeactivated extends SupplierEvent {
  final String id;

  const SupplierDeactivated(this.id);

  @override
  List<Object?> get props => [id];
}
