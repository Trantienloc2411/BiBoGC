import '../../domain/entities/notification_entity.dart';

class NotificationModel extends NotificationEntity {
  const NotificationModel({
    required super.id,
    required super.title,
    required super.message,
    required super.type,
    required super.role,
    required super.isRead,
    required super.createdAt,
    super.referenceId,
    super.referenceType,
  });

  factory NotificationModel.fromJson(Map<String, dynamic> json) {
    return NotificationModel(
      id: json['id'] as String,
      title: json['title'] as String,
      message: json['message'] as String,
      type: json['type'] as String? ?? 'Info',
      role: json['role'] as String? ?? 'Seller',
      isRead: json['isRead'] as bool? ?? false,
      createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
          DateTime.now(),
      referenceId: json['referenceId'] as String?,
      referenceType: json['referenceType'] as String?,
    );
  }

  NotificationEntity toEntity() => this;
}
