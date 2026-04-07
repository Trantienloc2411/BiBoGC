import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/config/app_routes.dart';
import '../../../../core/config/router.dart';
import '../../../../core/di/injection.dart';
import '../../../../core/network/dio_client.dart';
import '../../../auth/domain/repositories/auth_repository.dart';
import '../../../invoice/presentation/bloc/invoice_bloc.dart';
import '../../../sales_order/presentation/bloc/sales_order_bloc.dart';
import '../../presentation/widgets/home_app_bar.dart';
import '../../presentation/widgets/quick_actions_grid.dart';
import '../../presentation/widgets/recent_activity_list.dart';
import '../../presentation/widgets/summary_card.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  // Own the bloc instances so we can refresh them without needing context.read
  late final SalesOrderBloc _salesOrderBloc;
  late final InvoiceBloc _invoiceBloc;

  // Daily summary state
  double? _totalRevenue;
  int? _orderCount;
  double? _growthPercentage;

  // Route change tracking
  GoRouterDelegate? _routerDelegate;
  bool _hasPushedAway = false;

  @override
  void initState() {
    super.initState();
    _salesOrderBloc = getIt<SalesOrderBloc>()..add(const SalesOrdersStarted());
    _invoiceBloc = getIt<InvoiceBloc>()..add(const InvoicesStarted());
    _loadDailySummary();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    // Subscribe to router delegate changes once
    if (_routerDelegate == null) {
      _routerDelegate = GoRouter.of(context).routerDelegate;
      _routerDelegate!.addListener(_onRouteChanged);
    }
  }

  @override
  void dispose() {
    _routerDelegate?.removeListener(_onRouteChanged);
    _salesOrderBloc.close();
    _invoiceBloc.close();
    super.dispose();
  }

  void _onRouteChanged() {
    if (_routerDelegate == null) return;
    // Defer to the next frame so currentConfiguration reflects the settled route.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted || _routerDelegate == null) return;
      final path = _routerDelegate!.currentConfiguration.uri.path;
      if (path == AppRoutes.home && _hasPushedAway) {
        _hasPushedAway = false;
        _refreshData();
      } else if (path.isNotEmpty && path != AppRoutes.home) {
        _hasPushedAway = true;
      }
    });
  }

  void _refreshData() {
    _salesOrderBloc.add(const SalesOrdersStarted());
    _invoiceBloc.add(const InvoicesStarted());
    _loadDailySummary();
  }

  Future<void> _loadDailySummary() async {
    try {
      final response =
          await getIt<DioClient>().dio.get('/api/finance/reports/sales/daily');
      final data = response.data as Map<String, dynamic>;
      final payload = (data['data'] ?? data) as Map<String, dynamic>;
      if (mounted) {
        setState(() {
          _totalRevenue =
              (payload['totalRevenue'] as num?)?.toDouble() ?? 0;
          _orderCount =
              (payload['transactionCount'] as num?)?.toInt() ?? 0;
          _growthPercentage =
              (payload['revenueChangePercent'] as num?)?.toDouble() ?? 0;
        });
      }
    } catch (_) {
      // Leave nulls — SummaryCard shows shimmer
    }
  }

  Future<void> _handleLogout() async {
    await getIt<AuthRepository>().logout();
    appRouter.go(AppRoutes.login);
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider.value(value: _salesOrderBloc),
        BlocProvider.value(value: _invoiceBloc),
      ],
      child: Scaffold(
        backgroundColor: const Color(0xFFF5F5F5),
        body: LayoutBuilder(
          builder: (context, constraints) {
            if (constraints.maxWidth > 800) {
              return _buildTabletLayout(context);
            }
            return _buildMobileLayout(context);
          },
        ),
        floatingActionButton: _buildFab(context),
      ),
    );
  }

  Widget _buildMobileLayout(BuildContext context) {
    return SafeArea(
      child: RefreshIndicator(
        onRefresh: () async => _refreshData(),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: Column(
          children: [
            HomeAppBar(username: 'Chủ Cửa Hàng', onLogout: _handleLogout),
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                children: [
                  SummaryCard(
                    totalRevenue: _totalRevenue,
                    orderCount: _orderCount,
                    growthPercentage: _growthPercentage,
                  ),
                  const SizedBox(height: 24),
                  const QuickActionsGrid(),
                  const SizedBox(height: 24),
                  const RecentActivityList(),
                  const SizedBox(height: 80),
                ],
              ),
            ),
          ],
        ),
      ),
      ),
    );
  }

  Widget _buildTabletLayout(BuildContext context) {
    return Row(
      children: [
        NavigationRail(
          selectedIndex: 0,
          onDestinationSelected: (int index) {
            switch (index) {
              case 0:
                context.go(AppRoutes.home);
                break;
              case 1:
                context.push(AppRoutes.salesOrders);
                break;
              case 2:
                context.push(AppRoutes.invoices);
                break;
            }
          },
          labelType: NavigationRailLabelType.all,
          destinations: const [
            NavigationRailDestination(
              icon: Icon(Icons.home),
              label: Text('Trang chủ'),
            ),
            NavigationRailDestination(
              icon: Icon(Icons.receipt_long),
              label: Text('Đơn hàng'),
            ),
            NavigationRailDestination(
              icon: Icon(Icons.description),
              label: Text('Hóa đơn'),
            ),
          ],
        ),
        const VerticalDivider(thickness: 1, width: 1),
        Expanded(
          child: Column(
            children: [
              HomeAppBar(username: 'Chủ Cửa Hàng', onLogout: _handleLogout),
              Expanded(
                child: SingleChildScrollView(
                  padding: const EdgeInsets.all(24.0),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        flex: 3,
                        child: Column(
                          children: [
                            SummaryCard(
                              totalRevenue: _totalRevenue,
                              orderCount: _orderCount,
                              growthPercentage: _growthPercentage,
                            ),
                            const SizedBox(height: 24),
                            const QuickActionsGrid(),
                          ],
                        ),
                      ),
                      const SizedBox(width: 24),
                      const Expanded(flex: 2, child: RecentActivityList()),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildFab(BuildContext context) {
    return FloatingActionButton.extended(
      onPressed: () => context.push(AppRoutes.salesOrders),
      icon: const Icon(Icons.add_shopping_cart),
      label: const Text('Tạo đơn hàng'),
    );
  }
}
