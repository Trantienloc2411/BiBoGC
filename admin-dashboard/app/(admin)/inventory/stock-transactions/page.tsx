'use client'

import { useEffect, useState, useCallback } from 'react'
import { stockTransactionApi, productApi } from '@/lib/api'
import type { StockTransactionDtoV2, AdjustStockRequestV2, ProductDto } from '@/types'
import { formatCurrency, formatDateTime } from '@/lib/utils'
import { cn } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Pagination } from '@/components/ui/Pagination'
import { FormDialog, FormField, FormError, inputClass, selectClass } from '@/components/ui/FormDialog'
import { useToast } from '@/components/ui/Toast'
import { Plus, X, ArrowDownToLine, ArrowUpFromLine, RefreshCw } from 'lucide-react'

const PAGE_SIZE = 20

const IN_TYPES = new Set(['Purchase', 'AdjustmentIn', 'Return'])

const TYPE_META: Record<string, { label: string; colorClass: string }> = {
  Purchase:       { label: 'Nhập hàng',       colorClass: 'bg-blue-50 text-blue-700 border-blue-200' },
  Sale:           { label: 'Bán hàng',         colorClass: 'bg-emerald-50 text-emerald-700 border-emerald-200' },
  AdjustmentIn:   { label: 'Điều chỉnh tăng',  colorClass: 'bg-teal-50 text-teal-700 border-teal-200' },
  AdjustmentOut:  { label: 'Điều chỉnh giảm',  colorClass: 'bg-orange-50 text-orange-700 border-orange-200' },
  Damage:         { label: 'Hư hỏng',          colorClass: 'bg-red-50 text-red-700 border-red-200' },
  Expiry:         { label: 'Hết hạn',          colorClass: 'bg-red-50 text-red-700 border-red-200' },
  Return:         { label: 'Trả hàng',         colorClass: 'bg-purple-50 text-purple-700 border-purple-200' },
  SupplierReturn: { label: 'Trả NCC',          colorClass: 'bg-indigo-50 text-indigo-700 border-indigo-200' },
}

const TYPE_OPTIONS = Object.entries(TYPE_META).map(([k, v]) => ({ value: k, label: v.label }))

export default function StockTransactionsPage() {
  const { success, error: showError } = useToast()

  const [txns, setTxns] = useState<StockTransactionDtoV2[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)

  const [filterType, setFilterType] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [productSearch, setProductSearch] = useState('')

  const [showAdjust, setShowAdjust] = useState(false)
  const [products, setProducts] = useState<ProductDto[]>([])
  const [adjustLoading, setAdjustLoading] = useState(false)
  const [adjustError, setAdjustError] = useState('')
  const [adjustForm, setAdjustForm] = useState<AdjustStockRequestV2>({ productId: '', quantity: 0, reason: '', unitPrice: 0 })

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const params: Parameters<typeof stockTransactionApi.list>[0] = {
        pageNumber: p, pageSize: PAGE_SIZE,
        transactionType: filterType || undefined,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
        productId: productSearch || undefined,
      }
      const data = await stockTransactionApi.list(params)
      setTxns(data.items)
      setTotalCount(data.totalCount)
      setTotalPages(data.totalPages)
    } catch {
      showError('Không thể tải giao dịch kho')
    } finally { setLoading(false) }
  }, [filterType, dateFrom, dateTo, productSearch, showError])

  useEffect(() => { setPage(1); load(1) }, [filterType, dateFrom, dateTo, productSearch]) // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => { load(page) }, [page, load])

  async function openAdjust() {
    setAdjustForm({ productId: '', quantity: 0, reason: '', unitPrice: 0 })
    setAdjustError('')
    if (products.length === 0) {
      try {
        const data = await productApi.list({ pageSize: 200 })
        setProducts(data.items)
      } catch { /* ignore */ }
    }
    setShowAdjust(true)
  }

  async function handleAdjust(e: React.FormEvent) {
    e.preventDefault(); setAdjustError('')
    if (!adjustForm.productId) { setAdjustError('Vui lòng chọn sản phẩm.'); return }
    if (adjustForm.quantity === 0) { setAdjustError('Số lượng phải khác 0.'); return }
    if (!adjustForm.reason.trim()) { setAdjustError('Vui lòng nhập lý do.'); return }
    setAdjustLoading(true)
    try {
      await stockTransactionApi.adjust({ ...adjustForm, reason: adjustForm.reason.trim() })
      success('Điều chỉnh tồn kho thành công')
      setShowAdjust(false)
      load(page)
    } catch (err) {
      setAdjustError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setAdjustLoading(false) }
  }

  const hasFilters = filterType || dateFrom || dateTo || productSearch

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Giao dịch kho</h1>
        <Button size="sm" onClick={openAdjust} className="gap-1.5"><Plus size={15} /> Điều chỉnh kho</Button>
      </div>

      <Card className="flex flex-wrap gap-3 items-end">
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Loại giao dịch</label>
          <select value={filterType} onChange={e => setFilterType(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400">
            <option value="">Tất cả</option>
            {TYPE_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Từ ngày</label>
          <input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400" />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Đến ngày</label>
          <input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400" />
        </div>
        {hasFilters && (
          <button onClick={() => { setFilterType(''); setDateFrom(''); setDateTo(''); setProductSearch('') }}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 pb-2">
            <X size={14} /> Xoá lọc
          </button>
        )}
      </Card>

      <p className="text-sm text-gray-400 px-1">{totalCount.toLocaleString()} giao dịch</p>

      {loading ? <LoadingSpinner /> : (
        <>
          {txns.length === 0 ? (
            <Card><p className="text-center text-gray-400 py-8 text-sm">Không có giao dịch nào</p></Card>
          ) : (
            <Card className="p-0 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50/60">
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Sản phẩm</th>
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Lô</th>
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Loại</th>
                      <th className="text-right font-medium text-gray-500 px-4 py-3">SL</th>
                      <th className="text-right font-medium text-gray-500 px-4 py-3">Đơn giá</th>
                      <th className="text-right font-medium text-gray-500 px-4 py-3">Giá trị</th>
                      <th className="text-left font-medium text-gray-500 px-4 py-3">Ngày</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {txns.map(t => {
                      const meta = TYPE_META[t.transactionType]
                      const isIn = IN_TYPES.has(t.transactionType)
                      return (
                        <tr key={t.id} className="hover:bg-gray-50/50">
                          <td className="px-4 py-3">
                            <p className="font-medium text-gray-800">{t.productName}</p>
                            <p className="text-xs text-gray-400 mt-0.5 font-mono">{t.productSku}</p>
                            {t.notes && <p className="text-xs text-gray-400 mt-0.5 truncate max-w-[200px]">{t.notes}</p>}
                          </td>
                          <td className="px-4 py-3 text-gray-500 text-xs">{t.batchNumber ?? '—'}</td>
                          <td className="px-4 py-3">
                            <span className={cn('inline-flex items-center gap-1.5 px-2 py-0.5 text-xs font-medium rounded-md border', meta?.colorClass ?? 'bg-gray-50 text-gray-600 border-gray-200')}>
                              {isIn ? <ArrowDownToLine size={11} /> : <ArrowUpFromLine size={11} />}
                              {meta?.label ?? t.transactionType}
                            </span>
                          </td>
                          <td className={cn('px-4 py-3 text-right font-semibold', isIn ? 'text-emerald-600' : 'text-red-600')}>
                            {isIn ? '+' : '−'}{Math.abs(t.quantity)}
                          </td>
                          <td className="px-4 py-3 text-right text-gray-600">{formatCurrency(t.unitPrice)}</td>
                          <td className="px-4 py-3 text-right text-gray-700 font-medium">{formatCurrency(t.totalValue)}</td>
                          <td className="px-4 py-3 text-gray-500 text-xs whitespace-nowrap">{formatDateTime(t.transactionDate)}</td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </Card>
          )}
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}

      <FormDialog open={showAdjust} title="Điều chỉnh tồn kho" submitLabel="Xác nhận" loading={adjustLoading} onSubmit={handleAdjust} onCancel={() => setShowAdjust(false)}>
        <FormField label="Sản phẩm" required>
          <select className={selectClass} value={adjustForm.productId} onChange={e => setAdjustForm(f => ({ ...f, productId: e.target.value }))}>
            <option value="">— Chọn sản phẩm —</option>
            {products.map(p => <option key={p.id} value={p.id}>{p.name} ({p.sku})</option>)}
          </select>
        </FormField>
        <div className="flex items-center gap-2 p-3 bg-blue-50 rounded-md text-xs text-blue-700">
          <RefreshCw size={13} />
          <span>Nhập số dương (+) để tăng, số âm (−) để giảm tồn kho</span>
        </div>
        <FormField label="Số lượng điều chỉnh" required>
          <input type="number" className={inputClass} value={adjustForm.quantity || ''}
            onChange={e => setAdjustForm(f => ({ ...f, quantity: Number(e.target.value) }))}
            placeholder="+10 hoặc -5" />
        </FormField>
        <FormField label="Đơn giá">
          <input type="number" className={inputClass} value={adjustForm.unitPrice || ''}
            onChange={e => setAdjustForm(f => ({ ...f, unitPrice: Number(e.target.value) }))} />
        </FormField>
        <FormField label="Lý do điều chỉnh" required>
          <textarea className={inputClass} rows={2} value={adjustForm.reason}
            onChange={e => setAdjustForm(f => ({ ...f, reason: e.target.value }))}
            placeholder="Kiểm kê định kỳ, hàng hư hỏng..." />
        </FormField>
        <FormError message={adjustError} />
      </FormDialog>
    </div>
  )
}
