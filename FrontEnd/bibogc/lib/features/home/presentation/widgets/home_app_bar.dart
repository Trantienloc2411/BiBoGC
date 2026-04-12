import 'package:flutter/material.dart';
import '../../../../core/di/injection.dart';
import '../../../../core/network/network_mode.dart';
import '../../../../core/network/network_service.dart';

class HomeAppBar extends StatelessWidget implements PreferredSizeWidget {
  final String username;
  final VoidCallback? onLogout;

  const HomeAppBar({super.key, required this.username, this.onLogout});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final selectedMode = getIt<NetworkService>().selectedMode;
    return AppBar(
      backgroundColor: theme.colorScheme.primary,
      elevation: 0,
      toolbarHeight: 80,
      title: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Xin chào,',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onPrimary.withAlpha(204),
            ),
          ),
          const SizedBox(height: 4),
          Row(
            children: [
              Text(
                username,
                style: theme.textTheme.headlineSmall?.copyWith(
                  color: theme.colorScheme.onPrimary,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(width: 8),
              const Text('👋', style: TextStyle(fontSize: 24)),
            ],
          ),
        ],
      ),
      actions: [
        Stack(
          children: [
            IconButton(
              onPressed: () {},
              icon: Icon(
                Icons.notifications,
                color: theme.colorScheme.onPrimary,
              ),
              style: IconButton.styleFrom(
                backgroundColor: Colors.white.withAlpha(51),
                shape: const CircleBorder(),
              ),
            ),
            Positioned(
              right: 8,
              top: 8,
              child: Container(
                width: 10,
                height: 10,
                decoration: const BoxDecoration(
                  color: Colors.red,
                  shape: BoxShape.circle,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(width: 8),
        PopupMenuButton<String>(
          icon: CircleAvatar(
            radius: 18,
            backgroundColor: Colors.white.withAlpha(51),
            child: Icon(
              Icons.person,
              color: theme.colorScheme.onPrimary,
              size: 22,
            ),
          ),
          offset: const Offset(0, 56),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          itemBuilder: (context) => [
            PopupMenuItem(
              enabled: false,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    username,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    'Chế độ mạng',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurface.withAlpha(160),
                    ),
                  ),
                  const Divider(),
                ],
              ),
            ),
            CheckedPopupMenuItem(
              value: 'network_auto',
              checked: selectedMode == NetworkMode.auto,
              child: const Text('Auto'),
            ),
            CheckedPopupMenuItem(
              value: 'network_lan',
              checked: selectedMode == NetworkMode.lan,
              child: const Text('Local (LAN)'),
            ),
            CheckedPopupMenuItem(
              value: 'network_wan',
              checked: selectedMode == NetworkMode.wan,
              child: const Text('Public (WAN)'),
            ),
            const PopupMenuDivider(),
            const PopupMenuItem(
              value: 'logout',
              child: Row(
                children: [
                  Icon(Icons.logout, color: Colors.red, size: 20),
                  SizedBox(width: 12),
                  Text(
                    'Đăng xuất',
                    style: TextStyle(color: Colors.red),
                  ),
                ],
              ),
            ),
          ],
          onSelected: (value) async {
            if (value == 'logout') {
              _showLogoutDialog(context);
              return;
            }
            await _handleNetworkModeSelection(context, value);
          },
        ),
        const SizedBox(width: 8),
      ],
    );
  }

  void _showLogoutDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.logout, color: Colors.red),
            SizedBox(width: 12),
            Text('Đăng xuất'),
          ],
        ),
        content: const Text(
          'Bạn có chắc chắn muốn đăng xuất không?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Hủy'),
          ),
          FilledButton(
            onPressed: () {
              Navigator.of(ctx).pop();
              onLogout?.call();
            },
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Đăng xuất'),
          ),
        ],
      ),
    );
  }

  Future<void> _handleNetworkModeSelection(
    BuildContext context,
    String value,
  ) async {
    final networkService = getIt<NetworkService>();
    NetworkMode? mode;
    switch (value) {
      case 'network_auto':
        mode = NetworkMode.auto;
        break;
      case 'network_lan':
        mode = NetworkMode.lan;
        break;
      case 'network_wan':
        mode = NetworkMode.wan;
        break;
    }
    if (mode == null) return;
    await networkService.setMode(mode);
    if (!context.mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Đã chuyển chế độ mạng: ${mode.label}'),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  @override
  Size get preferredSize => const Size.fromHeight(80);
}
