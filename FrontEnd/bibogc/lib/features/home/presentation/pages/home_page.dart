import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../presentation/widgets/home_app_bar.dart';
import '../../presentation/widgets/quick_actions_grid.dart';
import '../../presentation/widgets/recent_activity_list.dart';
import '../../presentation/widgets/summary_card.dart';
import '../../../../core/config/app_routes.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage>
    with SingleTickerProviderStateMixin {
  bool _isFabExpanded = false;
  late AnimationController _animationController;
  late Animation<double> _rotateAnimation;

  @override
  void initState() {
    super.initState();
    _initAnimations();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    // Re-initialize if null (safeguard for Hot Reload)
    // Note: late variables can't be null-checked easily without a wrapper,
    // but moving init to a helper allows us to call it if needed.
    // Actually, for Hot Reload adding new 'late' fields, the only fix is Hot Restart.
    // However, we can try to wrap initialization in a way that avoids the crash if we make them nullable.
  }

  void _initAnimations() {
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 200),
      vsync: this,
    );
    _rotateAnimation = Tween<double>(begin: 0.0, end: 0.5).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  void _toggleFab() {
    setState(() {
      _isFabExpanded = !_isFabExpanded;
      if (_isFabExpanded) {
        _animationController.forward();
      } else {
        _animationController.reverse();
      }
    });
  }

  void _handleFabAction(String action) {
    _toggleFab();
    switch (action) {
      case 'orders':
        context.push(AppRoutes.salesOrders);
        break;
      case 'import':
        context.push(AppRoutes.importStock);
        break;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F5),
      body: LayoutBuilder(
        builder: (context, constraints) {
          if (constraints.maxWidth > 800) {
            return _buildTabletLayout();
          }
          return _buildMobileLayout();
        },
      ),
      floatingActionButton: _buildExpandableFab(),
    );
  }

  Widget _buildMobileLayout() {
    return const SafeArea(
      child: SingleChildScrollView(
        child: Column(
          children: [
            HomeAppBar(username: 'Chủ Cửa Hàng'),
            Padding(
              padding: EdgeInsets.all(16.0),
              child: Column(
                children: [
                  SummaryCard(
                    totalRevenue: 1250000,
                    orderCount: 12,
                    growthPercentage: 8,
                  ),
                  SizedBox(height: 24),
                  QuickActionsGrid(),
                  SizedBox(height: 24),
                  RecentActivityList(),
                  SizedBox(height: 80), // Space for FAB
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTabletLayout() {
    return Row(
      children: [
        NavigationRail(
          selectedIndex: 0,
          onDestinationSelected: (int index) {
            switch (index) {
              case 0:
                context.go(AppRoutes.home);
                break;
              // case 1: // Inventory
              //   context.go('/inventory');
              //   break;
              // case 2: // Orders
              //   context.go('/orders');
              //   break;
              case 3:
                context.push(AppRoutes.suppliers);
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
              icon: Icon(Icons.inventory),
              label: Text('Kho'),
            ),
            NavigationRailDestination(
              icon: Icon(Icons.receipt_long),
              label: Text('Đơn hàng'),
            ),
            NavigationRailDestination(
              icon: Icon(Icons.people),
              label: Text('Đối tác'),
            ),
          ],
        ),
        const VerticalDivider(thickness: 1, width: 1),
        Expanded(
          child: Column(
            children: [
              const HomeAppBar(username: 'Chủ Cửa Hàng'),
              Expanded(
                child: SingleChildScrollView(
                  padding: const EdgeInsets.all(24.0),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        flex: 3,
                        child: Column(
                          children: const [
                            SummaryCard(
                              totalRevenue: 1250000,
                              orderCount: 12,
                              growthPercentage: 8,
                            ),
                            SizedBox(height: 24),
                            QuickActionsGrid(),
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

  Widget _buildExpandableFab() {
    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        if (_isFabExpanded) ...[
          _buildFabOption(
            icon: Icons.move_to_inbox,
            label: 'Nhập kho',
            onTap: () => _handleFabAction('import'),
          ),
          const SizedBox(height: 16),
          _buildFabOption(
            icon: Icons.add_shopping_cart,
            label: 'Tạo đơn hàng',
            onTap: () => _handleFabAction('orders'),
          ),
          const SizedBox(height: 16),
        ],
        FloatingActionButton(
          onPressed: _toggleFab,
          child: RotationTransition(
            turns: _rotateAnimation,
            child: const Icon(Icons.add),
          ),
        ),
      ],
    );
  }

  Widget _buildFabOption({
    required IconData icon,
    required String label,
    required VoidCallback onTap,
  }) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(8),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withAlpha(26),
                blurRadius: 4,
                offset: const Offset(0, 2),
              ),
            ],
          ),
          child: Text(
            label,
            style: const TextStyle(fontWeight: FontWeight.bold),
          ),
        ),
        const SizedBox(width: 16),
        FloatingActionButton.small(
          onPressed: onTap,
          backgroundColor: Colors.white,
          foregroundColor: Theme.of(context).colorScheme.primary,
          child: Icon(icon),
        ),
      ],
    );
  }
}
