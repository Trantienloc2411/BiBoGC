import 'package:equatable/equatable.dart';
import '../../domain/entities/notification_entity.dart';

abstract class NotificationEvent extends Equatable {
  const NotificationEvent();
  @override
  List<Object?> get props => [];
}

class NotificationStarted extends NotificationEvent {
  const NotificationStarted();
}

class NotificationStopped extends NotificationEvent {
  const NotificationStopped();
}

class NotificationFetchRequested extends NotificationEvent {
  const NotificationFetchRequested();
}

class NotificationReceived extends NotificationEvent {
  final NotificationEntity notification;
  const NotificationReceived(this.notification);
  @override
  List<Object?> get props => [notification];
}

class NotificationMarkAsRead extends NotificationEvent {
  final String id;
  const NotificationMarkAsRead(this.id);
  @override
  List<Object?> get props => [id];
}

class NotificationMarkAllAsRead extends NotificationEvent {
  const NotificationMarkAllAsRead();
}
