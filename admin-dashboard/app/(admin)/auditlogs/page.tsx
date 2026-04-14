'use client'

import { useEffect, useState, useCallback, useRef } from 'react'
import { api } from '@/lib/api'
import { formatDate } from '@/lib/utils'
import { AuditLogDto, PagedResult } from '@/types'
import { Card } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Pagination } from '@/components/ui/Pagination'
import { CheckCircle, XCircle } from 'lucide-react'

const DEFAULT_PAGE_SIZE = 10

export default function AuditLogsPage() {
  const [logs, setLogs]       = useState<AuditLogDto[]>([])
  const [total, setTotal]     = useState(0)
  const [page, setPage]       = useState(1)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const pageSizeRef = useRef(DEFAULT_PAGE_SIZE)
  pageSizeRef.current = pageSize
  const [loading, setLoading] = useState(true)
  const [action, setAction]   = useState('')

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const ps = pageSizeRef.current
      const params = new URLSearchParams({
        page: String(p),
        pageSize: String(ps),
      })
      if (action) params.set('action', action)

      const res = await api.get(`/api/auditlogs?${params}`)
      if (res.ok) {
        const json: PagedResult<AuditLogDto> = await res.json()
        setLogs(json.items)
        setTotal(json.totalCount)
      }
    } finally {
      setLoading(false)
    }
  }, [action])

  useEffect(() => {
    setPage(1)
    load(1)
  }, [action]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    load(page)
  }, [page, load])

  function handlePageSizeChange(newSize: number) {
    setPageSize(newSize)
    pageSizeRef.current = newSize
    setPage(1)
    load(1)
  }

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-bold text-gray-800">Audit Log</h1>

      <Card className="flex gap-3 items-center">
        <input
          type="text"
          placeholder="Lọc theo hành động (vd: Product, SalesOrder...)"
          value={action}
          onChange={e => setAction(e.target.value)}
          className="flex-1 text-sm text-gray-700 focus:outline-none bg-transparent"
        />
        {action && (
          <button onClick={() => setAction('')} className="text-sm text-gray-400 hover:text-gray-600">
            Xoá
          </button>
        )}
      </Card>

      <p className="text-sm text-gray-400 px-1">{total.toLocaleString()} bản ghi</p>

      <Card className="p-0 overflow-hidden">
        {loading ? (
          <div className="flex justify-center py-12"><LoadingSpinner /></div>
        ) : logs.length === 0 ? (
          <p className="text-center text-gray-400 py-8 text-sm">Không có dữ liệu</p>
        ) : (
          <ul className="divide-y divide-gray-100">
            {logs.map(log => (
              <li key={log.id} className="px-4 py-3.5">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      {log.isSuccess
                        ? <CheckCircle size={16} className="text-emerald-500 shrink-0" />
                        : <XCircle    size={16} className="text-red-500 shrink-0" />
                      }
                      <span className="text-sm font-semibold text-gray-800 truncate">
                        {log.action}
                      </span>
                    </div>
                    {log.description && (
                      <p className="text-sm text-gray-500 mt-1 ml-6 break-all">{log.description}</p>
                    )}
                    <p className="text-xs text-gray-400 mt-1.5 ml-6">
                      {log.username ?? 'Ẩn danh'}
                      {log.ipAddress ? ` · ${log.ipAddress}` : ''}
                    </p>
                  </div>
                  <span className="text-xs text-gray-400 shrink-0 mt-0.5 text-right">
                    {new Date(log.timestamp).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                    <br />
                    {formatDate(log.timestamp.slice(0, 10))}
                  </span>
                </div>
              </li>
            ))}
            {Array.from({ length: Math.max(0, pageSize - logs.length) }).map((_, i) => (
              <li key={`empty-${i}`} className="h-[60px]" />
            ))}
          </ul>
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
