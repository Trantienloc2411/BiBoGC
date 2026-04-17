import 'package:equatable/equatable.dart';
import '../../domain/entities/notification_entity.dart';

class NotificationState extends Equatable {
  final List<NotificationEntity> notifications;
  final int unreadCount;
  final bool isLoading;
  final NotificationEntity? latestPush;

  const NotificationState({
    this.notifications = const [],
    this.unreadCount = 0,
    this.isLoading = false,
    this.latestPush,
  });

  NotificationState copyWith({
    List<NotificationEntity>? notifications,
    int? unreadCount,
    bool? isLoading,
    NotificationEntity? latestPush,
  }) {
    return NotificationState(
      notifications: notifications ?? this.notifications,
      unreadCount: unreadCount ?? this.unreadCount,
      isLoading: isLoading ?? this.isLoading,
      latestPush: latestPush,
    );
  }

  @override
  List<Object?> get props => [notifications, unreadCount, isLoading, latestPush];
}
