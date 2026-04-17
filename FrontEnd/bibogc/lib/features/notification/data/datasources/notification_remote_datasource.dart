import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:injectable/injectable.dart';
import 'package:logger/logger.dart';
import 'package:signalr_netcore/signalr_client.dart';

import '../../../../core/config/env_config.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../core/network/dio_client.dart';
import '../models/notification_model.dart';

abstract class NotificationRemoteDataSource {
  Future<List<NotificationModel>> getNotifications({
    String role = 'Seller',
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 20,
  });

  Future<void> markAsRead(String id);
  Future<void> markAllAsRead(String role);

  /// Starts the SignalR connection and calls [onNotification] for every
  /// incoming push. Call [stopHub] when done.
  Future<void> startHub({
    required String role,
    required void Function(NotificationModel) onNotification,
  });

  Future<void> stopHub();
}

@LazySingleton(as: NotificationRemoteDataSource)
class NotificationRemoteDataSourceImpl implements NotificationRemoteDataSource {
  final Dio _dio;
  final FlutterSecureStorage _storage;
  final Logger _logger;

  HubConnection? _hubConnection;

  NotificationRemoteDataSourceImpl(
    DioClient dioClient,
    this._storage,
    this._logger,
  ) : _dio = dioClient.dio;

  @override
  Future<List<NotificationModel>> getNotifications({
    String role = 'Seller',
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 20,
  }) async {
    final res = await _dio.get(
      ApiConstants.notifications,
      queryParameters: {
        'role': role,
        'unreadOnly': unreadOnly,
        'page': page,
        'pageSize': pageSize,
      },
    );
    final data = res.data['data'] as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>? ?? [];
    return items
        .map((e) => NotificationModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<void> markAsRead(String id) async {
    await _dio.put('${ApiConstants.notifications}/$id/read');
  }

  @override
  Future<void> markAllAsRead(String role) async {
    await _dio.put(
      '${ApiConstants.notifications}/read-all',
      queryParameters: {'role': role},
    );
  }

  @override
  Future<void> startHub({
    required String role,
    required void Function(NotificationModel) onNotification,
  }) async {
    if (_hubConnection != null) return;

    final baseUrl = EnvConfig.network.wanOrigin;
    final hubUrl = '$baseUrl${ApiConstants.notificationsHub}';

    final token = await _storage.read(key: 'auth_token');

    final httpOptions = HttpConnectionOptions(
      accessTokenFactory: () async => token ?? '',
      transport: HttpTransportType.WebSockets,
      logMessageContent: false,
    );

    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: httpOptions)
        .withAutomaticReconnect(
          retryDelays: [0, 2000, 5000, 10000, 30000],
        )
        .build();

    _hubConnection!.on('ReceiveNotification', (args) {
      if (args == null || args.isEmpty) return;
      try {
        final map = args[0] as Map<String, dynamic>;
        onNotification(NotificationModel.fromJson(map));
      } catch (e) {
        _logger.e('SignalR parse error: $e');
      }
    });

    _hubConnection!.onreconnected(({connectionId}) async {
      try {
        await _hubConnection!.invoke('JoinRoleGroup', args: [role]);
      } catch (_) {}
    });

    try {
      await _hubConnection!.start();
      await _hubConnection!.invoke('JoinRoleGroup', args: [role]);
      _logger.i('SignalR connected to $hubUrl as $role');
    } catch (e) {
      _logger.w('SignalR connection failed: $e — falling back to polling');
    }
  }

  @override
  Future<void> stopHub() async {
    await _hubConnection?.stop();
    _hubConnection = null;
  }
}
