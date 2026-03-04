/// Centralized route paths for `go_router`.
///
/// Why:
/// - avoids duplicated string literals across the app
/// - makes refactors safer (change in one place)
/// - improves discoverability for new/junior devs
class AppRoutes {
  static const welcome = '/welcome';
  static const login = '/login';
  static const home = '/home';

  static const products = '/products';
  static const suppliers = '/suppliers';
  static String supplierDetail(String id) => '$suppliers/$id';

  static const importStock = '/inventory/import';

  const AppRoutes._();
}

