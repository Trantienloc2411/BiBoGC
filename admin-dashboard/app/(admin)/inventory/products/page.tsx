'use client'

import { useEffect, useState, useCallback, useRef } from 'react'
import Link from 'next/link'
import { productApi, categoryApi } from '@/lib/api'
import type {
  ProductDto, ProductStatus,
  CreateProductRequestV2, UpdateProductRequest,
  CategoryTreeNode,
} from '@/types'
import { formatCurrency } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Pagination } from '@/components/ui/Pagination'
import { FormDialog, FormField, FormError, inputClass, selectClass } from '@/components/ui/FormDialog'
import { CategoryPicker } from '@/components/ui/CategoryPicker'
import { AsyncSupplierSelect } from '@/components/ui/AsyncSupplierSelect'
import { MoneyInput } from '@/components/ui/MoneyInput'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { Search, Eye, Plus, Pencil, Trash2, X, AlertTriangle, Download } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useExportFile } from '@/hooks/useExportFile'
import { exportExistingProducts } from '@/lib/exportService'

const DEFAULT_PAGE_SIZE = 10


const STATUS_STYLES: Record<string, string> = {
  Active: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  Inactive: 'bg-gray-100 text-gray-600 border-gray-200',
  OutOfStock: 'bg-orange-50 text-orange-700 border-orange-200',
  Discontinued: 'bg-red-50 text-red-600 border-red-200',
}
const STATUS_LABELS: Record<string, string> = {
  Active: 'Đang bán',
  Inactive: 'Ngừng bán',
  OutOfStock: 'Hết hàng',
  Discontinued: 'Ngừng kinh doanh',
}
const STATUS_VALUES: { value: number; label: string; key: ProductStatus }[] = [
  { value: 1, label: 'Đang bán', key: 'Active' },
  { value: 2, label: 'Ngừng bán', key: 'Inactive' },
  { value: 4, label: 'Hết hàng', key: 'OutOfStock' },
  { value: 3, label: 'Ngừng kinh doanh', key: 'Discontinued' },
]

const UNITS_OPTIONS: { value: number; label: string }[] = [
  { value: 1, label: 'Cái' }, { value: 2, label: 'Hộp' }, { value: 3, label: 'Chai' },
  { value: 4, label: 'Lon' }, { value: 5, label: 'Gói' }, { value: 6, label: 'Bịch' },
  { value: 7, label: 'Lốc' }, { value: 8, label: 'Thùng' }, { value: 9, label: 'Cuộn' },
  { value: 10, label: 'Vỉ' }, { value: 11, label: 'Cây' }, { value: 12, label: 'Thanh' },
  { value: 13, label: 'Túi' }, { value: 14, label: 'Bộ' }, { value: 15, label: 'Đôi' },
  { value: 16, label: 'Cân' }, { value: 21, label: 'Kg' }, { value: 22, label: 'Lạng' },
  { value: 31, label: 'Lít' }, { value: 40, label: 'Quả' }, { value: 41, label: 'Trái' },
]

type CreateForm = {
  name: string; description: string; sku: string; requiresBatchTracking: boolean
  price: number; baseUnits: number
  categoryId: string; supplierId: string
}
type EditForm = {
  name: string; description: string; price: number
  categoryId: string; supplierId: string; status: number
}

export default function ProductsPage() {
  const { success, error: showError } = useToast()
  const { exportFile: handleExportProducts, loading: exportLoading } = useExportFile(exportExistingProducts)

  const [products, setProducts] = useState<ProductDto[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const pageSizeRef = useRef(DEFAULT_PAGE_SIZE)
  pageSizeRef.current = pageSize
  const [loading, setLoading] = useState(true)

  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [supplierId, setSupplierId] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [lowStockOnly, setLowStockOnly] = useState(false)

  const [categories, setCategories] = useState<{ id: string; name: string; depth: number }[]>([])

  const [showCreate, setShowCreate] = useState(false)
  const [showEdit, setShowEdit] = useState(false)
  const [editingProduct, setEditingProduct] = useState<ProductDto | null>(null)
  const [formLoading, setFormLoading] = useState(false)
  const [formError, setFormError] = useState('')
  const [createForm, setCreateForm] = useState<CreateForm>({
    name: '', description: '', sku: '', requiresBatchTracking: false,
    price: 0, baseUnits: 1,
    categoryId: '', supplierId: '',
  })
  const [editForm, setEditForm] = useState<EditForm>({
    name: '', description: '', price: 0,
    categoryId: '', supplierId: '', status: 0,
  })

  const [pickerKey, setPickerKey] = useState(0)

  const [deleteTarget, setDeleteTarget] = useState<ProductDto | null>(null)
  const [deleting, setDeleting] = useState(false)

  const debounceRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined)

  useEffect(() => {
    clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => setSearchTerm(searchInput), 400)
    return () => clearTimeout(debounceRef.current)
  }, [searchInput])

  useEffect(() => {
    categoryApi.tree().then(tree => {
      setCategories(flattenTree(tree))
    }).catch(() => {})
  }, [])

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const ps = pageSizeRef.current
      const params: Record<string, unknown> = { pageNumber: p, pageSize: ps }
      if (searchTerm) params.searchTerm = searchTerm
      if (categoryId) params.categoryId = categoryId
      if (supplierId) params.supplierId = supplierId
      if (statusFilter) params.status = statusFilter
      if (lowStockOnly) params.lowStockOnly = true
      const data = await productApi.list(params as Parameters<typeof productApi.list>[0])
      setProducts(data.items)
      setTotalCount(data.totalCount)
    } catch {
      showError('Không thể tải danh sách sản phẩm')
    } finally {
      setLoading(false)
    }
  }, [searchTerm, categoryId, supplierId, statusFilter, lowStockOnly, showError])

  function handlePageSizeChange(newSize: number) {
    setPageSize(newSize)
    pageSizeRef.current = newSize
    setPage(1)
    load(1)
  }

  // Filter changes: reset to page 1 and reload. Also handles initial mount load.
  useEffect(() => { setPage(1); load(1) }, [searchTerm, categoryId, supplierId, statusFilter, lowStockOnly]) // eslint-disable-line react-hooks/exhaustive-deps
  // Pagination clicks: only fire for pages > 1 to avoid duplicating the filter effect on mount.
  useEffect(() => { if (page > 1) load(page) }, [page, load])

  function flattenTree(
    nodes: CategoryTreeNode[],
    depth = 0,
    result: { id: string; name: string; depth: number }[] = [],
  ): { id: string; name: string; depth: number }[] {
    for (const node of nodes) {
      result.push({ id: node.id, name: node.name, depth })
      if (node.subCategories?.length) flattenTree(node.subCategories, depth + 1, result)
    }
    return result
  }

  function openCreate() {
    setCreateForm({ name: '', description: '', sku: '', requiresBatchTracking: false, price: 0, baseUnits: 1, categoryId: '', supplierId: '' })
    setFormError('')
    setPickerKey(k => k + 1)
    setShowCreate(true)
  }

  function openEdit(p: ProductDto) {
    setEditingProduct(p)
    setEditForm({
      name: p.name, description: p.description ?? '',
      price: p.price,
      categoryId: p.categoryId ?? '', supplierId: p.supplierId ?? '',
      status: STATUS_VALUES.find(s => s.key === p.status)?.value ?? 0,
    })
    setFormError('')
    setShowEdit(true)
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!createForm.name.trim()) { setFormError('Tên sản phẩm không được để trống.'); return }
    if (!createForm.sku.trim()) { setFormError('SKU không được để trống.'); return }
    if (!createForm.baseUnits) { setFormError('Vui lòng chọn đơn vị tính.'); return }
    setFormLoading(true)
    try {
      const body: CreateProductRequestV2 = {
        name: createForm.name.trim(),
        sku: createForm.sku.trim(),
        description: createForm.description || undefined,
        requiresBatchTracking: createForm.requiresBatchTracking,
        price: createForm.price,
        categoryId: createForm.categoryId || undefined,
        baseUnits: createForm.baseUnits,
      }
      await productApi.create(body)
      success('Thêm sản phẩm thành công')
      setShowCreate(false)
      load(page)
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleEdit(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!editingProduct) return
    if (!editForm.name.trim()) { setFormError('Tên sản phẩm không được để trống.'); return }
    setFormLoading(true)
    try {
      const body: UpdateProductRequest = {
        name: editForm.name.trim(),
        description: editForm.description || undefined,
        price: editForm.price || undefined,
        status: editForm.status,
      }
      await productApi.update(editingProduct.id, body)
      success('Cập nhật sản phẩm thành công')
      setShowEdit(false)
      load(page)
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await productApi.delete(deleteTarget.id)
      success('Đã xoá sản phẩm')
      load(page)
    } catch (err) {
      showError('Xoá thất bại', err instanceof Error ? err.message : undefined)
    } finally { setDeleting(false); setDeleteTarget(null) }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Sản phẩm</h1>
        <div className="flex items-center gap-2">
          <Button
            size="sm"
            variant="secondary"
            onClick={handleExportProducts}
            loading={exportLoading}
            disabled={exportLoading}
            className="gap-1.5"
          >
            <Download size={15} />
            {exportLoading ? 'Đang xuất...' : 'Xuất sản phẩm'}
          </Button>
          <Button size="sm" onClick={openCreate} className="gap-1.5"><Plus size={15} /> Thêm sản phẩm</Button>
        </div>
      </div>

      <Card className="flex flex-wrap gap-3 items-end">
        <div className="flex-1 min-w-[180px]">
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Tìm kiếm</label>
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Tên, SKU..." value={searchInput} onChange={e => setSearchInput(e.target.value)}
              className="w-full text-sm text-gray-700 border border-gray-200 rounded-md pl-9 pr-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400" />
          </div>
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Danh mục</label>
          <select value={categoryId} onChange={e => setCategoryId(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400">
            <option value="">Tất cả</option>
            {categories.map(c => (
              <option key={c.id} value={c.id}>
                {c.depth > 0 ? `${'　'.repeat(c.depth)}└ ${c.name}` : c.name}
              </option>
            ))}
          </select>
        </div>
        <div className="min-w-[200px]">
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Nhà cung cấp</label>
          <AsyncSupplierSelect
            value={supplierId}
            onChange={setSupplierId}
            placeholder="Tất cả nhà cung cấp"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Trạng thái</label>
          <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
            className="text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400">
            <option value="">Tất cả</option>
            {STATUS_VALUES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
          </select>
        </div>
        <label className="flex items-center gap-2 pb-2 cursor-pointer">
          <input type="checkbox" checked={lowStockOnly} onChange={e => setLowStockOnly(e.target.checked)} className="rounded" />
          <span className="text-sm text-gray-600">Sắp hết hàng</span>
        </label>
        {(searchInput || categoryId || supplierId || statusFilter || lowStockOnly) && (
          <button onClick={() => { setSearchInput(''); setSearchTerm(''); setCategoryId(''); setSupplierId(''); setStatusFilter(''); setLowStockOnly(false) }}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 pb-2">
            <X size={14} /> Xoá lọc
          </button>
        )}
      </Card>

      <p className="text-sm text-gray-400 px-1">{totalCount.toLocaleString()} sản phẩm</p>

      <Card className="p-0 overflow-hidden">
        {loading ? (
          <div className="flex justify-center py-12"><LoadingSpinner /></div>
        ) : products.length === 0 ? (
          <p className="text-center text-gray-400 py-8 text-sm">Không có sản phẩm nào</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50/60">
                  <th className="text-left font-medium text-gray-500 px-4 py-3">SKU</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Tên sản phẩm</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Danh mục</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">Tồn kho</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">Giá bán</th>
                  <th className="text-left font-medium text-gray-500 px-4 py-3">Trạng thái</th>
                  <th className="text-right font-medium text-gray-500 px-4 py-3">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {products.map(p => (
                  <tr key={p.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-4 py-3 text-gray-500 font-mono text-xs">{p.sku}</td>
                    <td className="px-4 py-3">
                      <p className="font-medium text-gray-800">{p.name}</p>
                      {p.supplierName && <p className="text-xs text-gray-400 mt-0.5">{p.supplierName}</p>}
                    </td>
                    <td className="px-4 py-3 text-gray-500 text-sm">{p.categoryName ?? '—'}</td>
                    <td className="px-4 py-3 text-right">
                      <span className={cn('font-medium', p.isLowStock ? 'text-red-600' : 'text-gray-700')}>
                        {p.availableStock ?? 0}
                      </span>
                      {p.isLowStock && <AlertTriangle size={13} className="inline ml-1 text-amber-500" />}
                    </td>
                    <td className="px-4 py-3 text-right text-gray-700">{formatCurrency(p.price)}</td>
                    <td className="px-4 py-3">
                      <span className={cn('inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-md border',
                        STATUS_STYLES[p.status] ?? 'bg-gray-50 text-gray-600 border-gray-200')}>
                        {STATUS_LABELS[p.status] ?? p.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Link href={`/inventory/products/${p.id}`}>
                          <Button size="sm" variant="ghost" className="gap-1"><Eye size={14} /></Button>
                        </Link>
                        <Button size="sm" variant="ghost" onClick={() => openEdit(p)}><Pencil size={14} /></Button>
                        <Button size="sm" variant="ghost" onClick={() => setDeleteTarget(p)} className="text-red-500 hover:text-red-700"><Trash2 size={14} /></Button>
                      </div>
                    </td>
                  </tr>
                ))}
                {Array.from({ length: Math.max(0, pageSize - products.length) }).map((_, i) => (
                  <tr key={`empty-${i}`} className="h-[52px]"><td colSpan={7} /></tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="border-t border-gray-100">
          <Pagination
            page={page}
            pageSize={pageSize}
            totalItems={totalCount}
            onPageChange={setPage}
            onPageSizeChange={handlePageSizeChange}
          />
        </div>
      </Card>

      {/* Create modal */}
      <FormDialog open={showCreate} title="Thêm sản phẩm" loading={formLoading} onSubmit={handleCreate} onCancel={() => setShowCreate(false)}>
        <FormField label="Tên sản phẩm" required>
          <input className={inputClass} value={createForm.name} onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))} placeholder="Nhập tên sản phẩm" />
        </FormField>
        <FormField label="SKU" required>
          <input className={inputClass} value={createForm.sku} onChange={e => setCreateForm(f => ({ ...f, sku: e.target.value }))} placeholder="VD: SP001" />
        </FormField>
        <FormField label="Đơn vị tính" required>
          <select className={selectClass} value={createForm.baseUnits} onChange={e => setCreateForm(f => ({ ...f, baseUnits: Number(e.target.value) }))}>
            {UNITS_OPTIONS.map(u => <option key={u.value} value={u.value}>{u.label}</option>)}
          </select>
        </FormField>
        <FormField label="Danh mục">
          <CategoryPicker
            key={pickerKey}
            value={createForm.categoryId || null}
            onChange={id => setCreateForm(f => ({ ...f, categoryId: id ?? '' }))}
          />
        </FormField>
        <FormField label="Giá bán">
          <MoneyInput className={inputClass} value={createForm.price} onChange={v => setCreateForm(f => ({ ...f, price: v }))} placeholder="0" />
        </FormField>
        <FormField label="Mô tả">
          <textarea className={inputClass} rows={2} value={createForm.description} onChange={e => setCreateForm(f => ({ ...f, description: e.target.value }))} />
        </FormField>
        <label className="flex items-center gap-2 cursor-pointer">
          <input type="checkbox" checked={createForm.requiresBatchTracking} onChange={e => setCreateForm(f => ({ ...f, requiresBatchTracking: e.target.checked }))} className="rounded" />
          <span className="text-sm text-gray-700">Theo dõi theo lô hàng</span>
        </label>
        <FormError message={formError} />
      </FormDialog>

      {/* Edit modal */}
      <FormDialog open={showEdit} title="Sửa sản phẩm" loading={formLoading} onSubmit={handleEdit} onCancel={() => setShowEdit(false)}>
        <FormField label="Tên sản phẩm" required>
          <input className={inputClass} value={editForm.name} onChange={e => setEditForm(f => ({ ...f, name: e.target.value }))} />
        </FormField>
        <FormField label="SKU">
          <input className={cn(inputClass, 'bg-gray-50 text-gray-400 cursor-not-allowed')} value={editingProduct?.sku ?? ''} readOnly disabled />
        </FormField>
        <FormField label="Giá bán">
          <MoneyInput className={inputClass} value={editForm.price} onChange={v => setEditForm(f => ({ ...f, price: v }))} placeholder="0" />
        </FormField>
        <FormField label="Trạng thái">
          <select className={selectClass} value={editForm.status} onChange={e => setEditForm(f => ({ ...f, status: Number(e.target.value) }))}>
            {STATUS_VALUES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
          </select>
        </FormField>
        <FormField label="Mô tả">
          <textarea className={inputClass} rows={2} value={editForm.description} onChange={e => setEditForm(f => ({ ...f, description: e.target.value }))} />
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      <ConfirmDialog open={deleteTarget !== null} title="Xoá sản phẩm?"
        description={`Bạn có chắc muốn xoá "${deleteTarget?.name}"? Thao tác này không thể hoàn tác.`}
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><Trash2 size={24} className="text-red-500" /></div>}
        confirmLabel="Xoá" variant="danger" loading={deleting} onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)} />
    </div>
  )
}
