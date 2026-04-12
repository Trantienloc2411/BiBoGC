import 'dart:async';
import 'dart:io';

import 'package:equatable/equatable.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:injectable/injectable.dart';
import 'package:logger/logger.dart';

import '../config/env_config.dart';
import 'network_mode.dart';

enum ActiveNetworkRoute { lan, wan }

class NetworkStatus extends Equatable {
  const NetworkStatus({
    required this.selectedMode,
    required this.activeRoute,
    required this.baseUrl,
    this.lastProbeAt,
  });

  final NetworkMode selectedMode;
  final ActiveNetworkRoute activeRoute;
  final String baseUrl;
  final DateTime? lastProbeAt;

  @override
  List<Object?> get props => [selectedMode, activeRoute, baseUrl, lastProbeAt];
}

@lazySingleton
class NetworkService {
  static const _modeStorageKey = 'network_mode_pref';

  final FlutterSecureStorage _storage;
  final Logger _logger;
  final _statusController = StreamController<NetworkStatus>.broadcast();

  NetworkService({required FlutterSecureStorage storage, required Logger logger})
    : _storage = storage,
      _logger = logger;

  NetworkMode _selectedMode = NetworkMode.auto;
  ActiveNetworkRoute _activeRoute = ActiveNetworkRoute.wan;
  DateTime? _lastProbeAt;
  Future<void>? _ongoingResolve;
  bool _initialized = false;

  Stream<NetworkStatus> get statusStream => _statusController.stream;
  NetworkMode get selectedMode => _selectedMode;
  ActiveNetworkRoute get activeRoute => _activeRoute;
  bool get isLanActive => _activeRoute == ActiveNetworkRoute.lan;
  bool get isAutoMode => _selectedMode == NetworkMode.auto;

  String get currentBaseUrl {
    final network = EnvConfig.network;
    return switch (_activeRoute) {
      ActiveNetworkRoute.lan => network.lanOrigin,
      ActiveNetworkRoute.wan => network.wanOrigin,
    };
  }

  Future<void> initialize() async {
    if (_initialized) return;
    final stored = await _storage.read(key: _modeStorageKey);
    _selectedMode = NetworkModeX.fromStorage(stored);
    _logger.i('NetworkService initialized with mode: ${_selectedMode.label}');
    await _resolveMode(force: true, reason: 'initial startup');
    _initialized = true;
  }

  Future<void> setMode(NetworkMode mode) async {
    _selectedMode = mode;
    await _storage.write(key: _modeStorageKey, value: mode.storageValue);
    _logger.i('Network mode changed by user to: ${mode.label}');
    await _resolveMode(force: true, reason: 'manual mode switch');
  }

  Future<void> onAppResumed() async {
    await revalidateInBackground(force: true, reason: 'app resumed');
  }

  Future<void> revalidateInBackground({
    bool force = false,
    String reason = 'background revalidation',
  }) async {
    if (_selectedMode != NetworkMode.auto) return;
    final shouldCheck = force || _isProbeStale();
    if (!shouldCheck) return;

    unawaited(_resolveMode(force: force, reason: reason));
  }

  Future<void> markLanAsUnavailable() async {
    if (_activeRoute == ActiveNetworkRoute.lan) {
      _activeRoute = ActiveNetworkRoute.wan;
      _lastProbeAt = DateTime.now();
      _logger.w('LAN failed at runtime, switched active route to WAN');
      _emitStatus();
    }
  }

  bool shouldRetryOverWan({required RequestContext requestContext}) {
    return _activeRoute == ActiveNetworkRoute.lan &&
        requestContext.failoverAttempted == false;
  }

  Future<void> _resolveMode({required bool force, required String reason}) {
    if (_ongoingResolve != null) return _ongoingResolve!;
    _ongoingResolve = _resolveModeInternal(force: force, reason: reason);
    return _ongoingResolve!.whenComplete(() => _ongoingResolve = null);
  }

  Future<void> _resolveModeInternal({
    required bool force,
    required String reason,
  }) async {
    if (_selectedMode == NetworkMode.lan) {
      _activeRoute = ActiveNetworkRoute.lan;
      _lastProbeAt = DateTime.now();
      _logger.i('Using LAN (manual mode)');
      _emitStatus();
      return;
    }
    if (_selectedMode == NetworkMode.wan) {
      _activeRoute = ActiveNetworkRoute.wan;
      _lastProbeAt = DateTime.now();
      _logger.i('Using WAN (manual mode)');
      _emitStatus();
      return;
    }

    if (!force && !_isProbeStale()) return;

    final sameSubnetHint = await _isLikelySameSubnet();
    _logger.i(
      'Auto network resolve ($reason). Same subnet hint: '
      '${sameSubnetHint ?? 'unknown'}',
    );

    final isLanHealthy = await _probeLanHealth();
    _activeRoute = isLanHealthy ? ActiveNetworkRoute.lan : ActiveNetworkRoute.wan;
    _lastProbeAt = DateTime.now();
    _logger.i(
      'Auto resolve complete. Active route: '
      '${_activeRoute == ActiveNetworkRoute.lan ? 'LAN' : 'WAN'}',
    );
    _emitStatus();
  }

  bool _isProbeStale() {
    if (_lastProbeAt == null) return true;
    final elapsed = DateTime.now().difference(_lastProbeAt!);
    return elapsed >= EnvConfig.network.autoCheckInterval;
  }

  Future<bool> _probeLanHealth() async {
    final network = EnvConfig.network;
    final dio = HttpClient();
    dio.connectionTimeout = network.probeTimeout;
    try {
      final request = await dio.getUrl(
        Uri.parse('${network.lanOrigin}${network.healthEndpoint}'),
      );
      final response = await request.close().timeout(network.probeTimeout);
      final healthy = response.statusCode >= 200 && response.statusCode < 300;
      _logger.i(
        'LAN health check status: ${response.statusCode}, healthy: $healthy',
      );
      return healthy;
    } on TimeoutException {
      _logger.w('LAN health check timeout');
      return false;
    } catch (e) {
      _logger.w('LAN health check failed: $e');
      return false;
    } finally {
      dio.close(force: true);
    }
  }

  Future<bool?> _isLikelySameSubnet() async {
    try {
      final serverHost = Uri.parse(EnvConfig.network.lanOrigin).host;
      final serverIp = InternetAddress(serverHost);
      final interfaces = await NetworkInterface.list(
        type: InternetAddressType.IPv4,
        includeLoopback: false,
      );
      for (final interface in interfaces) {
        for (final address in interface.addresses) {
          if (_isSame24Subnet(address, serverIp)) return true;
        }
      }
      return false;
    } catch (e) {
      _logger.w('Unable to detect subnet hint: $e');
      return null;
    }
  }

  bool _isSame24Subnet(InternetAddress localIp, InternetAddress serverIp) {
    final localOctets = localIp.address.split('.');
    final serverOctets = serverIp.address.split('.');
    if (localOctets.length != 4 || serverOctets.length != 4) return false;
    return localOctets[0] == serverOctets[0] &&
        localOctets[1] == serverOctets[1] &&
        localOctets[2] == serverOctets[2];
  }

  void _emitStatus() {
    _statusController.add(
      NetworkStatus(
        selectedMode: _selectedMode,
        activeRoute: _activeRoute,
        baseUrl: currentBaseUrl,
        lastProbeAt: _lastProbeAt,
      ),
    );
  }
}

class RequestContext {
  const RequestContext({required this.failoverAttempted});

  final bool failoverAttempted;
}
