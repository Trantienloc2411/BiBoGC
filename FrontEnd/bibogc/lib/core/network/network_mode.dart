enum NetworkMode { auto, lan, wan }

extension NetworkModeX on NetworkMode {
  String get storageValue => name;

  String get label {
    switch (this) {
      case NetworkMode.auto:
        return 'Auto';
      case NetworkMode.lan:
        return 'Local (LAN)';
      case NetworkMode.wan:
        return 'Public (WAN)';
    }
  }

  static NetworkMode fromStorage(String? value) {
    return NetworkMode.values.firstWhere(
      (mode) => mode.storageValue == value,
      orElse: () => NetworkMode.auto,
    );
  }
}
