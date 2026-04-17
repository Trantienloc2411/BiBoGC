import 'package:injectable/injectable.dart';

import '../../domain/entities/notification_entity.dart';
import '../../domain/repositories/notification_repository.dart';
import '../datasources/notification_remote_datasource.dart';
import '../models/notification_model.dart';

@LazySingleton(as: NotificationRepository)
class NotificationRepositoryImpl implements NotificationRepository {
  final NotificationRemoteDataSource _remote;

  NotificationRepositoryImpl(this._remote);

  @override
  Future<List<NotificationEntity>> getNotifications({
    String role = 'Seller',
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 20,
  }) async {
    final models = await _remote.getNotifications(
      role: role,
      unreadOnly: unreadOnly,
      page: page,
      pageSize: pageSize,
    );
    return models;
  }

  @override
  Future<void> markAsRead(String id) => _remote.markAsRead(id);

  @override
  Future<void> markAllAsRead(String role) => _remote.markAllAsRead(role);

  @override
  Future<void> startHub({
    required String role,
    required void Function(NotificationEntity) onNotification,
  }) =>
      _remote.startHub(
        role: role,
        onNotification: (NotificationModel m) => onNotification(m),
      );

  @override
  Future<void> stopHub() => _remote.stopHub();
}
