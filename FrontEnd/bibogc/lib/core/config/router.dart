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
import '../../features/sales_order/presentation/bloc/sales_order_bloc.dart';
import '../../features/sales_order/presentation/pages/sales_orders_page.dart';
import '../../features/sales_order/presentation/pages/sales_order_detail_page.dart';
import '../../features/invoice/presentation/bloc/invoice_bloc.dart';
import '../../features/invoice/presentation/pages/invoices_page.dart';
import '../../features/invoice/presentation/pages/invoice_detail_page.dart';

final GoRouter appRouter = GoRouter(
  initialLocation: AppRoutes.welcome,
  routes: [
    GoRoute(
      path: AppRoutes.welcome,
      builder: (context, state) => const WelcomePage(),
    ),
    GoRoute(
      path: AppRoutes.login,
      builder: (context, state) => const LoginPage(),
    ),
    GoRoute(
      path: AppRoutes.home,
      builder: (context, state) => const HomePage(),
    ),
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
    // Suppliers — ShellRoute so list + detail share the same SupplierBloc
    ShellRoute(
      builder: (context, state, child) {
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
    // Sales Orders — ShellRoute so list + detail share the same SalesOrderBloc
    ShellRoute(
      builder: (context, state, child) {
        return BlocProvider(
          create: (_) =>
              getIt<SalesOrderBloc>()..add(const SalesOrdersStarted()),
          child: child,
        );
      },
      routes: [
        GoRoute(
          path: AppRoutes.salesOrders,
          builder: (context, state) => const SalesOrdersPage(),
          routes: [
            GoRoute(
              path: ':id',
              builder: (context, state) =>
                  SalesOrderDetailPage(id: state.pathParameters['id']!),
            ),
          ],
        ),
      ],
    ),
    // Invoices — ShellRoute so list + detail share the same InvoiceBloc
    ShellRoute(
      builder: (context, state, child) {
        return BlocProvider(
          create: (_) => getIt<InvoiceBloc>()..add(const InvoicesStarted()),
          child: child,
        );
      },
      routes: [
        GoRoute(
          path: AppRoutes.invoices,
          builder: (context, state) => const InvoicesPage(),
          routes: [
            GoRoute(
              path: ':id',
              builder: (context, state) =>
                  InvoiceDetailPage(id: state.pathParameters['id']!),
            ),
          ],
        ),
      ],
    ),
  ],
);
