import '../entities/notification_entity.dart';

abstract class NotificationRepository {
  Future<List<NotificationEntity>> getNotifications({
    String role = 'Seller',
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 20,
  });

  Future<void> markAsRead(String id);
  Future<void> markAllAsRead(String role);

  Future<void> startHub({
    required String role,
    required void Function(NotificationEntity) onNotification,
  });

  Future<void> stopHub();
}
