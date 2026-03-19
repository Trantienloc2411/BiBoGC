// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format width=80

// **************************************************************************
// InjectableConfigGenerator
// **************************************************************************

// ignore_for_file: type=lint
// coverage:ignore-file

// ignore_for_file: no_leading_underscores_for_library_prefixes
import 'package:flutter_secure_storage/flutter_secure_storage.dart' as _i558;
import 'package:get_it/get_it.dart' as _i174;
import 'package:injectable/injectable.dart' as _i526;
import 'package:logger/logger.dart' as _i974;

import '../../features/auth/data/datasources/auth_remote_datasource.dart'
    as _i161;
import '../../features/auth/data/repositories/auth_repository_impl.dart'
    as _i153;
import '../../features/auth/domain/repositories/auth_repository.dart' as _i787;
import '../../features/auth/presentation/bloc/auth_bloc.dart' as _i797;
import '../../features/inventory/data/datasources/inventory_remote_data_source.dart'
    as _i248;
import '../../features/inventory/data/repositories/inventory_repository_impl.dart'
    as _i572;
import '../../features/inventory/domain/repositories/inventory_repository.dart'
    as _i422;
import '../../features/inventory/presentation/bloc/stock_import_bloc.dart'
    as _i916;
import '../../features/invoice/data/datasources/invoice_remote_data_source.dart'
    as _i610;
import '../../features/invoice/data/repositories/invoice_repository_impl.dart'
    as _i205;
import '../../features/invoice/domain/repositories/invoice_repository.dart'
    as _i339;
import '../../features/invoice/presentation/bloc/invoice_bloc.dart' as _i269;
import '../../features/product/data/datasources/product_remote_data_source.dart'
    as _i1;
import '../../features/product/data/repositories/product_repository_impl.dart'
    as _i1040;
import '../../features/product/domain/repositories/product_repository.dart'
    as _i39;
import '../../features/product/presentation/bloc/product_bloc.dart' as _i415;
import '../../features/sales_order/data/datasources/sales_order_remote_data_source.dart'
    as _i376;
import '../../features/sales_order/data/repositories/sales_order_repository_impl.dart'
    as _i448;
import '../../features/sales_order/domain/repositories/sales_order_repository.dart'
    as _i568;
import '../../features/sales_order/presentation/bloc/sales_order_bloc.dart'
    as _i1060;
import '../../features/suppliers/data/datasources/supplier_remote_data_source.dart'
    as _i184;
import '../../features/suppliers/data/repositories/supplier_repository_impl.dart'
    as _i427;
import '../../features/suppliers/domain/repositories/supplier_repository.dart'
    as _i642;
import '../../features/suppliers/presentation/bloc/supplier_bloc.dart' as _i720;
import '../network/dio_client.dart' as _i667;
import 'register_module.dart' as _i291;

extension GetItInjectableX on _i174.GetIt {
  // initializes the registration of main-scope dependencies inside of GetIt
  _i174.GetIt init({
    String? environment,
    _i526.EnvironmentFilter? environmentFilter,
  }) {
    final gh = _i526.GetItHelper(this, environment, environmentFilter);
    final registerModule = _$RegisterModule();
    gh.lazySingleton<_i974.Logger>(() => registerModule.logger);
    gh.lazySingleton<_i558.FlutterSecureStorage>(
      () => registerModule.secureStorage,
    );
    gh.lazySingleton<_i667.DioClient>(
      () => _i667.DioClient(
        storage: gh<_i558.FlutterSecureStorage>(),
        logger: gh<_i974.Logger>(),
      ),
    );
    gh.lazySingleton<_i161.AuthRemoteDataSource>(
      () => _i161.AuthRemoteDataSourceImpl(gh<_i667.DioClient>()),
    );
    gh.lazySingleton<_i184.SupplierRemoteDataSource>(
      () => _i184.SupplierRemoteDataSourceImpl(gh<_i667.DioClient>()),
    );
    gh.lazySingleton<_i1.ProductRemoteDataSource>(
      () => _i1.ProductRemoteDataSourceImpl(gh<_i667.DioClient>()),
    );
    gh.lazySingleton<_i610.InvoiceRemoteDataSource>(
      () => _i610.InvoiceRemoteDataSourceImpl(gh<_i667.DioClient>()),
    );
    gh.lazySingleton<_i787.AuthRepository>(
      () => _i153.AuthRepositoryImpl(
        gh<_i161.AuthRemoteDataSource>(),
        gh<_i558.FlutterSecureStorage>(),
        gh<_i974.Logger>(),
      ),
    );
    gh.lazySingleton<_i339.InvoiceRepository>(
      () => _i205.InvoiceRepositoryImpl(gh<_i610.InvoiceRemoteDataSource>()),
    );
    gh.lazySingleton<_i642.SupplierRepository>(
      () => _i427.SupplierRepositoryImpl(gh<_i184.SupplierRemoteDataSource>()),
    );
    gh.lazySingleton<_i248.InventoryRemoteDataSource>(
      () => _i248.InventoryRemoteDataSourceImpl(gh<_i667.DioClient>()),
    );
    gh.factory<_i797.AuthBloc>(
      () => _i797.AuthBloc(gh<_i787.AuthRepository>()),
    );
    gh.lazySingleton<_i39.ProductRepository>(
      () => _i1040.ProductRepositoryImpl(gh<_i1.ProductRemoteDataSource>()),
    );
    gh.factory<_i720.SupplierBloc>(
      () => _i720.SupplierBloc(gh<_i642.SupplierRepository>()),
    );
    gh.lazySingleton<_i376.SalesOrderRemoteDataSource>(
      () => _i376.SalesOrderRemoteDataSourceImpl(gh<_i667.DioClient>()),
    );
    gh.factory<_i415.ProductBloc>(
      () => _i415.ProductBloc(gh<_i39.ProductRepository>()),
    );
    gh.factory<_i269.InvoiceBloc>(
      () => _i269.InvoiceBloc(gh<_i339.InvoiceRepository>()),
    );
    gh.lazySingleton<_i422.InventoryRepository>(
      () =>
          _i572.InventoryRepositoryImpl(gh<_i248.InventoryRemoteDataSource>()),
    );
    gh.lazySingleton<_i568.SalesOrderRepository>(
      () => _i448.SalesOrderRepositoryImpl(
        gh<_i376.SalesOrderRemoteDataSource>(),
      ),
    );
    gh.factory<_i916.StockImportBloc>(
      () => _i916.StockImportBloc(gh<_i422.InventoryRepository>()),
    );
    gh.factory<_i1060.SalesOrderBloc>(
      () => _i1060.SalesOrderBloc(gh<_i568.SalesOrderRepository>()),
    );
    return this;
  }
}

class _$RegisterModule extends _i291.RegisterModule {}
