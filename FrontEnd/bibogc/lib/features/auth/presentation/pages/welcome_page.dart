import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/config/app_routes.dart';

class WelcomePage extends StatelessWidget {
  const WelcomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          // Background gradient
          Container(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [Color(0xFF0F172A), Color(0xFF0D3B8E)],
                stops: [0.0, 1.0],
              ),
            ),
          ),
          // Top-right glow
          Positioned(
            top: -100,
            right: -100,
            child: Container(
              width: 320,
              height: 320,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: const Color(0xFF22C55E).withAlpha(20),
              ),
            ),
          ),
          // Bottom-left glow
          Positioned(
            bottom: -80,
            left: -80,
            child: Container(
              width: 260,
              height: 260,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white.withAlpha(8),
              ),
            ),
          ),
          // Content
          SafeArea(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 28.0),
              child: Column(
                children: [
                  const Spacer(flex: 2),

                  // App icon
                  Image.asset(
                    'assets/logos/bibo-gc-app-icon-96@2x.png',
                    width: 96,
                    height: 96,
                  ),
                  const SizedBox(height: 28),

                  // App name
                  const Text(
                    "BiBo's GC",
                    style: TextStyle(
                      fontSize: 38,
                      fontWeight: FontWeight.w800,
                      color: Colors.white,
                      letterSpacing: -1.0,
                    ),
                  ),
                  const SizedBox(height: 6),

                  // Subtitle badge
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 5),
                    decoration: BoxDecoration(
                      color: const Color(0xFF22C55E).withAlpha(30),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(
                        color: const Color(0xFF22C55E).withAlpha(80),
                      ),
                    ),
                    child: const Text(
                      'GROCERY CONTROLLER',
                      style: TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.w600,
                        color: Color(0xFF4ADE80),
                        letterSpacing: 2.5,
                      ),
                    ),
                  ),
                  const SizedBox(height: 20),

                  // Tagline
                  Text(
                    'Quản lý cửa hàng tạp hóa\nĐơn giản · Nhanh chóng · Hiệu quả',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 15,
                      color: Colors.white.withAlpha(160),
                      height: 1.6,
                    ),
                  ),

                  const Spacer(flex: 2),

                  // Feature chips
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    alignment: WrapAlignment.center,
                    children: const [
                      _FeatureChip(label: '📦  Kho hàng'),
                      _FeatureChip(label: '🧾  Đơn hàng'),
                      _FeatureChip(label: '📊  Báo cáo'),
                      _FeatureChip(label: '🔔  Cảnh báo'),
                    ],
                  ),

                  const Spacer(),

                  // CTA button
                  SizedBox(
                    width: double.infinity,
                    height: 56,
                    child: ElevatedButton(
                      onPressed: () => context.push(AppRoutes.login),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: const Color(0xFF0069D2),
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                      ),
                      child: const Text(
                        'Bắt đầu ngay',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.2,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),

                  Text(
                    'Dành cho nhân viên & quản lý cửa hàng',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.white.withAlpha(80),
                    ),
                  ),
                  const SizedBox(height: 32),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _FeatureChip extends StatelessWidget {
  final String label;

  const _FeatureChip({required this.label});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 9),
      decoration: BoxDecoration(
        color: Colors.white.withAlpha(15),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: Colors.white.withAlpha(28)),
      ),
      child: Text(
        label,
        style: const TextStyle(
          fontSize: 13,
          color: Colors.white,
          fontWeight: FontWeight.w500,
        ),
      ),
    );
  }
}
