import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class DialogUtils {
  static void showErrorDialog(
    BuildContext context, {
    required String message,
    String? title,
    VoidCallback? onPressed,
  }) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(
          title ?? 'Lỗi',
          style: const TextStyle(
            color: Colors.red,
            fontWeight: FontWeight.bold,
          ),
        ),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () {
              context.pop();
              onPressed?.call();
            },
            child: const Text('Đóng'),
          ),
        ],
      ),
    );
  }

  static void showSuccessDialog(
    BuildContext context, {
    required String message,
    String? title,
    VoidCallback? onPressed,
  }) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(
          title ?? 'Thành công',
          style: const TextStyle(
            color: Colors.green,
            fontWeight: FontWeight.bold,
          ),
        ),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () {
              context.pop();
              onPressed?.call();
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  static void showConfirmationDialog(
    BuildContext context, {
    required String message,
    required VoidCallback onConfirm,
    String? title,
    String confirmText = 'Đồng ý',
    String cancelText = 'Hủy',
  }) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(title ?? 'Xác nhận'),
        content: Text(message),
        actions: [
          TextButton(onPressed: () => context.pop(), child: Text(cancelText)),
          ElevatedButton(
            onPressed: () {
              context.pop();
              onConfirm();
            },
            child: Text(confirmText),
          ),
        ],
      ),
    );
  }
}
