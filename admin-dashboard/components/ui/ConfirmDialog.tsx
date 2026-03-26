'use client'

interface ConfirmDialogProps {
  open: boolean
  title: string
  description?: string
  icon?: React.ReactNode
  confirmLabel?: string
  cancelLabel?: string
  variant?: 'danger' | 'primary'
  loading?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmDialog({
  open,
  title,
  description,
  icon,
  confirmLabel = 'Xác nhận',
  cancelLabel = 'Huỷ',
  variant = 'primary',
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  if (!open) return null

  const confirmClass = variant === 'danger'
    ? 'bg-red-600 hover:bg-red-700'
    : 'bg-blue-600 hover:bg-blue-700'

  return (
    <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-sm p-6 text-center">
        {icon && (
          <div className="flex items-center justify-center mb-4">{icon}</div>
        )}
        <h3 className="text-base font-semibold text-gray-800 mb-1">{title}</h3>
        {description && (
          <p className="text-sm text-gray-500 mb-5">{description}</p>
        )}
        <div className="flex gap-3">
          <button
            onClick={onCancel}
            disabled={loading}
            className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 transition-colors disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <button
            onClick={onConfirm}
            disabled={loading}
            className={`flex-1 px-4 py-2.5 text-sm font-medium text-white rounded-md transition-colors disabled:opacity-50 ${confirmClass}`}
          >
            {loading ? 'Đang xử lý...' : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
