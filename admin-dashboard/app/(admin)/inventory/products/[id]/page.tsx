'use client'

import { useEffect, useState, useCallback } from 'react'
import { useParams } from 'next/navigation'
import Link from 'next/link'
import { productApi } from '@/lib/api'
import type {
  ProductDto, ProductBatchDtoV2, ProductVariantDtoV2,
  AddBatchRequest, UpdateBatchRequest,
  CreateVariantRequestV2, UpdateVariantRequest,
} from '@/types'
import { VariantUnitCode, VariantUnitLabel } from '@/types'
import { formatCurrency, formatDate } from '@/lib/utils'
import { cn } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { FormDialog, FormField, FormError, inputClass } from '@/components/ui/FormDialog'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { ArrowLeft, Plus, Pencil, Trash2, AlertTriangle, Package, Layers } from 'lucide-react'

const STATUS_LABELS: Record<string, string> = {
  Active: 'Đang bán', Inactive: 'Ngừng bán',
  OutOfStock: 'Hết hàng', Discontinued: 'Ngừng kinh doanh',
}
const STATUS_STYLES: Record<string, string> = {
  Active: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  Inactive: 'bg-gray-100 text-gray-600 border-gray-200',
  OutOfStock: 'bg-orange-50 text-orange-700 border-orange-200',
  Discontinued: 'bg-red-50 text-red-600 border-red-200',
}

const TABS = [
  { id: 'batches', label: 'Lô hàng', icon: Package },
  { id: 'variants', label: 'Biến thể', icon: Layers },
] as const
type TabId = typeof TABS[number]['id']

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { success, error: showError } = useToast()

  const [product, setProduct] = useState<ProductDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState<string | null>(null)
  const [tab, setTab] = useState<TabId>('batches')

  const [batches, setBatches] = useState<ProductBatchDtoV2[]>([])
  const [variants, setVariants] = useState<ProductVariantDtoV2[]>([])

  const loadProduct = useCallback(async () => {
    setLoading(true); setPageError(null)
    try {
      const p = await productApi.getById(id)
      setProduct(p)
    } catch {
      setPageError('Không thể tải sản phẩm.')
    } finally { setLoading(false) }
  }, [id])

  const loadBatches = useCallback(async () => {
    try {
      const data = await productApi.getBatches(id, { pageSize: 100 })
      setBatches(data.items)
    } catch {
      setBatches([])
    }
  }, [id])

  const loadVariants = useCallback(async () => {
    try {
      const data = await productApi.getVariants(id)
      setVariants(Array.isArray(data) ? data : [])
    } catch {
      setVariants([])
    }
  }, [id])

  useEffect(() => {
    loadProduct()
    loadBatches()
    loadVariants()
  }, [loadProduct, loadBatches, loadVariants])

  if (loading) return <LoadingSpinner />
  if (pageError || !product) return (
    <div className="space-y-4">
      <Link href="/inventory/products" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700">
        <ArrowLeft size={16} /> Quay lại
      </Link>
      <Card><p className="text-center text-gray-500 py-8">{pageError ?? 'Không tìm thấy'}</p></Card>
    </div>
  )

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Link href="/inventory/products" className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
          <ArrowLeft size={18} />
        </Link>
        <div className="flex-1">
          <h1 className="text-xl font-bold text-gray-800">{product.name}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{product.sku}</p>
        </div>
        <span className={cn('inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-md border',
          STATUS_STYLES[product.status] ?? 'bg-gray-50 text-gray-600 border-gray-200')}>
          {STATUS_LABELS[product.status] ?? product.status}
        </span>
        {product.isLowStock && (
          <span className="inline-flex items-center gap-1.5 px-3 py-1 text-xs font-medium bg-amber-50 text-amber-700 rounded-md border border-amber-200">
            <AlertTriangle size={13} /> Sắp hết hàng
          </span>
        )}
      </div>

      <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
        <Card>
          <p className="text-xs text-gray-500">Giá bán</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{formatCurrency(product.price)}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Tổng tồn kho</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{product.totalStock}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Có thể bán</p>
          <p className={cn('text-lg font-bold mt-1', product.isLowStock ? 'text-red-600' : 'text-gray-800')}>{product.availableStock ?? 0}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Hết hạn</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{product.expiredStock}</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Sắp hết hạn</p>
          <p className="text-lg font-bold text-gray-800 mt-1">{product.expiringSoonStock}</p>
        </Card>
      </div>

      {(product.categoryName || product.supplierName || product.description) && (
        <Card className="space-y-2 text-sm">
          {product.categoryName && <p><span className="text-gray-500">Danh mục:</span> <span className="text-gray-800">{product.categoryName}</span></p>}
          {product.supplierName && <p><span className="text-gray-500">Nhà cung cấp:</span> <span className="text-gray-800">{product.supplierName as string}</span></p>}
          {product.description && <p className="text-gray-600">{product.description}</p>}
        </Card>
      )}

      <div className="flex bg-gray-100 rounded-md p-1 gap-1 w-fit">
        {TABS.map(t => (
          <button key={t.id} onClick={() => setTab(t.id)}
            className={cn('flex items-center gap-1.5 px-4 py-2 text-sm font-medium rounded-md transition-all',
              tab === t.id ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500 hover:text-gray-700')}>
            <t.icon size={15} /> {t.label}
          </button>
        ))}
      </div>

      {tab === 'batches' && (
        <BatchSection productId={id} batches={batches} onRefresh={loadBatches} />
      )}
      {tab === 'variants' && (
        <VariantSection productId={id} variants={variants} onRefresh={loadVariants} />
      )}
    </div>
  )
}

// ─── Batch Section ────────────────────────────────────────────────────────────

function batchStatusBadge(b: ProductBatchDtoV2) {
  if (b.isExpired) return <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-md bg-red-50 text-red-700 border border-red-200">Hết hạn</span>
  if (b.isExpiringSoon) return (
    <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-md bg-orange-50 text-orange-700 border border-orange-200">
      Sắp hết hạn ({b.daysUntilExpiration} ngày)
    </span>
  )
  return <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-md bg-emerald-50 text-emerald-700 border border-emerald-200">Còn hạn</span>
}

function BatchSection({ productId, batches, onRefresh }: { productId: string; batches: ProductBatchDtoV2[]; onRefresh: () => void }) {
  const { success, error: showError } = useToast()
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<ProductBatchDtoV2 | null>(null)
  const [formLoading, setFormLoading] = useState(false)
  const [formError, setFormError] = useState('')
  const [form, setForm] = useState<AddBatchRequest>({ batchNumber: '', quantity: 0 })
  const [deleteTarget, setDeleteTarget] = useState<ProductBatchDtoV2 | null>(null)
  const [deleting, setDeleting] = useState(false)

  function openCreate() {
    setEditing(null)
    setForm({ batchNumber: '', quantity: 0, manufacturingDate: undefined, expirationDate: undefined })
    setFormError('')
    setShowForm(true)
  }

  function openEdit(b: ProductBatchDtoV2) {
    setEditing(b)
    setForm({
      batchNumber: b.batchNumber, quantity: b.quantity,
      manufacturingDate: b.manufacturingDate?.slice(0, 10),
      expirationDate: b.expirationDate?.slice(0, 10),
    })
    setFormError('')
    setShowForm(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!form.batchNumber.trim()) { setFormError('Số lô không được trống.'); return }
    setFormLoading(true)
    try {
      if (editing) {
        const body: UpdateBatchRequest = {
          quantity: form.quantity,
          manufacturingDate: form.manufacturingDate || undefined,
          expirationDate: form.expirationDate || undefined,
        }
        await productApi.updateBatch(productId, editing.id, body)
        success('Cập nhật lô thành công')
      } else {
        await productApi.addBatch(productId, { ...form, batchNumber: form.batchNumber.trim() })
        success('Thêm lô thành công')
      }
      setShowForm(false)
      onRefresh()
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await productApi.deleteBatch(productId, deleteTarget.id)
      success('Đã xoá lô hàng')
      onRefresh()
    } catch (err) {
      showError('Xoá thất bại', err instanceof Error ? err.message : undefined)
    } finally { setDeleting(false); setDeleteTarget(null) }
  }

  return (
    <>
      <div className="flex items-center justify-between">
        <p className="text-sm text-gray-400">{batches.length} lô hàng</p>
        <Button size="sm" onClick={openCreate} className="gap-1.5"><Plus size={14} /> Thêm lô</Button>
      </div>
      {batches.length === 0 ? (
        <Card><p className="text-center text-gray-400 py-6 text-sm">Chưa có lô hàng nào</p></Card>
      ) : (
        <Card className="p-0 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50/60">
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Số lô</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">SL</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Sản xuất</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Hạn sử dụng</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Tình trạng</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {batches.map(b => (
                  <tr key={b.id} className="hover:bg-gray-50/50">
                    <td className="px-4 py-3 font-medium text-gray-800">{b.batchNumber}</td>
                    <td className="px-4 py-3 text-right text-gray-700">{b.quantity}</td>
                    <td className="px-4 py-3 text-gray-500">{b.manufacturingDate ? formatDate(b.manufacturingDate) : '—'}</td>
                    <td className="px-4 py-3 text-gray-500">{b.expirationDate ? formatDate(b.expirationDate) : '—'}</td>
                    <td className="px-4 py-3">{batchStatusBadge(b)}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-1">
                        <Button size="sm" variant="ghost" onClick={() => openEdit(b)}><Pencil size={14} /></Button>
                        <Button size="sm" variant="ghost" onClick={() => setDeleteTarget(b)} className="text-red-500"><Trash2 size={14} /></Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      <FormDialog open={showForm} title={editing ? 'Sửa lô hàng' : 'Thêm lô hàng'} loading={formLoading} onSubmit={handleSubmit} onCancel={() => setShowForm(false)}>
        <FormField label="Số lô" required>
          <input className={inputClass} value={form.batchNumber} onChange={e => setForm(f => ({ ...f, batchNumber: e.target.value }))} />
        </FormField>
        <FormField label="Số lượng">
          <input type="number" className={inputClass} value={form.quantity || ''} onChange={e => setForm(f => ({ ...f, quantity: Number(e.target.value) }))} />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Ngày sản xuất">
            <input type="date" className={inputClass} value={form.manufacturingDate ?? ''} onChange={e => setForm(f => ({ ...f, manufacturingDate: e.target.value || undefined }))} />
          </FormField>
          <FormField label="Hạn sử dụng">
            <input type="date" className={inputClass} value={form.expirationDate ?? ''} onChange={e => setForm(f => ({ ...f, expirationDate: e.target.value || undefined }))} />
          </FormField>
        </div>
        <FormError message={formError} />
      </FormDialog>

      <ConfirmDialog open={deleteTarget !== null} title="Xoá lô hàng?"
        description={`Xoá lô "${deleteTarget?.batchNumber}"?`}
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><Trash2 size={24} className="text-red-500" /></div>}
        confirmLabel="Xoá" variant="danger" loading={deleting} onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)} />
    </>
  )
}

// ─── Variant Section ──────────────────────────────────────────────────────────

function VariantSection({ productId, variants, onRefresh }: { productId: string; variants: ProductVariantDtoV2[]; onRefresh: () => void }) {
  const { success, error: showError } = useToast()
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<ProductVariantDtoV2 | null>(null)
  const [formLoading, setFormLoading] = useState(false)
  const [formError, setFormError] = useState('')
  const [form, setForm] = useState<Omit<CreateVariantRequestV2, 'productId'>>({ variantName: '', unit: VariantUnitCode.Lon, quantityBaseUnit: 1, salePrice: 0, costPrice: 0 })
  const [deleteTarget, setDeleteTarget] = useState<ProductVariantDtoV2 | null>(null)
  const [deleting, setDeleting] = useState(false)

  function openCreate() {
    setEditing(null)
    setForm({ variantName: '', unit: VariantUnitCode.Lon, quantityBaseUnit: 1, salePrice: 0, costPrice: 0, barcode: undefined, displayOrder: 0 })
    setFormError('')
    setShowForm(true)
  }

  function openEdit(v: ProductVariantDtoV2) {
    setEditing(v)
    setForm({ variantName: v.variantName, unit: v.unit, quantityBaseUnit: v.quantityBaseUnit, salePrice: v.salePrice, costPrice: v.costPrice ?? 0, barcode: v.barcode ?? undefined, displayOrder: v.displayOrder ?? 0 })
    setFormError('')
    setShowForm(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!form.variantName.trim()) { setFormError('Tên biến thể không được trống.'); return }
    setFormLoading(true)
    try {
      if (editing) {
        const body: UpdateVariantRequest = {
          productVariantId: editing.id,
          productId,
          variantName: form.variantName.trim(),
          salePrice: form.salePrice,
          costPrice: form.costPrice ?? 0,
          barcode: form.barcode || undefined,
          quantityBaseUnit: form.quantityBaseUnit,
          unit: form.unit,
          displayOrder: form.displayOrder ?? 0,
        }
        await productApi.updateVariant(productId, editing.id, body)
        success('Cập nhật biến thể thành công')
      } else {
        const body: CreateVariantRequestV2 = {
          productId,
          variantName: form.variantName.trim(),
          unit: form.unit,
          quantityBaseUnit: form.quantityBaseUnit,
          salePrice: form.salePrice,
          costPrice: form.costPrice ?? 0,
          barcode: form.barcode || undefined,
          displayOrder: form.displayOrder ?? 0,
        }
        await productApi.createVariant(productId, body)
        success('Thêm biến thể thành công')
      }
      setShowForm(false)
      onRefresh()
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await productApi.deleteVariant(productId, deleteTarget.id)
      success('Đã xoá biến thể')
      onRefresh()
    } catch (err) {
      showError('Xoá thất bại', err instanceof Error ? err.message : undefined)
    } finally { setDeleting(false); setDeleteTarget(null) }
  }

  return (
    <>
      <div className="flex items-center justify-between">
        <p className="text-sm text-gray-400">{variants.length} biến thể</p>
        <Button size="sm" onClick={openCreate} className="gap-1.5"><Plus size={14} /> Thêm biến thể</Button>
      </div>
      {variants.length === 0 ? (
        <Card><p className="text-center text-gray-400 py-6 text-sm">Chưa có biến thể nào</p></Card>
      ) : (
        <Card className="p-0 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50/60">
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Tên biến thể</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">SKU</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">ĐVT</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">Quy đổi</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Barcode</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">Giá bán</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {variants.map(v => (
                  <tr key={v.id} className="hover:bg-gray-50/50">
                    <td className="px-4 py-3 font-medium text-gray-800">{v.variantName}</td>
                    <td className="px-4 py-3 text-gray-500 font-mono text-xs">{v.sku}</td>
                    <td className="px-4 py-3 text-gray-500">{VariantUnitLabel[v.unit] ?? v.unit}</td>
                    <td className="px-4 py-3 text-right text-gray-500">{v.quantityBaseUnit}</td>
                    <td className="px-4 py-3 text-gray-400 text-xs">{v.barcode ?? '—'}</td>
                    <td className="px-4 py-3 text-right text-gray-700">{formatCurrency(v.salePrice)}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-1">
                        <Button size="sm" variant="ghost" onClick={() => openEdit(v)}><Pencil size={14} /></Button>
                        <Button size="sm" variant="ghost" onClick={() => setDeleteTarget(v)} className="text-red-500"><Trash2 size={14} /></Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      <FormDialog open={showForm} title={editing ? 'Sửa biến thể' : 'Thêm biến thể'} loading={formLoading} onSubmit={handleSubmit} onCancel={() => setShowForm(false)}>
        <FormField label="Tên biến thể" required>
          <input className={inputClass} value={form.variantName} onChange={e => setForm(f => ({ ...f, variantName: e.target.value }))} placeholder="VD: Thùng 24 lon" />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Đơn vị tính" required>
            <select className={inputClass} value={form.unit} onChange={e => setForm(f => ({ ...f, unit: Number(e.target.value) }))}>
              {Object.entries(VariantUnitLabel).map(([code, label]) => (
                <option key={code} value={code}>{label}</option>
              ))}
            </select>
          </FormField>
          <FormField label="Quy đổi (đơn vị cơ sở)">
            <input type="number" className={inputClass} value={form.quantityBaseUnit || ''} onChange={e => setForm(f => ({ ...f, quantityBaseUnit: Number(e.target.value) }))} />
          </FormField>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Giá bán">
            <input type="number" className={inputClass} value={form.salePrice || ''} onChange={e => setForm(f => ({ ...f, salePrice: Number(e.target.value) }))} />
          </FormField>
          <FormField label="Giá vốn">
            <input type="number" className={inputClass} value={form.costPrice || ''} onChange={e => setForm(f => ({ ...f, costPrice: Number(e.target.value) }))} />
          </FormField>
        </div>
        <FormField label="Barcode">
          <input className={inputClass} value={form.barcode ?? ''} onChange={e => setForm(f => ({ ...f, barcode: e.target.value || undefined }))} />
        </FormField>
        <FormField label="Thứ tự hiển thị">
          <input type="number" className={inputClass} value={form.displayOrder ?? 0} onChange={e => setForm(f => ({ ...f, displayOrder: Number(e.target.value) }))} />
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      <ConfirmDialog open={deleteTarget !== null} title="Xoá biến thể?"
        description={`Xoá "${deleteTarget?.variantName}"?`}
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><Trash2 size={24} className="text-red-500" /></div>}
        confirmLabel="Xoá" variant="danger" loading={deleting} onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)} />
    </>
  )
}
