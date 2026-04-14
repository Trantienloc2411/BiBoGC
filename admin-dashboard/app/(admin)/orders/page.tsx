'use client'

import { useEffect, useState, useCallback, useRef } from 'react'
import Link from 'next/link'
import { api } from '@/lib/api'
import { SalesOrderSummaryDto, PagedResult, ApiResponse } from '@/types'
import { formatCurrency, formatDateTime } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { StatusBadge } from '@/components/ui/StatusBadge'
import { Pagination } from '@/components/ui/Pagination'
import { Search, Eye, FileText, X } from 'lucide-react'

const DEFAULT_PAGE_SIZE = 10

const STATUS_OPTIONS = [
  { value: '', label: 'Tất cả' },
  { value: 'Draft', label: 'Đang soạn' },
  { value: 'Completed', label: 'Hoàn thành' },
  { value: 'Cancelled', label: 'Đã huỷ' },
]

export default function OrdersPage() {
  const [orders, setOrders] = useState<SalesOrderSummaryDto[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const pageSizeRef = useRef(DEFAULT_PAGE_SIZE)
  pageSizeRef.current = pageSize
  const [loading, setLoading] = useState(true)

  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const ps = pageSizeRef.current
      const params = new URLSearchParams({
        page: String(p),
        pageSize: String(ps),
      })
      if (status) params.set('status', status)
      if (search) params.set('search', search)
      if (dateFrom) params.set('dateFrom', dateFrom)
      if (dateTo) params.set('dateTo', dateTo)

      const res = await api.get(`/api/salesorders?${params}`)
      if (res.ok) {
        const json = await res.json()
        const data: PagedResult<SalesOrderSummaryDto> = json.data ?? json
        const sorted = [...data.items].sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        )
        setOrders(sorted)
        setTotal(data.totalCount)
      }
    } finally {
      setLoading(false)
    }
  }, [status, search, dateFrom, dateTo])

  useEffect(() => {
    setPage(1)
    load(1)
  }, [status, search, dateFrom, dateTo]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => { load(page) }, [page, load])

  function handlePageSizeChange(newSize: number) {
    setPageSize(newSize)
    pageSizeRef.current = newSize
    setPage(1)
    load(1)
  }

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-bold text-gray-800">Đơn hàng</h1>

      {/* Filters */}
      <Card className="flex flex-wrap gap-3 items-end">
        <div className="flex-1 min-w-[180px]">
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Tìm kiếm</label>
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Mã đơn, khách hàng..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full text-sm text-gray-700 border border-gray-200 rounded-md pl-9 pr-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400"
            />
          </div>
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Trạng thái</label>
          <select
            value={status}
            onChange={e => setStatus(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400"
          >
            {STATUS_OPTIONS.map(o => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Từ ngày</label>
          <input
            type="date"
            value={dateFrom}
            max={dateTo || undefined}
            onChange={e => {
              const val = e.target.value
              setDateFrom(val)
              if (dateTo && val > dateTo) setDateTo('')
            }}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Đến ngày</label>
          <input
            type="date"
            value={dateTo}
            min={dateFrom || undefined}
            onChange={e => {
              const val = e.target.value
              setDateTo(val)
              if (dateFrom && val < dateFrom) setDateFrom('')
            }}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400"
          />
        </div>
        {(status || search || dateFrom || dateTo) && (
          <button
            onClick={() => { setStatus(''); setSearch(''); setDateFrom(''); setDateTo('') }}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 pb-2"
          >
            <X size={14} /> Xoá bộ lọc
          </button>
        )}
      </Card>

      <p className="text-sm text-gray-400 px-1">{total.toLocaleString()} đơn hàng</p>

      <Card className="p-0 overflow-hidden">
        {loading ? (
          <div className="flex justify-center py-12"><LoadingSpinner /></div>
        ) : orders.length === 0 ? (
          <p className="text-center text-gray-400 py-8 text-sm">Không có đơn hàng nào</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50/60">
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Mã đơn</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Trạng thái</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">Tổng tiền</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Ngày tạo</th>
                  <th className="text-center font-medium text-gray-500 px-4 py-3">Hoá đơn</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {orders.map(order => (
                  <tr key={order.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-800">{order.orderNumber}</td>
                    <td className="px-4 py-3"><StatusBadge status={order.status} /></td>
                    <td className="px-4 py-3 text-right text-gray-700">{formatCurrency(order.totalAmount)}</td>
                    <td className="px-4 py-3 text-gray-500">{formatDateTime(order.createdAt)}</td>
                    <td className="px-4 py-3 text-center">
                      {order.invoiceNumber ? (
                        <Link href={`/invoices`} className="inline-flex items-center gap-1 text-xs text-blue-600 hover:underline">
                          <FileText size={13} /> {order.invoiceNumber}
                        </Link>
                      ) : (
                        <span className="text-xs text-gray-400">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <Link href={`/orders/${order.id}`}>
                        <Button size="sm" variant="ghost" className="gap-1.5">
                          <Eye size={15} /> Chi tiết
                        </Button>
                      </Link>
                    </td>
                  </tr>
                ))}
                {Array.from({ length: Math.max(0, pageSize - orders.length) }).map((_, i) => (
                  <tr key={`empty-${i}`} className="h-[52px]"><td colSpan={6} /></tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="border-t border-gray-100">
          <Pagination
            page={page}
            pageSize={pageSize}
            totalItems={total}
            onPageChange={setPage}
            onPageSizeChange={handlePageSizeChange}
          />
        </div>
      </Card>
    </div>
  )
}
