enum Environment { development, deployment }

class EnvConfig {
  static late Environment _current;

  static Environment get current => _current;
  static bool get isDevelopment => _current == Environment.development;
  static bool get isDeployment => _current == Environment.deployment;

  static const _baseUrls = {
    Environment.development: 'https://bibogc.dev.localhost:7079',
    Environment.deployment: 'https://bibo-s-gcs-test.onrender.com',
  };

  static String get baseUrl => _baseUrls[_current]!;

  static void init() {
    const envName = String.fromEnvironment('ENV', defaultValue: 'deployment');
    _current = Environment.values.firstWhere(
      (e) => e.name == envName,
      orElse: () => Environment.deployment,
    );
  }
}
