import 'package:go_router/go_router.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'app_routes.dart';
import '../di/injection.dart';
import '../../features/auth/presentation/pages/welcome_page.dart';
import '../../features/auth/presentation/pages/login_page.dart';
import '../../features/home/presentation/pages/home_page.dart';
import '../../features/suppliers/presentation/bloc/supplier_bloc.dart';
import '../../features/suppliers/presentation/pages/suppliers_page.dart';
import '../../features/suppliers/presentation/pages/supplier_detail_page.dart';
import '../../features/product/presentation/bloc/product_bloc.dart';
import '../../features/product/presentation/pages/products_page.dart';
import '../../features/inventory/presentation/pages/stock_import_page.dart';

final GoRouter appRouter = GoRouter(
  initialLocation: AppRoutes.welcome,
  routes: [
    GoRoute(
      path: AppRoutes.welcome,
      builder: (context, state) => const WelcomePage(),
    ),
    GoRoute(path: AppRoutes.login, builder: (context, state) => const LoginPage()),
    GoRoute(path: AppRoutes.home, builder: (context, state) => const HomePage()),
    GoRoute(
      path: AppRoutes.products,
      builder: (context, state) => BlocProvider(
        create: (_) => getIt<ProductBloc>()..add(const ProductsStarted()),
        child: const ProductsView(),
      ),
    ),
    GoRoute(
      path: AppRoutes.importStock,
      builder: (context, state) => StockImportPage(),
    ),
    ShellRoute(
      builder: (context, state, child) {
        // Route-level bloc so List + Detail share the same source of truth.
        return BlocProvider(
          create: (_) => getIt<SupplierBloc>()..add(SuppliersStarted()),
          child: child,
        );
      },
      routes: [
        GoRoute(
          path: AppRoutes.suppliers,
      builder: (context, state) => const SuppliersPage(),
      routes: [
        GoRoute(
          path: ':id',
          builder: (context, state) =>
              SupplierDetailPage(id: state.pathParameters['id']!),
            ),
          ],
        ),
      ],
    ),
  ],
);
