import 'package:bibogc/core/utils/dialog_utils.dart';
import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:bibogc/features/suppliers/presentation/bloc/supplier_bloc.dart';
import 'package:bibogc/features/suppliers/presentation/widgets/supplier_card.dart';
import 'package:bibogc/features/suppliers/presentation/widgets/supplier_form_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/config/app_routes.dart';

class SuppliersPage extends StatelessWidget {
  const SuppliersPage({super.key});

  @override
  Widget build(BuildContext context) {
    // Bloc is provided at route level (ShellRoute) so List + Detail share it.
    return const SuppliersView();
  }
}

class SuppliersView extends StatefulWidget {
  const SuppliersView({super.key});

  @override
  State<SuppliersView> createState() => _SuppliersViewState();
}

class _SuppliersViewState extends State<SuppliersView> {
  late final ScrollController _scrollController;

  @override
  void initState() {
    super.initState();
    _scrollController = ScrollController()..addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController
      ..removeListener(_onScroll)
      ..dispose();
    super.dispose();
  }

  void _onScroll() {
    if (!_scrollController.hasClients) return;

    // Trigger load-more when close to the bottom to avoid visible "hard stop".
    final threshold = 200.0;
    final position = _scrollController.position;
    final isNearBottom =
        position.pixels >= position.maxScrollExtent - threshold;
    if (!isNearBottom) return;

    context.read<SupplierBloc>().add(const SuppliersLoadMoreRequested());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: BackButton(onPressed: () => context.pop()),
        title: const Text('Nhà cung cấp'),
        centerTitle: true,
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _showAddSupplierDialog(context),
        child: const Icon(Icons.add),
      ),
      body: BlocListener<SupplierBloc, SupplierState>(
        listenWhen: (previous, current) =>
            previous.actionStatus != current.actionStatus,
        listener: (context, state) {
          if (state.actionStatus == SupplierActionStatus.failure) {
            DialogUtils.showErrorDialog(
              context,
              message: state.errorMessage ?? "Có lỗi xảy ra",
            );
          } else if (state.actionStatus == SupplierActionStatus.success) {
            DialogUtils.showSuccessDialog(
              context,
              message: state.actionMessage ?? "Thành công",
            );
          }
        },
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(8.0),
              child: Card(
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                elevation: 0,
                child: Padding(
                  padding: const EdgeInsets.all(12.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      _buildSearchBar(context),
                      const SizedBox(height: 8),
                      _buildFilters(context),
                    ],
                  ),
                ),
              ),
            ),
            Expanded(
              child: BlocBuilder<SupplierBloc, SupplierState>(
                builder: (context, state) {
                  if (state.status == SupplierStatus.loading &&
                      state.suppliers.isEmpty) {
                    return const Center(child: CircularProgressIndicator());
                  } else if (state.status == SupplierStatus.failure &&
                      state.suppliers.isEmpty) {
                    return Center(
                      child: Text(state.errorMessage ?? 'Lỗi tải dữ liệu'),
                    );
                  } else if (state.suppliers.isEmpty) {
                    return const Center(
                      child: Text('Không tìm thấy nhà cung cấp nào'),
                    );
                  }

                  return RefreshIndicator(
                    onRefresh: () async {
                      context.read<SupplierBloc>().add(
                        const SuppliersRefreshed(),
                      );
                    },
                    child: ListView.builder(
                      controller: _scrollController,
                      padding: const EdgeInsets.all(8),
                      itemCount:
                          state.suppliers.length +
                          (state.isLoadingMore ? 1 : 0),
                      itemBuilder: (context, index) {
                        if (index >= state.suppliers.length) {
                          return const Padding(
                            padding: EdgeInsets.symmetric(vertical: 16),
                            child: Center(child: CircularProgressIndicator()),
                          );
                        }

                        final supplier = state.suppliers[index];
                        return SupplierCard(
                          supplier: supplier,
                          onTap: () {
                            // Navigate to detail
                            context.push(AppRoutes.supplierDetail(supplier.id));
                          },
                          onEdit: () =>
                              _showEditSupplierDialog(context, supplier),
                          onToggleActive: () {
                            if (supplier.isActive) {
                              DialogUtils.showConfirmationDialog(
                                context,
                                title: "Vô hiệu hóa nhà cung cấp",
                                message:
                                    "Bạn có chắc muốn vô hiệu hóa nhà cung cấp này? Các giao dịch mới sẽ không thể thực hiện với nhà cung cấp này.",
                                confirmText: "Vô hiệu hóa",
                                onConfirm: () {
                                  context.read<SupplierBloc>().add(
                                    SupplierDeactivated(supplier.id),
                                  );
                                },
                              );
                            } else {
                              context.read<SupplierBloc>().add(
                                SupplierActivated(supplier.id),
                              );
                            }
                          },
                          onDelete: () {
                            DialogUtils.showConfirmationDialog(
                              context,
                              title: "Xóa nhà cung cấp",
                              message:
                                  "Bạn có chắc muốn xóa nhà cung cấp này? Hành động này không thể hoàn tác.",
                              confirmText: "Xóa",
                              onConfirm: () {
                                context.read<SupplierBloc>().add(
                                  SupplierDeleted(supplier.id),
                                );
                              },
                            );
                          },
                        );
                      },
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSearchBar(BuildContext context) {
    return TextField(
      decoration: const InputDecoration(
        hintText: 'Tìm kiếm nhà cung cấp...',
        prefixIcon: Icon(Icons.search),
      ),
      onChanged: (value) {
        context.read<SupplierBloc>().add(SuppliersSearchChanged(value));
      },
    );
  }

  Widget _buildFilters(BuildContext context) {
    return BlocBuilder<SupplierBloc, SupplierState>(
      buildWhen: (previous, current) =>
          previous.filterIsActive != current.filterIsActive,
      builder: (context, state) {
        return SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          padding: const EdgeInsets.symmetric(horizontal: 8),
          child: Row(
            children: [
              FilterChip(
                label: const Text('Tất cả'),
                selected: state.filterIsActive == null,
                onSelected: (selected) {
                  if (selected) {
                    context.read<SupplierBloc>().add(
                      const SuppliersFilterChanged(null),
                    );
                  }
                },
              ),
              const SizedBox(width: 8),
              FilterChip(
                label: const Text('Đang hoạt động'),
                selected: state.filterIsActive == true,
                onSelected: (selected) {
                  // If already selected and tapped, technically we could toggle off, but we have "All" option.
                  // Let's allowing toggling "Active" on. If unselected, go back to All? Or just stay?
                  // Standard logic: click 'Active' -> filter=true.
                  // If current is true, click 'Active' again -> maybe unfilter (null)?
                  if (state.filterIsActive == true) {
                    context.read<SupplierBloc>().add(
                      const SuppliersFilterChanged(null),
                    );
                  } else {
                    context.read<SupplierBloc>().add(
                      const SuppliersFilterChanged(true),
                    );
                  }
                },
              ),
              const SizedBox(width: 8),
              FilterChip(
                label: const Text('Ngưng hoạt động'),
                selected: state.filterIsActive == false,
                onSelected: (selected) {
                  if (state.filterIsActive == false) {
                    context.read<SupplierBloc>().add(
                      const SuppliersFilterChanged(null),
                    );
                  } else {
                    context.read<SupplierBloc>().add(
                      const SuppliersFilterChanged(false),
                    );
                  }
                },
              ),
            ],
          ),
        );
      },
    );
  }

  Future<void> _showAddSupplierDialog(BuildContext context) async {
    final supplierBloc = context.read<SupplierBloc>();
    final result = await showDialog<Supplier>(
      context: context,
      builder: (ctx) => const SupplierFormDialog(),
    );

    if (result != null) {
      supplierBloc.add(SupplierCreated(result));
    }
  }

  Future<void> _showEditSupplierDialog(
    BuildContext context,
    Supplier supplier,
  ) async {
    final supplierBloc = context.read<SupplierBloc>();
    final result = await showDialog<Supplier>(
      context: context,
      builder: (ctx) => SupplierFormDialog(supplier: supplier),
    );

    if (result != null) {
      supplierBloc.add(SupplierUpdated(result));
    }
  }
}
