part of 'product_bloc.dart';

enum ProductStatus { initial, loading, success, failure }

class ProductState extends Equatable {
  final ProductStatus status;
  final List<Product> products;
  final Product? selectedProduct;
  final ProductStatus detailStatus;
  final String? errorMessage;
  final String searchTerm;

  const ProductState({
    this.status = ProductStatus.initial,
    this.products = const [],
    this.selectedProduct,
    this.detailStatus = ProductStatus.initial,
    this.errorMessage,
    this.searchTerm = '',
  });

  ProductState copyWith({
    ProductStatus? status,
    List<Product>? products,
    Product? selectedProduct,
    ProductStatus? detailStatus,
    String? errorMessage,
    String? searchTerm,
  }) {
    return ProductState(
      status: status ?? this.status,
      products: products ?? this.products,
      selectedProduct: selectedProduct ?? this.selectedProduct,
      detailStatus: detailStatus ?? this.detailStatus,
      errorMessage: errorMessage,
      searchTerm: searchTerm ?? this.searchTerm,
    );
  }

  @override
  List<Object?> get props => [
    status,
    products,
    selectedProduct,
    detailStatus,
    errorMessage,
    searchTerm,
  ];
}
