// GENERATED CODE - DO NOT MODIFY BY HAND

// **************************************************************************
// InjectableConfigGenerator
// **************************************************************************

// ignore_for_file: type=lint
// coverage:ignore-file

// ignore_for_file: no_leading_underscores_for_library_prefixes
import 'package:flutter_secure_storage/flutter_secure_storage.dart' as _i3;
import 'package:get_it/get_it.dart' as _i1;
import 'package:injectable/injectable.dart' as _i2;
import 'package:logger/logger.dart' as _i4;

import '../../features/auth/data/datasources/auth_remote_datasource.dart'
    as _i22;
import '../../features/auth/data/repositories/auth_repository_impl.dart'
    as _i24;
import '../../features/auth/domain/repositories/auth_repository.dart' as _i23;
import '../../features/auth/presentation/bloc/auth_bloc.dart' as _i29;
import '../../features/inventory/data/datasources/inventory_remote_data_source.dart'
    as _i6;
import '../../features/inventory/data/repositories/inventory_repository_impl.dart'
    as _i8;
import '../../features/inventory/domain/repositories/inventory_repository.dart'
    as _i7;
import '../../features/inventory/presentation/bloc/stock_import_bloc.dart'
    as _i18;
import '../../features/invoice/data/datasources/invoice_remote_data_source.dart'
    as _i9;
import '../../features/invoice/data/repositories/invoice_repository_impl.dart'
    as _i11;
import '../../features/invoice/domain/repositories/invoice_repository.dart'
    as _i10;
import '../../features/invoice/presentation/bloc/invoice_bloc.dart' as _i25;
import '../../features/product/data/datasources/product_remote_data_source.dart'
    as _i12;
import '../../features/product/data/repositories/product_repository_impl.dart'
    as _i14;
import '../../features/product/domain/repositories/product_repository.dart'
    as _i13;
import '../../features/product/presentation/bloc/product_bloc.dart' as _i26;
import '../../features/sales_order/data/datasources/sales_order_remote_data_source.dart'
    as _i15;
import '../../features/sales_order/data/repositories/sales_order_repository_impl.dart'
    as _i17;
import '../../features/sales_order/domain/repositories/sales_order_repository.dart'
    as _i16;
import '../../features/sales_order/presentation/bloc/sales_order_bloc.dart'
    as _i27;
import '../../features/suppliers/data/datasources/supplier_remote_data_source.dart'
    as _i19;
import '../../features/suppliers/data/repositories/supplier_repository_impl.dart'
    as _i21;
import '../../features/suppliers/domain/repositories/supplier_repository.dart'
    as _i20;
import '../../features/suppliers/presentation/bloc/supplier_bloc.dart' as _i28;
import '../network/dio_client.dart' as _i5;
import 'register_module.dart' as _i30;

extension GetItInjectableX on _i1.GetIt {
// initializes the registration of main-scope dependencies inside of GetIt
  _i1.GetIt init({
    String? environment,
    _i2.EnvironmentFilter? environmentFilter,
  }) {
    final gh = _i2.GetItHelper(
      this,
      environment,
      environmentFilter,
    );
    final registerModule = _$RegisterModule();
    gh.lazySingleton<_i3.FlutterSecureStorage>(
        () => registerModule.secureStorage);
    gh.lazySingleton<_i4.Logger>(() => registerModule.logger);
    gh.lazySingleton<_i5.DioClient>(() => _i5.DioClient(
          storage: gh<_i3.FlutterSecureStorage>(),
          logger: gh<_i4.Logger>(),
        ));
    gh.lazySingleton<_i6.InventoryRemoteDataSource>(
        () => _i6.InventoryRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i7.InventoryRepository>(
        () => _i8.InventoryRepositoryImpl(gh<_i6.InventoryRemoteDataSource>()));
    gh.lazySingleton<_i9.InvoiceRemoteDataSource>(
        () => _i9.InvoiceRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i10.InvoiceRepository>(
        () => _i11.InvoiceRepositoryImpl(gh<_i9.InvoiceRemoteDataSource>()));
    gh.lazySingleton<_i12.ProductRemoteDataSource>(
        () => _i12.ProductRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i13.ProductRepository>(
        () => _i14.ProductRepositoryImpl(gh<_i12.ProductRemoteDataSource>()));
    gh.lazySingleton<_i15.SalesOrderRemoteDataSource>(
        () => _i15.SalesOrderRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i16.SalesOrderRepository>(() =>
        _i17.SalesOrderRepositoryImpl(gh<_i15.SalesOrderRemoteDataSource>()));
    gh.factory<_i18.StockImportBloc>(
        () => _i18.StockImportBloc(gh<_i7.InventoryRepository>()));
    gh.lazySingleton<_i19.SupplierRemoteDataSource>(
        () => _i19.SupplierRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i20.SupplierRepository>(
        () => _i21.SupplierRepositoryImpl(gh<_i19.SupplierRemoteDataSource>()));
    gh.lazySingleton<_i22.AuthRemoteDataSource>(
        () => _i22.AuthRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i23.AuthRepository>(() => _i24.AuthRepositoryImpl(
          gh<_i22.AuthRemoteDataSource>(),
          gh<_i3.FlutterSecureStorage>(),
        ));
    gh.factory<_i25.InvoiceBloc>(
        () => _i25.InvoiceBloc(gh<_i10.InvoiceRepository>()));
    gh.factory<_i26.ProductBloc>(
        () => _i26.ProductBloc(gh<_i13.ProductRepository>()));
    gh.factory<_i27.SalesOrderBloc>(
        () => _i27.SalesOrderBloc(gh<_i16.SalesOrderRepository>()));
    gh.factory<_i28.SupplierBloc>(
        () => _i28.SupplierBloc(gh<_i20.SupplierRepository>()));
    gh.factory<_i29.AuthBloc>(() => _i29.AuthBloc(gh<_i23.AuthRepository>()));
    return this;
  }
}

class _$RegisterModule extends _i30.RegisterModule {}
