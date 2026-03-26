'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { api } from '@/lib/api'
import { InvoiceDto, PagedResult } from '@/types'
import { formatCurrency, formatDateTime } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Pagination } from '@/components/ui/Pagination'
import { Button } from '@/components/ui/Button'
import { Eye, Download, X } from 'lucide-react'
import { downloadFile } from '@/lib/download'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'

const PAGE_SIZE = 20

const PAYMENT_LABELS: Record<string, string> = {
  Cash: 'Tiền mặt',
  BankTransfer: 'Chuyển khoản',
  QRPayment: 'QR Code',
}

export default function InvoicesPage() {
  const [invoices, setInvoices] = useState<InvoiceDto[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const [pendingPdf, setPendingPdf] = useState<{ id: string; number: string } | null>(null)
  const [downloading, setDownloading] = useState(false)
  const { error: showError } = useToast()

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

  const totalPages = Math.ceil(total / PAGE_SIZE)

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-bold text-gray-800">Hoá đơn</h1>

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
    </div>
  )
}
