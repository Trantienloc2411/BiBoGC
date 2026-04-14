import 'package:bibogc/core/di/injection.dart';
import 'package:bibogc/core/utils/currency_utils.dart';
import 'package:bibogc/core/network/dio_client.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class ProductDetailPage extends StatefulWidget {
  final String id;

  const ProductDetailPage({super.key, required this.id});

  @override
  State<ProductDetailPage> createState() => _ProductDetailPageState();
}

class _ProductDetailPageState extends State<ProductDetailPage>
    with SingleTickerProviderStateMixin {
  late final TabController _tabController;

  bool _loading = true;
  String? _error;
  Map<String, dynamic>? _product;
  List<Map<String, dynamic>> _batches = [];
  List<Map<String, dynamic>> _variants = [];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final dio = getIt<DioClient>().dio;
      final results = await Future.wait([
        dio.get('/api/products/${widget.id}'),
        dio.get('/api/products/${widget.id}/batches',
            queryParameters: {'pageSize': 100}),
        dio.get('/api/products/${widget.id}/variants'),
      ]);

      final batchRaw = results[1].data;
      final List batchList = batchRaw is Map
          ? ((batchRaw['items'] ?? batchRaw['value']?['items'] ?? []) as List)
          : (batchRaw as List);

      final List variantList = results[2].data is List
          ? results[2].data as List
          : ((results[2].data as Map)['value'] ?? []) as List;

      setState(() {
        _product = results[0].data as Map<String, dynamic>;
        _batches = batchList.cast<Map<String, dynamic>>();
        _variants = variantList.cast<Map<String, dynamic>>();
        _loading = false;
      });
    } catch (_) {
      setState(() {
        _error = 'Không thể tải thông tin sản phẩm';
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (_loading) {
      return Scaffold(
        appBar: AppBar(leading: const BackButton(), title: const Text('Chi tiết sản phẩm')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null || _product == null) {
      return Scaffold(
        appBar: AppBar(leading: const BackButton(), title: const Text('Chi tiết sản phẩm')),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(_error ?? 'Không tìm thấy sản phẩm'),
              const SizedBox(height: 16),
              TextButton.icon(
                icon: const Icon(Icons.refresh),
                label: const Text('Thử lại'),
                onPressed: _loadData,
              ),
            ],
          ),
        ),
      );
    }

    final p = _product!;
    final name = p['name'] as String? ?? '';
    final status = p['status'] as String? ?? 'Active';
    final sku = p['sku'] as String? ?? '';
    final description = p['description'] as String?;
    final categoryName = p['categoryName'] as String?;
    final requiresBatch = p['requiresBatchTracking'] as bool? ?? false;
    final totalStock = (p['totalStock'] as num?)?.toInt() ?? 0;
    final availableStock = (p['availableStock'] as num?)?.toInt() ?? 0;
    final expiredStock = (p['expiredStock'] as num?)?.toInt() ?? 0;
    final expiringSoon = (p['expiringSoonStock'] as num?)?.toInt() ?? 0;
    final price = (p['price'] as num?)?.toDouble() ?? 0;
    final isLowStock = p['isLowStock'] as bool? ?? false;

    return Scaffold(
      appBar: AppBar(
        leading: const BackButton(),
        title: Text(name),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadData,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _loadData,
        child: NestedScrollView(
          headerSliverBuilder: (context, _) => [
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Status + low stock chips
                    Wrap(
                      spacing: 8,
                      children: [
                        _StatusBadge(status: status),
                        if (isLowStock)
                          _InfoChip(
                            label: 'Sắp hết hàng',
                            color: Colors.amber.shade700,
                            bgColor: Colors.amber.shade50,
                          ),
                        if (requiresBatch)
                          _InfoChip(
                            label: 'Theo lô',
                            color: Colors.blue.shade700,
                            bgColor: Colors.blue.shade50,
                          ),
                      ],
                    ),
                    if (sku.isNotEmpty) ...[
                      const SizedBox(height: 4),
                      Text(
                        'SKU: $sku',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                          fontFamily: 'monospace',
                        ),
                      ),
                    ],
                    const SizedBox(height: 16),

                    // Stat cards
                    SingleChildScrollView(
                      scrollDirection: Axis.horizontal,
                      child: Row(
                        children: [
                          _StatCard(label: 'Giá bán', value: CurrencyUtils.formatCurrency(price)),
                          const SizedBox(width: 10),
                          _StatCard(label: 'Tổng tồn kho', value: '$totalStock'),
                          const SizedBox(width: 10),
                          _StatCard(
                            label: 'Có thể bán',
                            value: '$availableStock',
                            valueColor: isLowStock ? Colors.red.shade600 : null,
                          ),
                          if (requiresBatch) ...[
                            const SizedBox(width: 10),
                            _StatCard(label: 'Hết hạn', value: '$expiredStock'),
                            const SizedBox(width: 10),
                            _StatCard(label: 'Sắp hết hạn', value: '$expiringSoon'),
                          ],
                        ],
                      ),
                    ),
                    const SizedBox(height: 12),

                    // Category / description info
                    if (categoryName != null || (description != null && description.isNotEmpty)) ...[
                      Card(
                        margin: EdgeInsets.zero,
                        child: Padding(
                          padding: const EdgeInsets.all(12),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              if (categoryName != null)
                                _InfoRow(label: 'Danh mục', value: categoryName),
                              if (description != null && description.isNotEmpty) ...[
                                if (categoryName != null) const SizedBox(height: 6),
                                Text(
                                  description,
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: theme.colorScheme.onSurfaceVariant,
                                  ),
                                ),
                              ],
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),
                    ],
                  ],
                ),
              ),
            ),
            SliverPersistentHeader(
              pinned: true,
              delegate: _TabBarDelegate(
                TabBar(
                  controller: _tabController,
                  tabs: [
                    Tab(
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(Icons.inventory_2_outlined, size: 16),
                          const SizedBox(width: 6),
                          Text('Lô hàng (${_batches.length})'),
                        ],
                      ),
                    ),
                    Tab(
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(Icons.layers_outlined, size: 16),
                          const SizedBox(width: 6),
                          Text('Biến thể (${_variants.length})'),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
          body: TabBarView(
            controller: _tabController,
            children: [
              _BatchesTab(batches: _batches),
              _VariantsTab(variants: _variants),
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Batches Tab ──────────────────────────────────────────────────────────────

class _BatchesTab extends StatelessWidget {
  final List<Map<String, dynamic>> batches;

  const _BatchesTab({required this.batches});

  @override
  Widget build(BuildContext context) {
    if (batches.isEmpty) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.inventory_2_outlined, size: 48, color: Colors.grey),
            SizedBox(height: 8),
            Text('Chưa có lô hàng nào', style: TextStyle(color: Colors.grey)),
          ],
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.all(16),
      itemCount: batches.length,
      separatorBuilder: (_, i) => const SizedBox(height: 10),
      itemBuilder: (context, index) => _BatchCard(batch: batches[index]),
    );
  }
}

class _BatchCard extends StatelessWidget {
  final Map<String, dynamic> batch;

  const _BatchCard({required this.batch});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final batchNumber = batch['batchNumber'] as String? ?? '—';
    final quantity = (batch['quantity'] as num?)?.toInt() ?? 0;
    final mfDate = batch['manufacturingDate'] as String?;
    final expDate = batch['expirationDate'] as String?;
    final isExpired = batch['isExpired'] as bool? ?? false;
    final isExpiringSoon = batch['isExpiringSoon'] as bool? ?? false;
    final daysLeft = (batch['daysUntilExpiration'] as num?)?.toInt();

    Color badgeColor;
    Color badgeBg;
    String badgeLabel;
    if (isExpired) {
      badgeColor = Colors.red.shade700;
      badgeBg = Colors.red.shade50;
      badgeLabel = 'Hết hạn';
    } else if (isExpiringSoon) {
      badgeColor = Colors.orange.shade700;
      badgeBg = Colors.orange.shade50;
      badgeLabel = daysLeft != null ? 'Sắp hết hạn ($daysLeft ngày)' : 'Sắp hết hạn';
    } else {
      badgeColor = Colors.green.shade700;
      badgeBg = Colors.green.shade50;
      badgeLabel = 'Còn hạn';
    }

    return Card(
      margin: EdgeInsets.zero,
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    batchNumber,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                _InfoChip(label: badgeLabel, color: badgeColor, bgColor: badgeBg),
              ],
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                _InfoRow(label: 'Số lượng', value: '$quantity'),
                const SizedBox(width: 24),
                if (mfDate != null)
                  _InfoRow(label: 'Sản xuất', value: _formatDate(mfDate)),
                if (mfDate != null && expDate != null) const SizedBox(width: 24),
                if (expDate != null)
                  _InfoRow(label: 'Hạn sử dụng', value: _formatDate(expDate)),
              ],
            ),
          ],
        ),
      ),
    );
  }

  String _formatDate(String iso) {
    try {
      final dt = DateTime.parse(iso);
      return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year}';
    } catch (_) {
      return iso;
    }
  }
}

// ─── Variants Tab ─────────────────────────────────────────────────────────────

class _VariantsTab extends StatelessWidget {
  final List<Map<String, dynamic>> variants;

  const _VariantsTab({required this.variants});

  @override
  Widget build(BuildContext context) {
    if (variants.isEmpty) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.layers_outlined, size: 48, color: Colors.grey),
            SizedBox(height: 8),
            Text('Chưa có biến thể nào', style: TextStyle(color: Colors.grey)),
          ],
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.all(16),
      itemCount: variants.length,
      separatorBuilder: (_, i) => const SizedBox(height: 10),
      itemBuilder: (context, index) => _VariantCard(variant: variants[index]),
    );
  }
}

class _VariantCard extends StatelessWidget {
  final Map<String, dynamic> variant;

  const _VariantCard({required this.variant});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    final variantName = variant['variantName'] as String? ?? '—';
    final sku = variant['sku'] as String? ?? '';
    final barcode = variant['barcode'] as String?;
    final unitName = variant['unitName'] as String? ?? variant['unit']?.toString() ?? '';
    final quantityBase = (variant['quantityBaseUnit'] as num?)?.toInt() ?? 1;
    final salePrice = (variant['salePrice'] as num?)?.toDouble() ?? 0;
    final costPrice = (variant['costPrice'] as num?)?.toDouble();

    return Card(
      margin: EdgeInsets.zero,
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.primaryContainer.withAlpha(120),
                    shape: BoxShape.circle,
                  ),
                  child: Icon(
                    Icons.layers_outlined,
                    size: 18,
                    color: theme.colorScheme.primary,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        variantName,
                        style: theme.textTheme.titleSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      if (sku.isNotEmpty)
                        Text(
                          sku,
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                            fontFamily: 'monospace',
                          ),
                        ),
                    ],
                  ),
                ),
                Text(
                  CurrencyUtils.formatCurrency(salePrice),
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: theme.colorScheme.primary,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 10),
            Wrap(
              spacing: 16,
              runSpacing: 6,
              children: [
                _InfoRow(label: 'ĐVT', value: unitName),
                _InfoRow(label: 'Quy đổi', value: '×$quantityBase'),
                if (costPrice != null)
                  _InfoRow(label: 'Giá vốn', value: CurrencyUtils.formatCurrency(costPrice)),
                if (barcode != null && barcode.isNotEmpty)
                  _InfoRow(label: 'Barcode', value: barcode),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Shared small widgets ─────────────────────────────────────────────────────

class _StatusBadge extends StatelessWidget {
  final String status;

  const _StatusBadge({required this.status});

  static const _labels = {
    'Active': 'Đang bán',
    'Inactive': 'Ngừng bán',
    'OutOfStock': 'Hết hàng',
    'Discontinued': 'Ngừng kinh doanh',
  };

  static Color _color(String s) => switch (s) {
        'Active' => Colors.green.shade700,
        'Inactive' => Colors.grey.shade600,
        'OutOfStock' => Colors.orange.shade700,
        _ => Colors.red.shade600,
      };

  static Color _bg(String s) => switch (s) {
        'Active' => Colors.green.shade50,
        'Inactive' => Colors.grey.shade100,
        'OutOfStock' => Colors.orange.shade50,
        _ => Colors.red.shade50,
      };

  @override
  Widget build(BuildContext context) {
    return _InfoChip(
      label: _labels[status] ?? status,
      color: _color(status),
      bgColor: _bg(status),
    );
  }
}

class _InfoChip extends StatelessWidget {
  final String label;
  final Color color;
  final Color bgColor;

  const _InfoChip({
    required this.label,
    required this.color,
    required this.bgColor,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: color.withAlpha(80)),
      ),
      child: Text(
        label,
        style: TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: color,
        ),
      ),
    );
  }
}

class _StatCard extends StatelessWidget {
  final String label;
  final String value;
  final Color? valueColor;

  const _StatCard({required this.label, required this.value, this.valueColor});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      width: 110,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: theme.colorScheme.outlineVariant),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(8),
            blurRadius: 6,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: theme.textTheme.labelSmall?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
              color: valueColor ?? theme.colorScheme.onSurface,
            ),
          ),
        ],
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  final String label;
  final String value;

  const _InfoRow({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          '$label: ',
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
          ),
        ),
        Text(
          value,
          style: theme.textTheme.bodySmall?.copyWith(
            fontWeight: FontWeight.w600,
          ),
        ),
      ],
    );
  }
}

class _TabBarDelegate extends SliverPersistentHeaderDelegate {
  final TabBar tabBar;

  const _TabBarDelegate(this.tabBar);

  @override
  double get minExtent => tabBar.preferredSize.height;

  @override
  double get maxExtent => tabBar.preferredSize.height;

  @override
  Widget build(BuildContext context, double shrinkOffset, bool overlapsContent) {
    return Container(
      color: Theme.of(context).scaffoldBackgroundColor,
      child: tabBar,
    );
  }

  @override
  bool shouldRebuild(_TabBarDelegate oldDelegate) => tabBar != oldDelegate.tabBar;
}
