import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/config/app_routes.dart';
import '../../../../core/di/injection.dart';
import '../../../../core/security/biometric_auth_service.dart';
import '../../../../core/utils/dialog_utils.dart';
import '../../../../core/widgets/app_button.dart';
import '../../domain/repositories/auth_repository.dart';
import '../bloc/auth_bloc.dart';
import '../bloc/auth_event.dart';
import '../bloc/auth_state.dart';

class LoginPage extends StatelessWidget {
  const LoginPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<AuthBloc>(),
      child: const _LoginForm(),
    );
  }
}

class _LoginForm extends StatefulWidget {
  const _LoginForm();

  @override
  State<_LoginForm> createState() => _LoginFormState();
}

class _LoginFormState extends State<_LoginForm> {
  final _formKey = GlobalKey<FormState>();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _authRepository = getIt<AuthRepository>();
  final _biometricAuthService = getIt<BiometricAuthService>();
  bool _obscurePassword = true;
  bool _biometricAvailable = false;
  bool _checkingBiometric = false;
  String? _lastUsername;

  @override
  void initState() {
    super.initState();
    _initializeLoginHints();
  }

  Future<void> _initializeLoginHints() async {
    final lastUsername = await _authRepository.getLastUsername();
    final biometricAvailable = await _biometricAuthService.isAvailable();
    if (!mounted) return;
    setState(() {
      _lastUsername = (lastUsername != null && lastUsername.isNotEmpty)
          ? lastUsername
          : null;
      _biometricAvailable = biometricAvailable;
      if (_lastUsername != null && _usernameController.text.isEmpty) {
        _usernameController.text = _lastUsername!;
      }
    });
  }

  void _handleLogin(BuildContext context) {
    if (_formKey.currentState!.validate()) {
      context.read<AuthBloc>().add(
        LoginRequested(_usernameController.text, _passwordController.text),
      );
    }
  }

  Future<void> _handleBiometricAuthorize() async {
    setState(() => _checkingBiometric = true);
    final ok = await _biometricAuthService.authenticate(
      reason: 'Xác thực để truy cập tài khoản POS',
    );
    if (!mounted) return;
    setState(() => _checkingBiometric = false);
    if (!ok) {
      DialogUtils.showErrorDialog(
        context,
        title: 'Xác thực thất bại',
        message: 'Không thể xác thực sinh trắc học. Vui lòng thử lại.',
      );
      return;
    }
    final isLoggedIn = await _authRepository.isLoggedIn();
    if (!mounted) return;
    if (isLoggedIn) {
      context.go(AppRoutes.home);
      return;
    }
    DialogUtils.showErrorDialog(
      context,
      title: 'Phiên đăng nhập không tồn tại',
      message:
          'Bạn cần đăng nhập bằng mật khẩu ít nhất một lần trước khi dùng sinh trắc học.',
    );
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: BlocConsumer<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is AuthAuthenticated) {
            context.go(AppRoutes.home);
          } else if (state is AuthFailure) {
            DialogUtils.showErrorDialog(
              context,
              message: state.message,
              title: 'Đăng nhập thất bại',
            );
          }
        },
        builder: (context, state) {
          final isLoading = state is AuthLoading;

          return Column(
            children: [
              // ── Branded header ────────────────────────────────────────────
              Container(
                color: theme.colorScheme.primary,
                child: SafeArea(
                  bottom: false,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(8, 4, 20, 28),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        IconButton(
                          onPressed: () => context.pop(),
                          icon: const Icon(Icons.arrow_back_ios_new_rounded, size: 18),
                          color: theme.colorScheme.onPrimary.withAlpha(180),
                        ),
                        const SizedBox(height: 12),
                        Padding(
                          padding: const EdgeInsets.only(left: 8),
                          child: Row(
                            children: [
                              Image.asset(
                                'assets/logos/bibo-gc-app-icon-64@2x.png',
                                width: 52,
                                height: 52,
                              ),
                              const SizedBox(width: 14),
                              Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    "BiBo's GC",
                                    style: TextStyle(
                                      fontSize: 22,
                                      fontWeight: FontWeight.w800,
                                      color: theme.colorScheme.onPrimary,
                                      letterSpacing: -0.5,
                                    ),
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    'Đăng nhập tài khoản',
                                    style: TextStyle(
                                      fontSize: 13,
                                      color: theme.colorScheme.onPrimary.withAlpha(180),
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),

              // ── Form ──────────────────────────────────────────────────────
              Expanded(
                child: SingleChildScrollView(
                  padding: const EdgeInsets.fromLTRB(24, 32, 24, 24),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        // Username
                        TextFormField(
                          controller: _usernameController,
                          enabled: !isLoading,
                          textInputAction: TextInputAction.next,
                          decoration: const InputDecoration(
                            labelText: 'Tên đăng nhập',
                            hintText: 'Nhập tên nhân viên',
                            prefixIcon: Icon(Icons.person_outline_rounded),
                          ),
                          validator: (v) => (v == null || v.trim().isEmpty)
                              ? 'Vui lòng nhập tên đăng nhập'
                              : null,
                        ),
                        const SizedBox(height: 16),

                        // Password with visibility toggle
                        TextFormField(
                          controller: _passwordController,
                          enabled: !isLoading,
                          obscureText: _obscurePassword,
                          textInputAction: TextInputAction.done,
                          onFieldSubmitted: (_) => _handleLogin(context),
                          decoration: InputDecoration(
                            labelText: 'Mật khẩu',
                            hintText: 'Nhập mật khẩu',
                            prefixIcon: const Icon(Icons.lock_outline_rounded),
                            suffixIcon: IconButton(
                              onPressed: () => setState(
                                () => _obscurePassword = !_obscurePassword,
                              ),
                              icon: Icon(
                                _obscurePassword
                                    ? Icons.visibility_outlined
                                    : Icons.visibility_off_outlined,
                              ),
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                          validator: (v) => (v == null || v.isEmpty)
                              ? 'Vui lòng nhập mật khẩu'
                              : null,
                        ),
                        const SizedBox(height: 32),

                        // Login button
                        AppButton(
                          onPressed: isLoading ? null : () => _handleLogin(context),
                          isLoading: isLoading,
                          child: const Text('Đăng nhập'),
                        ),

                        // Biometric button
                        if (_biometricAvailable) ...[
                          const SizedBox(height: 12),
                          SizedBox(
                            height: 52,
                            child: OutlinedButton.icon(
                              onPressed: isLoading || _checkingBiometric
                                  ? null
                                  : _handleBiometricAuthorize,
                              icon: _checkingBiometric
                                  ? SizedBox(
                                      width: 18,
                                      height: 18,
                                      child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                        color: theme.colorScheme.primary,
                                      ),
                                    )
                                  : const Icon(Icons.fingerprint_rounded, size: 22),
                              label: Text(
                                _lastUsername == null
                                    ? 'Xác thực vân tay / Face ID'
                                    : 'Xác thực cho $_lastUsername',
                                style: const TextStyle(fontSize: 14),
                              ),
                              style: OutlinedButton.styleFrom(
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                              ),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
