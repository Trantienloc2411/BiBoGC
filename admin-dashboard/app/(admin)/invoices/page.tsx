'use client'

import { useEffect, useState, useCallback, useRef } from 'react'
import Link from 'next/link'
import { api } from '@/lib/api'
import { InvoiceDto, PagedResult } from '@/types'
import { formatCurrency, formatDateTime } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Pagination } from '@/components/ui/Pagination'
import { Button } from '@/components/ui/Button'
import { Eye, Download, X, Archive } from 'lucide-react'
import { downloadFile } from '@/lib/download'
import { exportInvoicesZip } from '@/lib/exportService'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'

const PAGE_SIZE = 20

const PAYMENT_LABELS: Record<string, string> = {
  Cash: 'Tiền mặt',
  BankTransfer: 'Chuyển khoản',
  QRPayment: 'QR Code',
}

// ─── Export progress overlay ─────────────────────────────────────────────────

function ExportProgressOverlay({ visible }: { visible: boolean }) {
  const [progress, setProgress] = useState(0)
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  useEffect(() => {
    if (visible) {
      setProgress(0)
      // Simulate gradual progress: fast at first, slows near 90%
      intervalRef.current = setInterval(() => {
        setProgress(prev => {
          if (prev >= 90) return prev
          const increment = prev < 40 ? 4 : prev < 70 ? 2 : 0.5
          return Math.min(prev + increment, 90)
        })
      }, 200)
    } else {
      if (intervalRef.current) clearInterval(intervalRef.current)
      setProgress(100)
    }
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [visible])

  if (!visible && progress !== 100) return null
  if (!visible) return null

  return (
    <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-sm p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
            <Archive size={20} className="text-blue-600" />
          </div>
          <div>
            <p className="text-sm font-semibold text-gray-800">Đang xuất hóa đơn...</p>
            <p className="text-xs text-gray-400 mt-0.5">Vui lòng không đóng trang này</p>
          </div>
        </div>

        {/* Progress bar */}
        <div className="w-full bg-gray-100 rounded-full h-2 overflow-hidden">
          <div
            className="h-2 bg-blue-500 rounded-full transition-all duration-200 ease-out"
            style={{ width: `${progress}%` }}
          />
        </div>
        <p className="text-xs text-gray-400 mt-2 text-right">{Math.round(progress)}%</p>

        <ul className="mt-4 space-y-1.5 text-xs text-gray-500">
          <li className="flex items-center gap-2">
            <span className={progress >= 20 ? 'text-green-500' : 'text-gray-300'}>✓</span>
            Kiểm tra đơn hàng chưa có hóa đơn
          </li>
          <li className="flex items-center gap-2">
            <span className={progress >= 45 ? 'text-green-500' : 'text-gray-300'}>✓</span>
            Tạo hóa đơn tự động (nếu cần)
          </li>
          <li className="flex items-center gap-2">
            <span className={progress >= 65 ? 'text-green-500' : 'text-gray-300'}>✓</span>
            Tạo file PDF cho từng hóa đơn
          </li>
          <li className="flex items-center gap-2">
            <span className={progress >= 85 ? 'text-green-500' : 'text-gray-300'}>✓</span>
            Nén thành file ZIP theo ngày
          </li>
        </ul>
      </div>
    </div>
  )
}

// ─── Page ────────────────────────────────────────────────────────────────────

export default function InvoicesPage() {
  const [invoices, setInvoices] = useState<InvoiceDto[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const [pendingPdf, setPendingPdf] = useState<{ id: string; number: string } | null>(null)
  const [downloading, setDownloading] = useState(false)

  const [showZipConfirm, setShowZipConfirm] = useState(false)
  const [exportingZip, setExportingZip] = useState(false)

  const { success, error: showError } = useToast()

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const params = new URLSearchParams({
        page: String(p),
        pageSize: String(PAGE_SIZE),
      })
      if (dateFrom) params.set('dateFrom', dateFrom)
      if (dateTo) params.set('dateTo', dateTo)

      const res = await api.get(`/api/invoices?${params}`)
      if (res.ok) {
        const json = await res.json()
        const data: PagedResult<InvoiceDto> = json.data ?? json
        const sorted = [...data.items].sort((a, b) =>
          new Date(b.invoiceDate).getTime() - new Date(a.invoiceDate).getTime()
        )
        setInvoices(sorted)
        setTotal(data.totalCount)
      }
    } finally {
      setLoading(false)
    }
  }, [dateFrom, dateTo])

  useEffect(() => {
    setPage(1)
    load(1)
  }, [dateFrom, dateTo]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => { load(page) }, [page, load])

  async function handleExportPdf() {
    if (!pendingPdf) return
    setDownloading(true)
    try {
      await downloadFile(
        `/api/invoices/${pendingPdf.id}/export/pdf`,
        `hoa-don-${pendingPdf.number}.pdf`
      )
    } catch {
      showError('Xuất PDF thất bại')
    } finally {
      setDownloading(false)
      setPendingPdf(null)
    }
  }

  async function handleExportZip() {
    setShowZipConfirm(false)
    setExportingZip(true)
    try {
      await exportInvoicesZip(dateFrom, dateTo)
      success('Xuất ZIP thành công', 'File đã được tải về máy của bạn.')
    } catch (err) {
      showError('Xuất ZIP thất bại', err instanceof Error ? err.message : undefined)
    } finally {
      setExportingZip(false)
    }
  }

  const totalPages = Math.ceil(total / PAGE_SIZE)

  const zipConfirmDescription = (() => {
    if (dateFrom && dateTo) return `Xuất tất cả hóa đơn từ ${dateFrom} đến ${dateTo} thành file ZIP?`
    if (dateFrom) return `Xuất tất cả hóa đơn từ ${dateFrom} thành file ZIP?`
    if (dateTo) return `Xuất tất cả hóa đơn đến ${dateTo} thành file ZIP?`
    return 'Xuất tất cả hóa đơn thành file ZIP? (Không có bộ lọc ngày — có thể mất nhiều thời gian)'
  })()

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Hoá đơn</h1>
        <Button
          size="sm"
          variant="secondary"
          onClick={() => setShowZipConfirm(true)}
          className="gap-1.5"
        >
          <Archive size={15} />
          Xuất ZIP
        </Button>
      </div>

      <Card className="flex flex-wrap gap-3 items-end">
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Từ ngày</label>
          <input
            type="date"
            value={dateFrom}
            onChange={e => setDateFrom(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Đến ngày</label>
          <input
            type="date"
            value={dateTo}
            onChange={e => setDateTo(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400"
          />
        </div>
        {(dateFrom || dateTo) && (
          <button
            onClick={() => { setDateFrom(''); setDateTo('') }}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 pb-2"
          >
            <X size={14} /> Xoá bộ lọc
          </button>
        )}
      </Card>

      <p className="text-sm text-gray-400 px-1">{total.toLocaleString()} hoá đơn</p>

      {loading ? <LoadingSpinner /> : (
        <>
          {invoices.length === 0 ? (
            <Card>
              <p className="text-center text-gray-400 py-8 text-sm">Không có hoá đơn nào</p>
            </Card>
          ) : (
            <Card className="p-0 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50/60">
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Số hoá đơn</th>
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Mã đơn hàng</th>
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Thanh toán</th>
                      <th className="text-right font-medium text-gray-500 px-4 py-3">Tổng tiền</th>
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Ngày</th>
                      <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {invoices.map(inv => (
                      <tr key={inv.id} className="hover:bg-gray-50/50 transition-colors">
                        <td className="px-4 py-3 font-medium text-gray-800">{inv.invoiceNumber}</td>
                        <td className="px-4 py-3 text-gray-600">
                          <Link href={`/orders/${inv.salesOrderId}`} className="hover:text-blue-600 hover:underline">
                            {inv.orderNumber}
                          </Link>
                        </td>
                        <td className="px-4 py-3 text-gray-500">
                          {PAYMENT_LABELS[inv.paymentMethod] ?? inv.paymentMethod}
                        </td>
                        <td className="px-4 py-3 text-right text-gray-700">{formatCurrency(inv.grandTotal)}</td>
                        <td className="px-4 py-3 text-gray-500">{formatDateTime(inv.invoiceDate)}</td>
                        <td className="px-4 py-3 text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => setPendingPdf({ id: inv.id, number: inv.invoiceNumber })}
                              className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-md transition-colors"
                              title="Tải PDF"
                            >
                              <Download size={15} />
                            </button>
                            <Link href={`/invoices/${inv.id}`}>
                              <Button size="sm" variant="ghost" className="gap-1.5">
                                <Eye size={15} /> Chi tiết
                              </Button>
                            </Link>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}

          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}

      {/* Single PDF download confirm */}
      <ConfirmDialog
        open={pendingPdf !== null}
        title="Xuất hoá đơn PDF?"
        description={`Tải hoá đơn ${pendingPdf?.number ?? ''} dạng PDF?`}
        icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Download size={24} className="text-blue-600" /></div>}
        confirmLabel="Tải xuống"
        loading={downloading}
        onConfirm={handleExportPdf}
        onCancel={() => setPendingPdf(null)}
      />

      {/* ZIP export confirm */}
      <ConfirmDialog
        open={showZipConfirm}
        title="Xuất hóa đơn ZIP?"
        description={zipConfirmDescription}
        icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Archive size={24} className="text-blue-600" /></div>}
        confirmLabel="Xuất ZIP"
        loading={false}
        onConfirm={handleExportZip}
        onCancel={() => setShowZipConfirm(false)}
      />

      {/* Export progress overlay */}
      <ExportProgressOverlay visible={exportingZip} />
    </div>
  )
}
