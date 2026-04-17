import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';

import '../../domain/entities/notification_entity.dart';
import '../bloc/notification_bloc.dart';
import '../bloc/notification_event.dart';
import '../bloc/notification_state.dart';

class NotificationBell extends StatefulWidget {
  const NotificationBell({super.key});

  @override
  State<NotificationBell> createState() => _NotificationBellState();
}

class _NotificationBellState extends State<NotificationBell> {
  @override
  Widget build(BuildContext context) {
    return BlocConsumer<NotificationBloc, NotificationState>(
      listenWhen: (prev, curr) => curr.latestPush != null && curr.latestPush != prev.latestPush,
      listener: (context, state) {
        final n = state.latestPush!;
        _showSnackBar(context, n);
      },
      builder: (context, state) {
        return Stack(
          clipBehavior: Clip.none,
          children: [
            IconButton(
              icon: const Icon(Icons.notifications_outlined),
              tooltip: 'Thông báo',
              onPressed: () => _openPanel(context),
            ),
            if (state.unreadCount > 0)
              Positioned(
                right: 6,
                top: 6,
                child: Container(
                  padding: const EdgeInsets.all(2),
                  constraints: const BoxConstraints(minWidth: 16, minHeight: 16),
                  decoration: const BoxDecoration(
                    color: Colors.red,
                    shape: BoxShape.circle,
                  ),
                  child: Text(
                    state.unreadCount > 99 ? '99+' : '${state.unreadCount}',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 9,
                      fontWeight: FontWeight.bold,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ),
              ),
          ],
        );
      },
    );
  }

  void _showSnackBar(BuildContext context, NotificationEntity n) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(n.title,
                style: const TextStyle(
                    fontWeight: FontWeight.bold, color: Colors.white)),
            Text(n.message,
                style: const TextStyle(fontSize: 12, color: Colors.white70)),
          ],
        ),
        backgroundColor: _snackBarColor(n.type),
        duration: const Duration(seconds: 4),
        behavior: SnackBarBehavior.floating,
        action: SnackBarAction(
          label: 'Xem',
          textColor: Colors.white,
          onPressed: () => _openPanel(context),
        ),
      ),
    );
  }

  Color _snackBarColor(String type) {
    switch (type) {
      case 'OutOfStock':
      case 'Error':
      case 'OrderCancelled':
        return Colors.red.shade700;
      case 'LowStock':
      case 'Warning':
        return Colors.orange.shade700;
      case 'NewOrder':
      case 'OrderCompleted':
        return Colors.green.shade700;
      default:
        return Colors.blueGrey.shade700;
    }
  }

  void _openPanel(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => BlocProvider.value(
        value: context.read<NotificationBloc>(),
        child: const _NotificationPanel(),
      ),
    );
  }
}

class _NotificationPanel extends StatelessWidget {
  const _NotificationPanel();

  @override
  Widget build(BuildContext context) {
    return DraggableScrollableSheet(
      initialChildSize: 0.7,
      maxChildSize: 0.95,
      minChildSize: 0.4,
      builder: (_, scrollController) {
        return Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: Column(
            children: [
              // Handle
              Container(
                width: 40,
                height: 4,
                margin: const EdgeInsets.symmetric(vertical: 12),
                decoration: BoxDecoration(
                  color: Colors.grey.shade300,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              // Header
              BlocBuilder<NotificationBloc, NotificationState>(
                builder: (context, state) => Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  child: Row(
                    children: [
                      const Text('Thông báo',
                          style: TextStyle(
                              fontSize: 18, fontWeight: FontWeight.bold)),
                      const Spacer(),
                      if (state.unreadCount > 0)
                        TextButton.icon(
                          onPressed: () => context
                              .read<NotificationBloc>()
                              .add(const NotificationMarkAllAsRead()),
                          icon: const Icon(Icons.done_all, size: 16),
                          label: const Text('Đọc tất cả',
                              style: TextStyle(fontSize: 12)),
                        ),
                    ],
                  ),
                ),
              ),
              const Divider(height: 1),
              // List
              Expanded(
                child: BlocBuilder<NotificationBloc, NotificationState>(
                  builder: (context, state) {
                    if (state.isLoading && state.notifications.isEmpty) {
                      return const Center(child: CircularProgressIndicator());
                    }
                    if (state.notifications.isEmpty) {
                      return const Center(
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(Icons.notifications_none,
                                size: 48, color: Colors.grey),
                            SizedBox(height: 8),
                            Text('Không có thông báo',
                                style: TextStyle(color: Colors.grey)),
                          ],
                        ),
                      );
                    }
                    return ListView.separated(
                      controller: scrollController,
                      itemCount: state.notifications.length,
                      separatorBuilder: (context, index) =>
                          const Divider(height: 1, indent: 56),
                      itemBuilder: (_, index) {
                        final n = state.notifications[index];
                        return _NotificationTile(n: n);
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}

class _NotificationTile extends StatelessWidget {
  final NotificationEntity n;
  const _NotificationTile({required this.n});

  @override
  Widget build(BuildContext context) {
    final icon = _typeIcon(n.type);
    final color = _typeColor(n.type);
    final timeStr = _timeAgo(n.createdAt);

    return InkWell(
      onTap: () {
        if (!n.isRead) {
          context
              .read<NotificationBloc>()
              .add(NotificationMarkAsRead(n.id));
        }
      },
      child: Container(
        color: n.isRead ? null : color.withValues(alpha: 0.05),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              width: 36,
              height: 36,
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.12),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, color: color, size: 18),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          n.title,
                          style: TextStyle(
                            fontWeight: n.isRead
                                ? FontWeight.normal
                                : FontWeight.bold,
                            fontSize: 13.5,
                          ),
                        ),
                      ),
                      if (!n.isRead)
                        Container(
                          width: 8,
                          height: 8,
                          decoration: BoxDecoration(
                              color: color, shape: BoxShape.circle),
                        ),
                    ],
                  ),
                  const SizedBox(height: 2),
                  Text(
                    n.message,
                    style: const TextStyle(
                        fontSize: 12.5, color: Colors.black54),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    timeStr,
                    style:
                        const TextStyle(fontSize: 11, color: Colors.black38),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  IconData _typeIcon(String type) {
    switch (type) {
      case 'NewOrder':       return Icons.shopping_cart_outlined;
      case 'OrderCancelled': return Icons.cancel_outlined;
      case 'OrderCompleted': return Icons.check_circle_outline;
      case 'StockReceived':  return Icons.inventory_2_outlined;
      case 'LowStock':       return Icons.warning_amber_outlined;
      case 'OutOfStock':     return Icons.remove_shopping_cart_outlined;
      case 'BatchAdded':     return Icons.add_box_outlined;
      case 'PriceChanged':   return Icons.sell_outlined;
      case 'ProductDiscontinued': return Icons.block_outlined;
      case 'Error':          return Icons.error_outline;
      case 'Warning':        return Icons.warning_outlined;
      default:               return Icons.notifications_outlined;
    }
  }

  Color _typeColor(String type) {
    switch (type) {
      case 'OutOfStock':
      case 'Error':
      case 'OrderCancelled':
      case 'ProductDiscontinued':
        return Colors.red;
      case 'LowStock':
      case 'Warning':
        return Colors.orange;
      case 'OrderCompleted':
      case 'StockReceived':
        return Colors.green;
      case 'NewOrder':
        return Colors.indigo;
      case 'BatchAdded':
        return Colors.blue;
      case 'PriceChanged':
        return Colors.purple;
      default:
        return Colors.blueGrey;
    }
  }

  String _timeAgo(DateTime dt) {
    final diff = DateTime.now().difference(dt);
    if (diff.inMinutes < 1) return 'Vừa xong';
    if (diff.inMinutes < 60) return '${diff.inMinutes} phút trước';
    if (diff.inHours < 24) return '${diff.inHours} giờ trước';
    if (diff.inDays < 7) return '${diff.inDays} ngày trước';
    return DateFormat('dd/MM/yyyy').format(dt);
  }
}
