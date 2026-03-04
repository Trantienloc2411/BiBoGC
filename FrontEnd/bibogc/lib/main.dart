import 'package:flutter/material.dart';
import 'app.dart';
import 'package:bibogc/core/di/injection.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await configureDependencies();
  runApp(const BiBoApp());
}
