import 'package:equatable/equatable.dart';

class NotificationEntity extends Equatable {
  final String id;
  final String title;
  final String message;
  final String type;
  final String role;
  final bool isRead;
  final DateTime createdAt;
  final String? referenceId;
  final String? referenceType;

  const NotificationEntity({
    required this.id,
    required this.title,
    required this.message,
    required this.type,
    required this.role,
    required this.isRead,
    required this.createdAt,
    this.referenceId,
    this.referenceType,
  });

  NotificationEntity copyWith({bool? isRead}) {
    return NotificationEntity(
      id: id,
      title: title,
      message: message,
      type: type,
      role: role,
      isRead: isRead ?? this.isRead,
      createdAt: createdAt,
      referenceId: referenceId,
      referenceType: referenceType,
    );
  }

  @override
  List<Object?> get props => [id, isRead];
}
