import 'package:flutter/material.dart';

class RecentActivityList extends StatelessWidget {
  const RecentActivityList({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Gần đây',
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            TextButton(onPressed: () {}, child: const Text('Xem tất cả')),
          ],
        ),
        const SizedBox(height: 16),
        ListView.separated(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: 3,
          separatorBuilder: (_, index) => const SizedBox(height: 16),
          itemBuilder: (context, index) {
            return _buildActivityItem(context, index);
          },
        ),
      ],
    );
  }

  Widget _buildActivityItem(BuildContext context, int index) {
    // Mock data
    final items = [
      {
        'icon': Icons.receipt,
        'color': Colors.blue.shade100,
        'iconColor': Colors.blue.shade800,
        'title': 'Hóa đơn #1024',
        'subtitle': '14:20 • 3 mặt hàng',
        'amount': '45.000đ',
        'isPositive': true,
      },
      {
        'icon': Icons.add_box,
        'color': Colors.orange.shade100,
        'iconColor': Colors.orange.shade800,
        'title': 'Nhập hàng: Mì Hảo Hảo',
        'subtitle': '11:05 • 5 thùng',
        'amount': 'Nhập kho',
        'isPositive': false,
      },
      {
        'icon': Icons.receipt,
        'color': Colors.blue.shade100,
        'iconColor': Colors.blue.shade800,
        'title': 'Hóa đơn #1023',
        'subtitle': '09:45 • 1 mặt hàng',
        'amount': '12.000đ',
        'isPositive': true,
      },
    ];

    final item = items[index];
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(8),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: item['color'] as Color,
              shape: BoxShape.circle,
            ),
            child: Icon(
              item['icon'] as IconData,
              color: item['iconColor'] as Color,
              size: 24,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item['title'] as String,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Text(
                  item['subtitle'] as String,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
          if ((item['isPositive'] as bool))
            Text(
              item['amount'] as String,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            )
          else
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: (item['color'] as Color).withAlpha(128),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(
                item['amount'] as String,
                style: theme.textTheme.labelMedium?.copyWith(
                  color: Colors.brown,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
        ],
      ),
    );
  }
}
