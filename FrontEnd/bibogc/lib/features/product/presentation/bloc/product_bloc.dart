import 'package:bibogc/core/error/failures.dart';
import 'package:bibogc/features/product/domain/entities/product.dart';
import 'package:bibogc/features/product/domain/repositories/product_repository.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:injectable/injectable.dart';
import 'package:rxdart/rxdart.dart';

part 'product_event.dart';
part 'product_state.dart';

@injectable
class ProductBloc extends Bloc<ProductEvent, ProductState> {
  final ProductRepository _repository;

  ProductBloc(this._repository) : super(const ProductState()) {
    on<ProductsStarted>(_onStarted);
    on<ProductsSearchChanged>(
      _onSearchChanged,
      transformer: (events, mapper) => events
          .debounceTime(const Duration(milliseconds: 400))
          .distinct()
          .switchMap(mapper),
    );
    on<ProductDetailRequested>(_onDetailRequested);
    on<ProductVariantsRequested>(_onVariantsRequested);
  }

  Future<void> _onStarted(
    ProductsStarted event,
    Emitter<ProductState> emit,
  ) async {
    emit(state.copyWith(status: ProductStatus.loading));
    await _loadProducts(emit);
  }

  Future<void> _onSearchChanged(
    ProductsSearchChanged event,
    Emitter<ProductState> emit,
  ) async {
    emit(
      state.copyWith(
        searchTerm: event.searchTerm,
        status: ProductStatus.loading,
      ),
    );
    await _loadProducts(emit);
  }

  Future<void> _onDetailRequested(
    ProductDetailRequested event,
    Emitter<ProductState> emit,
  ) async {
    emit(state.copyWith(detailStatus: ProductStatus.loading));
    final result = await _repository.getProductById(event.id);
    result.fold(
      (failure) => emit(
        state.copyWith(
          detailStatus: ProductStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (product) => emit(
        state.copyWith(
          detailStatus: ProductStatus.success,
          selectedProduct: product,
        ),
      ),
    );
  }

  Future<void> _loadProducts(Emitter<ProductState> emit) async {
    final result = await _repository.getProducts(
      pageNumber: 1,
      pageSize: 50,
      searchTerm: state.searchTerm,
    );

    result.fold(
      (failure) => emit(
        state.copyWith(
          status: ProductStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (products) => emit(
        state.copyWith(status: ProductStatus.success, products: products),
      ),
    );
  }

  String _mapFailureToMessage(Failure failure) {
    if (failure is ServerFailure) {
      return failure.message;
    }
    return 'Đã xảy ra lỗi không xác định';
  }

  Future<void> _onVariantsRequested(
    ProductVariantsRequested event,
    Emitter<ProductState> emit,
  ) async {
    emit(state.copyWith(variantStatus: ProductStatus.loading, variants: []));
    final result = await _repository.getVariantsByProductId(event.productId);
    result.fold(
      (failure) => emit(
        state.copyWith(
          variantStatus: ProductStatus.failure,
          errorMessage: _mapFailureToMessage(failure),
        ),
      ),
      (variants) => emit(
        state.copyWith(variantStatus: ProductStatus.success, variants: variants),
      ),
    );
  }
}
