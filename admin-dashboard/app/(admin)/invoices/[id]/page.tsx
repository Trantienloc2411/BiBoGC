'use client'

import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import Link from 'next/link'
import { api } from '@/lib/api'
import { InvoiceDto } from '@/types'
import { formatCurrency, formatDate } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { PdfViewerModal } from '@/components/ui/PdfViewerModal'
import { downloadFile } from '@/lib/download'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { ArrowLeft, Download, Eye, Store, Phone } from 'lucide-react'

const VAT_RATE = 0.01
const PIT_RATE = 0.005

const PAYMENT_LABELS: Record<string, string> = {
  Cash: 'Tiền mặt',
  BankTransfer: 'Chuyển khoản',
  QRPayment: 'QR Code',
}

export default function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { error: showError } = useToast()

  const [invoice, setInvoice] = useState<InvoiceDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [showPdf, setShowPdf] = useState(false)
  const [showDownloadConfirm, setShowDownloadConfirm] = useState(false)
  const [downloading, setDownloading] = useState(false)

  useEffect(() => {
    async function load() {
      setLoading(true)
      setError(null)
      try {
        const res = await api.get(`/api/invoices/${id}`)
        if (res.ok) {
          const json = await res.json()
          setInvoice(json.data ?? json)
        } else {
          setError('Không thể tải chi tiết hoá đơn.')
        }
      } catch {
        setError('Lỗi kết nối.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [id])

  async function handleDownload() {
    if (!invoice) return
    setDownloading(true)
    try {
      await downloadFile(
        `/api/invoices/${id}/export/pdf`,
        `hoa-don-${invoice.invoiceNumber}.pdf`
      )
    } catch {
      showError('Xuất PDF thất bại')
    } finally {
      setDownloading(false)
      setShowDownloadConfirm(false)
    }
  }

  if (loading) return <LoadingSpinner />
  if (error || !invoice) {
    return (
      <div className="space-y-4">
        <Link href="/invoices" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Quay lại
        </Link>
        <Card>
          <p className="text-center text-gray-500 py-8">{error ?? 'Không tìm thấy hoá đơn'}</p>
        </Card>
      </div>
    )
  }

  const grandTotal = invoice.grandTotal ?? 0
  const subTotal = invoice.subTotal ?? 0
  const discountAmount = invoice.discountAmount ?? 0
  const vatAmount = Math.round(grandTotal * VAT_RATE)
  const pitAmount = Math.round(grandTotal * PIT_RATE)
  const pdfPath = `/api/invoices/${id}/export/pdf`

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <Link href="/invoices" className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
            <ArrowLeft size={18} />
          </Link>
          <div>
            <h1 className="text-xl font-bold text-gray-800">{invoice.invoiceNumber}</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              {formatDate(invoice.invoiceDate)} · Đơn hàng:{' '}
              <Link href={`/orders/${invoice.salesOrderId}`} className="text-blue-600 hover:underline">
                {invoice.orderNumber}
              </Link>
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button size="sm" variant="secondary" onClick={() => setShowPdf(true)} className="gap-1.5">
            <Eye size={15} /> Xem PDF
          </Button>
          <Button size="sm" onClick={() => setShowDownloadConfirm(true)} className="gap-1.5">
            <Download size={15} /> Tải PDF
          </Button>
        </div>
      </div>

      {/* Store info */}
      <Card className="flex items-start gap-3">
        <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center shrink-0">
          <Store size={20} className="text-blue-600" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="font-medium text-gray-800">{invoice.storeName}</p>
          <p className="text-sm text-gray-500 mt-0.5">{invoice.storeAddress}</p>
          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 mt-1.5 text-xs text-gray-400">
            {invoice.storePhone && (
              <span className="flex items-center gap-1"><Phone size={12} /> {invoice.storePhone}</span>
            )}
            {invoice.storeTaxCode && (
              <span>MST: {invoice.storeTaxCode}</span>
            )}
          </div>
        </div>
      </Card>

      {/* Summary cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <p className="text-xs text-gray-500">Tiền hàng</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{formatCurrency(subTotal)}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Giảm giá</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{formatCurrency(discountAmount)}</p>
        </Card>

        {/* Tax breakdown card */}
        <Card>
          <p className="text-xs text-gray-500 mb-2">Thuế (tính trên tổng thanh toán)</p>
          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-500">VAT 1%</span>
              <span className="text-sm font-semibold text-amber-600">{formatCurrency(vatAmount)}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-500">PIT 0.5%</span>
              <span className="text-sm font-semibold text-amber-600">{formatCurrency(pitAmount)}</span>
            </div>
            <div className="border-t border-gray-100 pt-1.5 flex items-center justify-between">
              <span className="text-xs font-medium text-gray-600">Tổng thuế</span>
              <span className="text-sm font-bold text-gray-800">{formatCurrency(vatAmount + pitAmount)}</span>
            </div>
          </div>
        </Card>

        <Card>
          <p className="text-xs text-gray-500">Tổng thanh toán</p>
          <p className="text-lg font-bold text-blue-600 mt-1">{formatCurrency(grandTotal)}</p>
          <p className="text-xs text-gray-400 mt-1.5">
            Đã trả: {formatCurrency(invoice.amountPaid ?? 0)}
            {(invoice.changeAmount ?? 0) > 0 && ` · Thối: ${formatCurrency(invoice.changeAmount)}`}
          </p>
          <p className="text-xs text-gray-400 mt-0.5">
            {PAYMENT_LABELS[invoice.paymentMethod] ?? invoice.paymentMethod}
          </p>
        </Card>
      </div>

      {/* Customer info (if present) */}
      {(invoice.customerName || invoice.customerPhone) && (
        <Card>
          <p className="text-xs text-gray-500 mb-1">Khách hàng</p>
          <p className="text-sm text-gray-800">
            {invoice.customerName ?? 'Khách lẻ'}
            {invoice.customerPhone && <span className="text-gray-400 ml-2">· {invoice.customerPhone}</span>}
          </p>
        </Card>
      )}

      {/* Items table */}
      <Card className="p-0 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-gray-100">
          <h3 className="text-sm font-semibold text-gray-700">Chi tiết hoá đơn ({invoice.items.length} mục)</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/40">
                <th className="text-left font-medium text-gray-500 px-5 py-2.5">#</th>
                <th className="text-left font-medium text-gray-500 px-5 py-2.5">Sản phẩm</th>
                <th className="text-left font-medium text-gray-500 px-5 py-2.5">SKU</th>
                <th className="text-left font-medium text-gray-500 px-5 py-2.5">ĐVT</th>
                <th className="text-right font-medium text-gray-500 px-5 py-2.5">Đơn giá</th>
                <th className="text-right font-medium text-gray-500 px-5 py-2.5">SL</th>
                <th className="text-right font-medium text-gray-500 px-5 py-2.5">Thành tiền</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {invoice.items.map((item, idx) => (
                <tr key={item.id}>
                  <td className="px-5 py-3 text-gray-400">{idx + 1}</td>
                  <td className="px-5 py-3 text-gray-800">
                    {item.productName}
                    {item.variantName && (
                      <span className="text-gray-400 ml-1">({item.variantName})</span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-gray-400 text-xs font-mono">{item.sku}</td>
                  <td className="px-5 py-3 text-gray-500">{item.unit}</td>
                  <td className="px-5 py-3 text-right text-gray-600">{formatCurrency(item.unitPrice)}</td>
                  <td className="px-5 py-3 text-right text-gray-600">{item.quantity}</td>
                  <td className="px-5 py-3 text-right font-medium text-gray-800">{formatCurrency(item.lineTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Bottom summary */}
        <div className="px-5 py-3.5 border-t border-gray-100 bg-gray-50/30 flex justify-end">
          <div className="space-y-1.5 text-sm w-72">
            <div className="flex justify-between text-gray-500">
              <span>Tiền hàng</span>
              <span>{formatCurrency(subTotal)}</span>
            </div>
            {discountAmount > 0 && (
              <div className="flex justify-between text-gray-500">
                <span>Giảm giá</span>
                <span>-{formatCurrency(discountAmount)}</span>
              </div>
            )}
            <div className="flex justify-between text-gray-500">
              <span>VAT 1%</span>
              <span>{formatCurrency(vatAmount)}</span>
            </div>
            <div className="flex justify-between text-gray-500">
              <span>PIT 0.5%</span>
              <span>{formatCurrency(pitAmount)}</span>
            </div>
            <div className="flex justify-between font-semibold text-gray-800 pt-1.5 border-t border-gray-200">
              <span>Tổng thanh toán</span>
              <span>{formatCurrency(grandTotal)}</span>
            </div>
          </div>
        </div>
      </Card>

      {/* PDF Viewer Modal */}
      <PdfViewerModal
        open={showPdf}
        title={`Hoá đơn ${invoice.invoiceNumber}`}
        pdfUrl={pdfPath}
        downloadFilename={`hoa-don-${invoice.invoiceNumber}.pdf`}
        onClose={() => setShowPdf(false)}
      />

      {/* Download Confirm */}
      <ConfirmDialog
        open={showDownloadConfirm}
        title="Tải hoá đơn PDF?"
        description={`Tải hoá đơn ${invoice.invoiceNumber} dạng PDF?`}
        icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Download size={24} className="text-blue-600" /></div>}
        confirmLabel="Tải xuống"
        loading={downloading}
        onConfirm={handleDownload}
        onCancel={() => setShowDownloadConfirm(false)}
      />
    </div>
  )
}
