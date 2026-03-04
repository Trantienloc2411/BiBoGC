part of 'product_bloc.dart';

abstract class ProductEvent extends Equatable {
  const ProductEvent();

  @override
  List<Object?> get props => [];
}

class ProductsStarted extends ProductEvent {
  const ProductsStarted();
}

class ProductsSearchChanged extends ProductEvent {
  final String searchTerm;

  const ProductsSearchChanged(this.searchTerm);

  @override
  List<Object?> get props => [searchTerm];
}

class ProductDetailRequested extends ProductEvent {
  final String id;

  const ProductDetailRequested(this.id);

  @override
  List<Object?> get props => [id];
}
