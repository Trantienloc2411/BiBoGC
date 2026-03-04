class ApiConstants {
  // Base URL (User requested to use testUrl)
  static const String baseUrl = 'https://bibo-s-gcs-test.onrender.com';

  // Auth
  static const String login = '/api/auth/login';

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
}
