import 'package:bibogc/features/suppliers/domain/entities/supplier.dart';
import 'package:flutter/material.dart';

class SupplierFormDialog extends StatefulWidget {
  final Supplier? supplier; // Null for Create, Non-null for Update

  const SupplierFormDialog({super.key, this.supplier});

  @override
  State<SupplierFormDialog> createState() => _SupplierFormDialogState();
}

class _SupplierFormDialogState extends State<SupplierFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _nameController;
  late TextEditingController _contactNameController;
  late TextEditingController _phoneController;
  late TextEditingController _addressController;
  bool _isActive = true;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.supplier?.name ?? '');
    _contactNameController = TextEditingController(
      text: widget.supplier?.contactName ?? '',
    );
    _phoneController = TextEditingController(
      text: widget.supplier?.contactPhone ?? '',
    );
    _addressController = TextEditingController(
      text: widget.supplier?.address ?? '',
    );
    _isActive = widget.supplier?.isActive ?? true;
  }

  @override
  void dispose() {
    _nameController.dispose();
    _contactNameController.dispose();
    _phoneController.dispose();
    _addressController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isEditing = widget.supplier != null;
    final theme = Theme.of(context);

    return AlertDialog(
      // Make dialog feel more spacious, especially on tablet.
      // Smaller inset = wider dialog.
      insetPadding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
      title: Text(isEditing ? 'Cập nhật Nhà cung cấp' : 'Thêm Nhà cung cấp'),
      // Use a plain ConstrainedBox instead of LayoutBuilder to avoid
      // intrinsic dimension issues inside AlertDialog.
      // Increase maxWidth + give it a comfortable minimum height so
      // the dialog feels larger.
      content: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 640, minHeight: 360),
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    labelText: 'Tên nhà cung cấp *',
                  ),
                  validator: (value) {
                    if (value == null || value.isEmpty) {
                      return 'Vui lòng nhập tên nhà cung cấp';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _contactNameController,
                  decoration: const InputDecoration(labelText: 'Người liên hệ'),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _phoneController,
                  decoration: const InputDecoration(labelText: 'Số điện thoại'),
                  keyboardType: TextInputType.phone,
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _addressController,
                  decoration: const InputDecoration(labelText: 'Địa chỉ'),
                  maxLines: 2,
                ),
                if (isEditing) ...[
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      Checkbox(
                        value: _isActive,
                        onChanged: (value) {
                          setState(() {
                            _isActive = value ?? true;
                          });
                        },
                      ),
                      const Text('Đang hoạt động'),
                    ],
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
      actionsPadding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
      actions: [
        Row(
          children: [
            Expanded(
              child: SizedBox(
                height: 48,
                child: ElevatedButton(
                  onPressed: () => Navigator.of(context).pop(),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: theme.colorScheme.error,
                    foregroundColor: theme.colorScheme.onPrimary,
                  ),
                  child: const Text('Hủy'),
                ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: SizedBox(
                height: 48,
                child: ElevatedButton(
                  onPressed: () {
                    if (_formKey.currentState!.validate()) {
                      final supplier = Supplier(
                        id:
                            widget.supplier?.id ??
                            '', // ID handled by repo/backend for new items
                        name: _nameController.text,
                        contactName: _contactNameController.text.isNotEmpty
                            ? _contactNameController.text
                            : null,
                        contactPhone: _phoneController.text.isNotEmpty
                            ? _phoneController.text
                            : null,
                        address: _addressController.text.isNotEmpty
                            ? _addressController.text
                            : null,
                        isActive: _isActive,
                      );
                      Navigator.of(context).pop(supplier);
                    }
                  },
                  child: Text(isEditing ? 'Cập nhật' : 'Thêm mới'),
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }
}
