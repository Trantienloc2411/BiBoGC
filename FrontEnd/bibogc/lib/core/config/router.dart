import 'package:go_router/go_router.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'app_routes.dart';
import '../di/injection.dart';
import '../../features/auth/presentation/pages/welcome_page.dart';
import '../../features/auth/presentation/pages/login_page.dart';
import '../../features/home/presentation/pages/home_page.dart';
import '../../features/product/presentation/bloc/product_bloc.dart';
import '../../features/product/presentation/pages/product_detail_page.dart';
import '../../features/product/presentation/pages/products_page.dart';
import '../../features/sales_order/presentation/bloc/sales_order_bloc.dart';
import '../../features/sales_order/presentation/pages/sales_orders_page.dart';
import '../../features/sales_order/presentation/pages/sales_order_detail_page.dart';
import '../../features/invoice/presentation/bloc/invoice_bloc.dart';
import '../../features/invoice/presentation/pages/invoices_page.dart';
import '../../features/invoice/presentation/pages/invoice_detail_page.dart';

late final GoRouter appRouter;

GoRouter createAppRouter({String initialLocation = AppRoutes.welcome}) {
  return GoRouter(
    initialLocation: initialLocation,
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
        routes: [
          GoRoute(
            path: ':id',
            builder: (context, state) =>
                ProductDetailPage(id: state.pathParameters['id']!),
          ),
        ],
      ),
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
}
