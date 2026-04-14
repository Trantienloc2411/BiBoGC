'use client'

import { useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { formatNumber } from '@/lib/utils'

const DEFAULT_SIZE_OPTIONS = [10, 20, 50, 100]

export interface PaginationProps {
  page: number
  pageSize: number
  totalItems: number
  onPageChange: (page: number) => void
  onPageSizeChange?: (size: number) => void
  pageSizeOptions?: number[]
}

function buildPageNumbers(page: number, totalPages: number): (number | 'ellipsis')[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, i) => i + 1)
  }
  const result: (number | 'ellipsis')[] = [1]
  const left = Math.max(2, page - 1)
  const right = Math.min(totalPages - 1, page + 1)
  if (left > 2) result.push('ellipsis')
  for (let i = left; i <= right; i++) result.push(i)
  if (right < totalPages - 1) result.push('ellipsis')
  result.push(totalPages)
  return result
}

export function Pagination({
  page,
  pageSize,
  totalItems,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = DEFAULT_SIZE_OPTIONS,
}: PaginationProps) {
  const [jumpInput, setJumpInput] = useState('')
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize))
  const pageNums = buildPageNumbers(page, totalPages)

  const from = totalItems === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, totalItems)

  function handleJump(e: React.FormEvent) {
    e.preventDefault()
    const n = parseInt(jumpInput, 10)
    if (!isNaN(n) && n >= 1 && n <= totalPages) {
      onPageChange(n)
      setJumpInput('')
    }
  }

  return (
    <div className="flex items-center justify-between gap-3 px-4 py-2.5 flex-wrap text-xs text-gray-500 select-none min-h-[44px]">
      {/* Left: page size selector + record range */}
      <div className="flex items-center gap-3">
        {onPageSizeChange && (
          <label className="flex items-center gap-1.5 whitespace-nowrap">
            <span>Hiển thị</span>
            <select
              value={pageSize}
              onChange={e => onPageSizeChange(Number(e.target.value))}
              className="border border-gray-200 rounded px-1.5 py-0.5 text-xs text-gray-700 focus:outline-none focus:ring-1 focus:ring-blue-400 bg-white cursor-pointer"
            >
              {pageSizeOptions.map(s => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
            <span>/ trang</span>
          </label>
        )}
        <span className="text-gray-400 whitespace-nowrap">
          {totalItems === 0
            ? 'Không có bản ghi'
            : `${formatNumber(from)}–${formatNumber(to)} / ${formatNumber(totalItems)} bản ghi`}
        </span>
      </div>

      {/* Center: prev / page numbers / next */}
      <div className="flex items-center gap-0.5">
        <button
          onClick={() => onPageChange(Math.max(1, page - 1))}
          disabled={page <= 1}
          className="w-7 h-7 flex items-center justify-center rounded text-gray-500 hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          aria-label="Trang trước"
        >
          <ChevronLeft size={14} />
        </button>

        {pageNums.map((n, i) =>
          n === 'ellipsis' ? (
            <span key={`e${i}`} className="w-7 h-7 flex items-center justify-center text-gray-400 text-xs">…</span>
          ) : (
            <button
              key={n}
              onClick={() => onPageChange(n as number)}
              className={`w-7 h-7 flex items-center justify-center rounded text-xs font-medium transition-colors ${
                n === page
                  ? 'bg-blue-600 text-white shadow-sm'
                  : 'text-gray-600 hover:bg-gray-100'
              }`}
            >
              {n}
            </button>
          )
        )}

        <button
          onClick={() => onPageChange(Math.min(totalPages, page + 1))}
          disabled={page >= totalPages}
          className="w-7 h-7 flex items-center justify-center rounded text-gray-500 hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          aria-label="Trang sau"
        >
          <ChevronRight size={14} />
        </button>
      </div>

      {/* Right: jump to page */}
      <form onSubmit={handleJump} className="flex items-center gap-1.5 whitespace-nowrap">
        <span>Đến trang</span>
        <input
          type="number"
          min={1}
          max={totalPages}
          value={jumpInput}
          onChange={e => setJumpInput(e.target.value)}
          className="w-12 border border-gray-200 rounded px-1.5 py-0.5 text-center text-xs text-gray-700 focus:outline-none focus:ring-1 focus:ring-blue-400"
          placeholder="—"
        />
        <button
          type="submit"
          className="px-2 py-0.5 rounded bg-gray-100 hover:bg-gray-200 text-gray-600 transition-colors"
        >
          Đi
        </button>
      </form>
    </div>
  )
}
