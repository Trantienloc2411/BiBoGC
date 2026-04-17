import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:injectable/injectable.dart';

import '../../domain/entities/notification_entity.dart';
import '../../domain/repositories/notification_repository.dart';
import 'notification_event.dart';
import 'notification_state.dart';

const _role = 'Seller';
const _pollInterval = Duration(minutes: 2);

@injectable
class NotificationBloc extends Bloc<NotificationEvent, NotificationState> {
  final NotificationRepository _repository;
  Timer? _pollTimer;

  NotificationBloc(this._repository) : super(const NotificationState()) {
    on<NotificationStarted>(_onStarted);
    on<NotificationStopped>(_onStopped);
    on<NotificationFetchRequested>(_onFetchRequested);
    on<NotificationReceived>(_onReceived);
    on<NotificationMarkAsRead>(_onMarkAsRead);
    on<NotificationMarkAllAsRead>(_onMarkAllAsRead);
  }

  Future<void> _onStarted(
    NotificationStarted event,
    Emitter<NotificationState> emit,
  ) async {
    emit(state.copyWith(isLoading: true));
    await _fetchAndEmit(emit);

    // Start SignalR
    await _repository.startHub(
      role: _role,
      onNotification: (NotificationEntity n) {
        add(NotificationReceived(n));
      },
    );

    // Fallback polling
    _pollTimer?.cancel();
    _pollTimer = Timer.periodic(_pollInterval, (_) {
      add(const NotificationFetchRequested());
    });
  }

  Future<void> _onStopped(
    NotificationStopped event,
    Emitter<NotificationState> emit,
  ) async {
    _pollTimer?.cancel();
    await _repository.stopHub();
  }

  Future<void> _onFetchRequested(
    NotificationFetchRequested event,
    Emitter<NotificationState> emit,
  ) async {
    await _fetchAndEmit(emit, silent: true);
  }

  void _onReceived(
    NotificationReceived event,
    Emitter<NotificationState> emit,
  ) {
    final n = event.notification;
    final existing = state.notifications.any((e) => e.id == n.id);
    if (existing) return;

    final updated = [n, ...state.notifications];
    emit(state.copyWith(
      notifications: updated,
      unreadCount: state.unreadCount + (n.isRead ? 0 : 1),
      latestPush: n,
    ));
  }

  Future<void> _onMarkAsRead(
    NotificationMarkAsRead event,
    Emitter<NotificationState> emit,
  ) async {
    final updated = state.notifications
        .map((n) => n.id == event.id ? n.copyWith(isRead: true) : n)
        .toList();
    final newUnread =
        (state.unreadCount - 1).clamp(0, state.notifications.length);
    emit(state.copyWith(notifications: updated, unreadCount: newUnread));
    try {
      await _repository.markAsRead(event.id);
    } catch (_) {
      add(const NotificationFetchRequested());
    }
  }

  Future<void> _onMarkAllAsRead(
    NotificationMarkAllAsRead event,
    Emitter<NotificationState> emit,
  ) async {
    final updated =
        state.notifications.map((n) => n.copyWith(isRead: true)).toList();
    emit(state.copyWith(notifications: updated, unreadCount: 0));
    try {
      await _repository.markAllAsRead(_role);
    } catch (_) {
      add(const NotificationFetchRequested());
    }
  }

  Future<void> _fetchAndEmit(
    Emitter<NotificationState> emit, {
    bool silent = false,
  }) async {
    try {
      final items = await _repository.getNotifications(role: _role);
      emit(state.copyWith(
        notifications: items,
        unreadCount: items.where((n) => !n.isRead).length,
        isLoading: false,
      ));
    } catch (_) {
      if (!silent) emit(state.copyWith(isLoading: false));
    }
  }

  @override
  Future<void> close() {
    _pollTimer?.cancel();
    _repository.stopHub();
    return super.close();
  }
}
