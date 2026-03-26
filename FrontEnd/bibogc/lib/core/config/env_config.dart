enum Environment { development, deployment }

class EnvConfig {
  static late Environment _current;

  static Environment get current => _current;
  static bool get isDevelopment => _current == Environment.development;
  static bool get isDeployment => _current == Environment.deployment;

  static const _baseUrls = {
    //Environment.development: 'https://10.0.2.2:7079', // Android emulator → host machine
    // Environment.development: 'http://localhost:7079', // iOS simulator
    Environment.development: 'https://c611-171-250-163-175.ngrok-free.app', // Physical device via PC hotspot
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
