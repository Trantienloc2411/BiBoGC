import 'package:flutter/services.dart';
import 'package:intl/intl.dart';

/// A [TextInputFormatter] that formats integer inputs with comma thousand
/// separators as the user types.
/// e.g. typing "1000000" → displays "1,000,000"
///
/// To parse the value back to a number, strip commas first:
///   int.parse(controller.text.replaceAll(',', ''))
class ThousandsSeparatorFormatter extends TextInputFormatter {
  static final _formatter = NumberFormat('#,###', 'en_US');

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final text = newValue.text;
    if (text.isEmpty) return newValue;

    // Strip all non-digit characters
    final digits = text.replaceAll(RegExp(r'[^\d]'), '');
    if (digits.isEmpty) {
      return newValue.copyWith(text: '');
    }

    final number = int.tryParse(digits);
    if (number == null) return oldValue;

    final formatted = _formatter.format(number);
    return TextEditingValue(
      text: formatted,
      selection: TextSelection.collapsed(offset: formatted.length),
    );
  }
}
