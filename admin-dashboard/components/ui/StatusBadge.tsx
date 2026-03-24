import { cn } from '@/lib/utils'

const STATUS_STYLES: Record<string, string> = {
  Draft:      'bg-yellow-50 text-yellow-700 border-yellow-200',
  Completed:  'bg-emerald-50 text-emerald-700 border-emerald-200',
  Cancelled:  'bg-red-50 text-red-700 border-red-200',
}

const STATUS_LABELS: Record<string, string> = {
  Draft:      'Đang soạn',
  Completed:  'Hoàn thành',
  Cancelled:  'Đã huỷ',
}

export function StatusBadge({ status }: { status: string }) {
  const style = STATUS_STYLES[status] ?? 'bg-gray-50 text-gray-600 border-gray-200'
  const label = STATUS_LABELS[status] ?? status

  return (
    <span className={cn('inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-md border', style)}>
      {label}
    </span>
  )
}
