import '../config/env_config.dart';

class ApiConstants {
  static String get baseUrl => EnvConfig.baseUrl;

  // Auth
  static const String login = '/api/Auth/login';
  static const String refreshToken = '/api/Auth/refresh';
  static const String logout = '/api/Auth/logout';
  static const String revoke = '/api/Auth/revoke';

  // Products
  static const String products = '/api/products';
  static const String productBatches = '/batches';
  static const String productVariants = '/variants';

  // Categories
  static const String categories = '/api/categories';

  // Suppliers
  static const String suppliers = '/api/suppliers';

  // Stock Transactions
  static const String stockTransactions = '/api/stocktransactions';
  static const String purchase = '$stockTransactions/purchase';
  static const String sale = '$stockTransactions/sale';
  static const String adjustment = '$stockTransactions/adjustment';

  // Sales Orders
  static const String salesOrders = '/api/salesorders';

  // Invoices
  static const String invoices = '/api/invoices';

  // Notifications
  static const String notifications = '/api/notifications';
  static const String notificationsHub = '/hubs/notifications';
}
