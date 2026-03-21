'use client'

import { useEffect, useState, useCallback } from 'react'
import { api } from '@/lib/api'
import { formatDate } from '@/lib/utils'
import { AuditLogDto, PagedResult } from '@/types'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { ChevronLeft, ChevronRight, CheckCircle, XCircle } from 'lucide-react'

const PAGE_SIZE = 20

export default function AuditLogsPage() {
  const [logs, setLogs]       = useState<AuditLogDto[]>([])
  const [total, setTotal]     = useState(0)
  const [page, setPage]       = useState(1)
  const [loading, setLoading] = useState(true)
  const [action, setAction]   = useState('')

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const params = new URLSearchParams({
        page: String(p),
        pageSize: String(PAGE_SIZE),
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

  const totalPages = Math.ceil(total / PAGE_SIZE)

  return (
    <div className="space-y-3">
      <h1 className="text-lg font-bold text-gray-800">Audit Log</h1>

      {/* Filter */}
      <Card className="flex gap-2">
        <input
          type="text"
          placeholder="Lọc theo hành động (vd: Product, SalesOrder...)"
          value={action}
          onChange={e => setAction(e.target.value)}
          className="flex-1 text-sm text-gray-700 focus:outline-none bg-transparent"
        />
        {action && (
          <button onClick={() => setAction('')} className="text-xs text-gray-400 hover:text-gray-600">
            Xoá
          </button>
        )}
      </Card>

      {/* Total */}
      <p className="text-xs text-gray-400 px-1">{total.toLocaleString()} bản ghi</p>

      {loading ? <LoadingSpinner /> : (
        <>
          {logs.length === 0 ? (
            <Card>
              <p className="text-center text-gray-400 py-6 text-sm">Không có dữ liệu</p>
            </Card>
          ) : (
            <ul className="space-y-2">
              {logs.map(log => (
                <li key={log.id}>
                  <Card className="py-3">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-1.5">
                          {log.isSuccess
                            ? <CheckCircle size={14} className="text-emerald-500 shrink-0" />
                            : <XCircle    size={14} className="text-red-500 shrink-0" />
                          }
                          <span className="text-sm font-semibold text-gray-800 truncate">
                            {log.action}
                          </span>
                        </div>
                        {log.description && (
                          <p className="text-xs text-gray-500 mt-0.5 ml-5 break-all">{log.description}</p>
                        )}
                        <p className="text-xs text-gray-400 mt-1 ml-5">
                          {log.username ?? 'Ẩn danh'}
                          {log.ipAddress ? ` · ${log.ipAddress}` : ''}
                        </p>
                      </div>
                      <span className="text-xs text-gray-400 shrink-0 mt-0.5">
                        {new Date(log.timestamp).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                        <br />
                        {formatDate(log.timestamp.slice(0, 10))}
                      </span>
                    </div>
                  </Card>
                </li>
              ))}
            </ul>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between px-1">
              <Button
                size="sm"
                variant="secondary"
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                <ChevronLeft size={16} />
              </Button>
              <span className="text-xs text-gray-500">
                Trang {page} / {totalPages}
              </span>
              <Button
                size="sm"
                variant="secondary"
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
              >
                <ChevronRight size={16} />
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
