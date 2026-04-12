enum Environment { development, deployment }

class NetworkEnvironmentConfig {
  const NetworkEnvironmentConfig({
    required this.lanOrigin,
    required this.wanOrigin,
    this.apiPrefix = '/api',
    this.healthEndpoint = '/api/health',
    this.autoCheckInterval = const Duration(minutes: 5),
    this.probeTimeout = const Duration(seconds: 2),
  });

  final String lanOrigin;
  final String wanOrigin;
  final String apiPrefix;
  final String healthEndpoint;
  final Duration autoCheckInterval;
  final Duration probeTimeout;

  String get lanApiBaseUrl => '$lanOrigin$apiPrefix';
  String get wanApiBaseUrl => '$wanOrigin$apiPrefix';
}

class EnvConfig {
  static late Environment _current;

  static Environment get current => _current;
  static bool get isDevelopment => _current == Environment.development;
  static bool get isDeployment => _current == Environment.deployment;

  static const _networkConfigs = {
    // run with command: flutter run --dart-define=ENV=development
    Environment.development: NetworkEnvironmentConfig(
      lanOrigin: 'http://192.168.0.233',
      wanOrigin: 'https://admin.bibogc.online',
    ),
    Environment.deployment: NetworkEnvironmentConfig(
      lanOrigin: 'http://192.168.0.233',
      wanOrigin: 'https://admin.bibogc.online',
    ),
  };

  static NetworkEnvironmentConfig get network => _networkConfigs[_current]!;
  static String get baseUrl => network.wanOrigin;

  static void init() {
    const envName = String.fromEnvironment('ENV', defaultValue: 'deployment');
    _current = Environment.values.firstWhere(
      (e) => e.name == envName,
      orElse: () => Environment.deployment,
    );
  }
}
