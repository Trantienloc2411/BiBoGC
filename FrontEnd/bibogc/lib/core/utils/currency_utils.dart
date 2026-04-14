import 'package:intl/intl.dart';

/// Centralized currency and number formatting utilities.
/// Uses comma (,) as the thousand separator to match Vietnamese POS convention.
/// e.g. 1000000 → "1,000,000đ"
class CurrencyUtils {
  static final _intFormatter = NumberFormat('#,###', 'en_US');

  /// Format as Vietnamese currency with comma thousand separator.
  /// e.g. 1,000,000đ
  static String formatCurrency(num amount) {
    return '${_intFormatter.format(amount.round())}đ';
  }

  /// Format a plain number with comma thousand separator (no currency symbol).
  /// e.g. 1,000
  static String formatNumber(num value) {
    return _intFormatter.format(value.round());
  }
}
