'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { productApi } from '@/lib/api'
import type { ProductDto, ProductBatchDtoV2 } from '@/types'
import { formatDate } from '@/lib/utils'
import { cn } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Button } from '@/components/ui/Button'
import { AlertTriangle, Clock, XCircle, Eye, ShieldAlert, CalendarX } from 'lucide-react'

const MAIN_TABS = [
  { id: 'low-stock', label: 'Sắp hết hàng', icon: AlertTriangle, color: 'text-amber-600', bg: 'bg-amber-50' },
  { id: 'expiry', label: 'Sắp hết hạn / Đã hết hạn', icon: Clock, color: 'text-red-600', bg: 'bg-red-50' },
] as const
type MainTabId = typeof MAIN_TABS[number]['id']

const SUB_TABS = [
  { id: 'expired', label: 'Đã hết hạn', icon: XCircle, color: 'text-red-600' },
  { id: 'expiring-soon', label: 'Sắp hết hạn', icon: Clock, color: 'text-orange-600' },
] as const
type SubTabId = typeof SUB_TABS[number]['id']

export default function AlertsPage() {
  const [mainTab, setMainTab] = useState<MainTabId>('low-stock')
  const [subTab, setSubTab] = useState<SubTabId>('expired')

  const [lowStock, setLowStock] = useState<ProductDto[]>([])
  const [lowStockTotal, setLowStockTotal] = useState(0)
  const [lowStockLoading, setLowStockLoading] = useState(true)

  const [expired, setExpired] = useState<(ProductBatchDtoV2 & { productName: string })[]>([])
  const [expiredTotal, setExpiredTotal] = useState(0)
  const [expiredLoading, setExpiredLoading] = useState(true)

  const [expiringSoon, setExpiringSoon] = useState<(ProductBatchDtoV2 & { productName: string })[]>([])
  const [expiringSoonTotal, setExpiringSoonTotal] = useState(0)
  const [expiringSoonLoading, setExpiringSoonLoading] = useState(true)

  const loadLowStock = useCallback(async () => {
    setLowStockLoading(true)
    try {
      const data = await productApi.lowStock()
      setLowStock(data)
      setLowStockTotal(data.length)
    } catch { /* ignore */ }
    finally { setLowStockLoading(false) }
  }, [])

  const loadExpired = useCallback(async () => {
    setExpiredLoading(true)
    try {
      const data = await productApi.expiredBatches()
      const batches = data.flatMap(p =>
        (p.recentBatches ?? [])
          .filter(b => b.isExpired)
          .map(b => ({ ...b, productName: p.name, productId: p.id }))
      )
      setExpired(batches)
      setExpiredTotal(batches.length)
    } catch { /* ignore */ }
    finally { setExpiredLoading(false) }
  }, [])

  const loadExpiringSoon = useCallback(async () => {
    setExpiringSoonLoading(true)
    try {
      const data = await productApi.expiringSoon(30)
      const batches = data.flatMap(p =>
        (p.recentBatches ?? [])
          .filter(b => b.isExpiringSoon && !b.isExpired)
          .map(b => ({ ...b, productName: p.name, productId: p.id }))
      )
      setExpiringSoon(batches)
      setExpiringSoonTotal(batches.length)
    } catch { /* ignore */ }
    finally { setExpiringSoonLoading(false) }
  }, [])

  useEffect(() => {
    Promise.all([loadLowStock(), loadExpired(), loadExpiringSoon()])
  }, [loadLowStock, loadExpired, loadExpiringSoon])

  const summaryLoading = lowStockLoading && expiredLoading && expiringSoonLoading

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-bold text-gray-800">Cảnh báo kho</h1>

      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-4">
        <div className={cn('text-left rounded-lg ring-2 transition-all', mainTab === 'low-stock' ? 'ring-blue-400' : 'ring-transparent')}>
          <button onClick={() => setMainTab('low-stock')} className="w-full">
            <Card className="flex items-center gap-3 hover:shadow-md transition-shadow">
              <div className="w-10 h-10 bg-amber-50 rounded-lg flex items-center justify-center shrink-0">
                <ShieldAlert size={20} className="text-amber-600" />
              </div>
              <div className="text-left">
                <p className="text-2xl font-bold text-gray-800">{summaryLoading ? '—' : lowStockTotal}</p>
                <p className="text-xs text-gray-500 mt-0.5">Sắp hết hàng</p>
              </div>
            </Card>
          </button>
        </div>
        <div className={cn('rounded-lg ring-2 transition-all', mainTab === 'expiry' && subTab === 'expired' ? 'ring-blue-400' : 'ring-transparent')}>
          <button onClick={() => { setMainTab('expiry'); setSubTab('expired') }} className="w-full">
            <Card className="flex items-center gap-3 hover:shadow-md transition-shadow">
              <div className="w-10 h-10 bg-red-50 rounded-lg flex items-center justify-center shrink-0">
                <CalendarX size={20} className="text-red-600" />
              </div>
              <div className="text-left">
                <p className="text-2xl font-bold text-gray-800">{summaryLoading ? '—' : expiredTotal}</p>
                <p className="text-xs text-gray-500 mt-0.5">Lô đã hết hạn</p>
              </div>
            </Card>
          </button>
        </div>
        <div className={cn('rounded-lg ring-2 transition-all', mainTab === 'expiry' && subTab === 'expiring-soon' ? 'ring-blue-400' : 'ring-transparent')}>
          <button onClick={() => { setMainTab('expiry'); setSubTab('expiring-soon') }} className="w-full">
            <Card className="flex items-center gap-3 hover:shadow-md transition-shadow">
              <div className="w-10 h-10 bg-orange-50 rounded-lg flex items-center justify-center shrink-0">
                <Clock size={20} className="text-orange-600" />
              </div>
              <div className="text-left">
                <p className="text-2xl font-bold text-gray-800">{summaryLoading ? '—' : expiringSoonTotal}</p>
                <p className="text-xs text-gray-500 mt-0.5">Sắp hết hạn</p>
              </div>
            </Card>
          </button>
        </div>
      </div>

      {/* Main tabs */}
      <div className="flex bg-gray-100 rounded-md p-1 gap-1 w-fit">
        {MAIN_TABS.map(t => (
          <button key={t.id} onClick={() => setMainTab(t.id)}
            className={cn('flex items-center gap-1.5 px-4 py-2 text-sm font-medium rounded-md transition-all',
              mainTab === t.id ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500 hover:text-gray-700')}>
            <t.icon size={15} /> {t.label}
          </button>
        ))}
      </div>

      {/* Tab content */}
      {mainTab === 'low-stock' && (
        <>
          {lowStockLoading ? <LoadingSpinner /> : (
            <>
              {lowStock.length === 0 ? (
                <Card><p className="text-center text-gray-400 py-8 text-sm">Không có sản phẩm sắp hết hàng</p></Card>
              ) : (
                <Card className="p-0 overflow-hidden">
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-gray-200 bg-gray-50/60">
                          <th className="text-left font-medium text-gray-500 px-4 py-3">Sản phẩm</th>
                          <th className="text-left font-medium text-gray-500 px-4 py-3">SKU</th>
                          <th className="text-left font-medium text-gray-500 px-4 py-3">Danh mục</th>
                          <th className="text-right font-medium text-gray-500 px-4 py-3">Tồn kho</th>
                          <th className="text-right font-medium text-gray-500 px-4 py-3">Ngưỡng</th>
                          <th className="text-left font-medium text-gray-500 px-4 py-3 w-48">Mức tồn</th>
                          <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-100">
                        {lowStock.map(p => {
                          const pct = p.lowStockThreshold > 0
                            ? Math.min(100, Math.round((p.availableStock / p.lowStockThreshold) * 100))
                            : 0
                          const barColor = pct === 0 ? 'bg-red-500' : pct < 50 ? 'bg-orange-400' : 'bg-amber-400'
                          return (
                            <tr key={p.id} className="hover:bg-gray-50/50">
                              <td className="px-4 py-3 font-medium text-gray-800">{p.name}</td>
                              <td className="px-4 py-3 text-gray-500 font-mono text-xs">{p.sku}</td>
                              <td className="px-4 py-3 text-gray-500">{p.categoryName ?? '—'}</td>
                              <td className="px-4 py-3 text-right font-semibold text-red-600">{p.availableStock}</td>
                              <td className="px-4 py-3 text-right text-gray-500">{p.lowStockThreshold}</td>
                              <td className="px-4 py-3">
                                <div className="flex items-center gap-2">
                                  <div className="flex-1 h-2 bg-gray-100 rounded-full overflow-hidden">
                                    <div className={cn('h-full rounded-full transition-all', barColor)} style={{ width: `${pct}%` }} />
                                  </div>
                                  <span className="text-xs text-gray-400 w-8 text-right">{pct}%</span>
                                </div>
                              </td>
                              <td className="px-4 py-3 text-right">
                                <Link href={`/inventory/products/${p.id}`}>
                                  <Button size="sm" variant="ghost"><Eye size={14} /></Button>
                                </Link>
                              </td>
                            </tr>
                          )
                        })}
                      </tbody>
                    </table>
                  </div>
                </Card>
              )}
            </>
          )}
        </>
      )}

      {mainTab === 'expiry' && (
        <>
          {/* Sub tabs */}
          <div className="flex gap-1 border-b border-gray-200">
            {SUB_TABS.map(t => (
              <button key={t.id} onClick={() => setSubTab(t.id)}
                className={cn('flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 transition-all -mb-px',
                  subTab === t.id
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300')}>
                <t.icon size={14} className={subTab === t.id ? 'text-blue-600' : t.color} />
                {t.label}
                <span className={cn('ml-1 text-xs px-1.5 py-0.5 rounded-full font-medium',
                  t.id === 'expired' ? 'bg-red-100 text-red-700' : 'bg-orange-100 text-orange-700')}>
                  {t.id === 'expired' ? expiredTotal : expiringSoonTotal}
                </span>
              </button>
            ))}
          </div>

          {subTab === 'expired' && (
            <>
              {expiredLoading ? <LoadingSpinner /> : (
                <>
                  {expired.length === 0 ? (
                    <Card><p className="text-center text-gray-400 py-8 text-sm">Không có lô hàng đã hết hạn</p></Card>
                  ) : (
                    <Card className="p-0 overflow-hidden">
                      <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                          <thead>
                            <tr className="border-b border-gray-200 bg-gray-50/60">
                              <th className="text-left font-medium text-gray-500 px-4 py-3">Sản phẩm</th>
                              <th className="text-left font-medium text-gray-500 px-4 py-3">Số lô</th>
                              <th className="text-right font-medium text-gray-500 px-4 py-3">SL</th>
                              <th className="text-left font-medium text-gray-500 px-4 py-3">Hạn sử dụng</th>
                              <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-100">
                            {expired.map((b, i) => (
                              <tr key={`${b.productId}-${b.batchNumber}-${i}`} className="hover:bg-gray-50/50">
                                <td className="px-4 py-3 font-medium text-gray-800">{b.productName}</td>
                                <td className="px-4 py-3 text-gray-500">{b.batchNumber}</td>
                                <td className="px-4 py-3 text-right text-gray-700">{b.quantity}</td>
                                <td className="px-4 py-3">
                                  <span className="inline-flex items-center gap-1.5 text-red-600 font-medium">
                                    <XCircle size={13} />
                                    {b.expirationDate ? formatDate(b.expirationDate) : '—'}
                                  </span>
                                </td>
                                <td className="px-4 py-3 text-right">
                                  <Link href={`/inventory/products/${b.productId}`}>
                                    <Button size="sm" variant="ghost"><Eye size={14} /></Button>
                                  </Link>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </Card>
                  )}
                </>
              )}
            </>
          )}

          {subTab === 'expiring-soon' && (
            <>
              {expiringSoonLoading ? <LoadingSpinner /> : (
                <>
                  {expiringSoon.length === 0 ? (
                    <Card><p className="text-center text-gray-400 py-8 text-sm">Không có lô hàng sắp hết hạn</p></Card>
                  ) : (
                    <Card className="p-0 overflow-hidden">
                      <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                          <thead>
                            <tr className="border-b border-gray-200 bg-gray-50/60">
                              <th className="text-left font-medium text-gray-500 px-4 py-3">Sản phẩm</th>
                              <th className="text-left font-medium text-gray-500 px-4 py-3">Số lô</th>
                              <th className="text-right font-medium text-gray-500 px-4 py-3">SL</th>
                              <th className="text-left font-medium text-gray-500 px-4 py-3">Hạn sử dụng</th>
                              <th className="text-right font-medium text-gray-500 px-4 py-3">Còn lại</th>
                              <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-100">
                            {expiringSoon.map((b, i) => (
                              <tr key={`${b.productId}-${b.batchNumber}-${i}`} className="hover:bg-gray-50/50">
                                <td className="px-4 py-3 font-medium text-gray-800">{b.productName}</td>
                                <td className="px-4 py-3 text-gray-500">{b.batchNumber}</td>
                                <td className="px-4 py-3 text-right text-gray-700">{b.quantity}</td>
                                <td className="px-4 py-3 text-orange-600 font-medium">
                                  {b.expirationDate ? formatDate(b.expirationDate) : '—'}
                                </td>
                                <td className="px-4 py-3 text-right">
                                  {b.daysUntilExpiration != null ? (
                                    <span className={cn('inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-md',
                                      b.daysUntilExpiration <= 7 ? 'bg-red-50 text-red-700' : 'bg-orange-50 text-orange-700')}>
                                      {b.daysUntilExpiration} ngày
                                    </span>
                                  ) : '—'}
                                </td>
                                <td className="px-4 py-3 text-right">
                                  <Link href={`/inventory/products/${b.productId}`}>
                                    <Button size="sm" variant="ghost"><Eye size={14} /></Button>
                                  </Link>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </Card>
                  )}
                </>
              )}
            </>
          )}
        </>
      )}
    </div>
  )
}
