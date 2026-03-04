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
    as _i16;
import '../../features/auth/data/repositories/auth_repository_impl.dart'
    as _i18;
import '../../features/auth/domain/repositories/auth_repository.dart' as _i17;
import '../../features/auth/presentation/bloc/auth_bloc.dart' as _i21;
import '../../features/inventory/data/datasources/inventory_remote_data_source.dart'
    as _i6;
import '../../features/inventory/data/repositories/inventory_repository_impl.dart'
    as _i8;
import '../../features/inventory/domain/repositories/inventory_repository.dart'
    as _i7;
import '../../features/inventory/presentation/bloc/stock_import_bloc.dart'
    as _i12;
import '../../features/product/data/datasources/product_remote_data_source.dart'
    as _i9;
import '../../features/product/data/repositories/product_repository_impl.dart'
    as _i11;
import '../../features/product/domain/repositories/product_repository.dart'
    as _i10;
import '../../features/product/presentation/bloc/product_bloc.dart' as _i19;
import '../../features/suppliers/data/datasources/supplier_remote_data_source.dart'
    as _i13;
import '../../features/suppliers/data/repositories/supplier_repository_impl.dart'
    as _i15;
import '../../features/suppliers/domain/repositories/supplier_repository.dart'
    as _i14;
import '../../features/suppliers/presentation/bloc/supplier_bloc.dart' as _i20;
import '../network/dio_client.dart' as _i5;
import 'register_module.dart' as _i22;

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
    gh.lazySingleton<_i9.ProductRemoteDataSource>(
        () => _i9.ProductRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i10.ProductRepository>(
        () => _i11.ProductRepositoryImpl(gh<_i9.ProductRemoteDataSource>()));
    gh.factory<_i12.StockImportBloc>(
        () => _i12.StockImportBloc(gh<_i7.InventoryRepository>()));
    gh.lazySingleton<_i13.SupplierRemoteDataSource>(
        () => _i13.SupplierRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i14.SupplierRepository>(
        () => _i15.SupplierRepositoryImpl(gh<_i13.SupplierRemoteDataSource>()));
    gh.lazySingleton<_i16.AuthRemoteDataSource>(
        () => _i16.AuthRemoteDataSourceImpl(gh<_i5.DioClient>()));
    gh.lazySingleton<_i17.AuthRepository>(() => _i18.AuthRepositoryImpl(
          gh<_i16.AuthRemoteDataSource>(),
          gh<_i3.FlutterSecureStorage>(),
        ));
    gh.factory<_i19.ProductBloc>(
        () => _i19.ProductBloc(gh<_i10.ProductRepository>()));
    gh.factory<_i20.SupplierBloc>(
        () => _i20.SupplierBloc(gh<_i14.SupplierRepository>()));
    gh.factory<_i21.AuthBloc>(() => _i21.AuthBloc(gh<_i17.AuthRepository>()));
    return this;
  }
}

class _$RegisterModule extends _i22.RegisterModule {}
