'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import Link from 'next/link'
import { api } from '@/lib/api'
import { SalesOrderDetailDto } from '@/types'
import { formatCurrency, formatDate } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { StatusBadge } from '@/components/ui/StatusBadge'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { ArrowLeft, FileText, Receipt } from 'lucide-react'

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const router = useRouter()
  const { success, error: showError } = useToast()

  const [order, setOrder] = useState<SalesOrderDetailDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [showInvoiceConfirm, setShowInvoiceConfirm] = useState(false)
  const [generatingInvoice, setGeneratingInvoice] = useState(false)

  useEffect(() => {
    async function load() {
      setLoading(true)
      setError(null)
      try {
        const res = await api.get(`/api/salesorders/${id}`)
        if (res.ok) {
          const json = await res.json()
          setOrder(json.data ?? json)
        } else {
          setError('Không thể tải chi tiết đơn hàng.')
        }
      } catch {
        setError('Lỗi kết nối.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [id])

  async function handleGenerateInvoice() {
    setGeneratingInvoice(true)
    try {
      const res = await api.post(`/api/salesorders/${id}/invoice`)
      if (res.ok) {
        const json = await res.json()
        const invoiceId = json.data?.id ?? json.id
        success('Tạo hoá đơn thành công')
        if (invoiceId) {
          router.push(`/invoices/${invoiceId}`)
        } else {
          const refreshRes = await api.get(`/api/salesorders/${id}`)
          if (refreshRes.ok) {
            const refreshJson = await refreshRes.json()
            setOrder(refreshJson.data ?? refreshJson)
          }
        }
      } else {
        showError('Không thể tạo hoá đơn', 'Vui lòng thử lại.')
      }
    } catch {
      showError('Lỗi kết nối')
    } finally {
      setGeneratingInvoice(false)
      setShowInvoiceConfirm(false)
    }
  }

  if (loading) return <LoadingSpinner />
  if (error || !order) {
    return (
      <div className="space-y-4">
        <Link href="/orders" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Quay lại
        </Link>
        <Card>
          <p className="text-center text-gray-500 py-8">{error ?? 'Không tìm thấy đơn hàng'}</p>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <Link href="/orders" className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
            <ArrowLeft size={18} />
          </Link>
          <div>
            <h1 className="text-xl font-bold text-gray-800">{order.orderNumber}</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              {formatDate(order.createdAt)}
              {order.completedAt && ` · Hoàn thành ${formatDate(order.completedAt)}`}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <StatusBadge status={order.status} />
          {!order.hasInvoice && order.status === 'Completed' && (
            <Button size="sm" onClick={() => setShowInvoiceConfirm(true)} className="gap-1.5">
              <Receipt size={15} /> Tạo hoá đơn
            </Button>
          )}
          {order.invoiceId && (
            <Link href={`/invoices/${order.invoiceId}`}>
              <Button size="sm" variant="secondary" className="gap-1.5">
                <FileText size={15} /> Xem hoá đơn
              </Button>
            </Link>
          )}
        </div>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card>
          <p className="text-xs text-gray-500">Tổng tiền hàng</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{formatCurrency(order.subTotal ?? 0)}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Giảm giá</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{formatCurrency(order.discountAmount ?? 0)}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Thuế</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{formatCurrency(order.taxAmount ?? 0)}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Thành tiền</p>
          <p className="text-lg font-bold text-blue-600 mt-1">{formatCurrency(order.totalAmount ?? 0)}</p>
        </Card>
      </div>

      {/* Items */}
      <Card className="p-0 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-gray-100">
          <h3 className="text-sm font-semibold text-gray-700">Danh sách sản phẩm ({order.items.length})</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/40">
                <th className="text-left font-medium text-gray-500 px-5 py-2.5">#</th>
                <th className="text-left font-medium text-gray-500 px-5 py-2.5">Sản phẩm</th>
                <th className="text-right font-medium text-gray-500 px-5 py-2.5">Đơn giá</th>
                <th className="text-right font-medium text-gray-500 px-5 py-2.5">SL</th>
                <th className="text-right font-medium text-gray-500 px-5 py-2.5">Thành tiền</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {order.items.map((item, idx) => (
                <tr key={idx}>
                  <td className="px-5 py-3 text-gray-400">{idx + 1}</td>
                  <td className="px-5 py-3 text-gray-800">
                    {item.productName}
                    {item.variantName && (
                      <span className="text-gray-400 ml-1">({item.variantName})</span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-right text-gray-600">{formatCurrency(item.unitPrice)}</td>
                  <td className="px-5 py-3 text-right text-gray-600">{item.quantity}</td>
                  <td className="px-5 py-3 text-right font-medium text-gray-800">{formatCurrency(item.lineTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="px-5 py-3 border-t border-gray-100 bg-gray-50/30 flex justify-end">
          <div className="space-y-1 text-sm w-64">
            <div className="flex justify-between text-gray-500">
              <span>Tổng tiền hàng</span><span>{formatCurrency(order.subTotal ?? 0)}</span>
            </div>
            {(order.discountAmount ?? 0) > 0 && (
              <div className="flex justify-between text-gray-500">
                <span>Giảm giá</span><span>-{formatCurrency(order.discountAmount)}</span>
              </div>
            )}
            {(order.taxAmount ?? 0) > 0 && (
              <div className="flex justify-between text-gray-500">
                <span>Thuế</span><span>{formatCurrency(order.taxAmount)}</span>
              </div>
            )}
            <div className="flex justify-between font-semibold text-gray-800 pt-1 border-t border-gray-200">
              <span>Thành tiền</span><span>{formatCurrency(order.totalAmount ?? 0)}</span>
            </div>
          </div>
        </div>
      </Card>

      <ConfirmDialog
        open={showInvoiceConfirm}
        title="Tạo hoá đơn?"
        description={`Tạo hoá đơn cho đơn hàng ${order.orderNumber}?`}
        icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Receipt size={24} className="text-blue-600" /></div>}
        confirmLabel="Tạo hoá đơn"
        loading={generatingInvoice}
        onConfirm={handleGenerateInvoice}
        onCancel={() => setShowInvoiceConfirm(false)}
      />
    </div>
  )
}
