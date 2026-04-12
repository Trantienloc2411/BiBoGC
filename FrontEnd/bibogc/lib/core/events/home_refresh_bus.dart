import 'dart:async';

class HomeRefreshBus {
  static final StreamController<void> _controller =
      StreamController<void>.broadcast();

  static Stream<void> get stream => _controller.stream;

  static void trigger() {
    if (!_controller.isClosed) {
      _controller.add(null);
    }
  }
}
