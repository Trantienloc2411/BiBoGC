import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/Button'

interface PaginationProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

export function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  return (
    <div className="flex items-center justify-between px-1 pt-2">
      <Button
        size="sm"
        variant="secondary"
        onClick={() => onPageChange(Math.max(1, page - 1))}
        disabled={page === 1}
      >
        <ChevronLeft size={18} />
      </Button>
      <span className="text-sm text-gray-500">
        Trang {page} / {totalPages}
      </span>
      <Button
        size="sm"
        variant="secondary"
        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        disabled={page === totalPages}
      >
        <ChevronRight size={18} />
      </Button>
    </div>
  )
}
