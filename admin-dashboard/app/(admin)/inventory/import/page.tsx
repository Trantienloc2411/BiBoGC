'use client'

import { useState, useRef } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { supplierApi, productApi, stockTransactionApi } from '@/lib/api'
import type {
  SupplierDtoV2, CreateSupplierRequestV2,
  ProductDto, CreateProductRequestV2,
  AddBatchRequest,
} from '@/types'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { FormField, FormError, inputClass, selectClass } from '@/components/ui/FormDialog'
import { useToast } from '@/components/ui/Toast'
import { ArrowLeft, Check, Search, Plus, ChevronRight } from 'lucide-react'
import { cn } from '@/lib/utils'

type Step = 1 | 2 | 3
type SupplierMode = 'search' | 'create'
type ProductMode = 'search' | 'create'

const STEPS: { id: Step; label: string }[] = [
  { id: 1, label: 'Nhà cung cấp' },
  { id: 2, label: 'Sản phẩm' },
  { id: 3, label: 'Lô hàng & Xác nhận' },
]

export default function ImportProductPage() {
  const router = useRouter()
  const { success } = useToast()

  const [step, setStep] = useState<Step>(1)
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState('')

  // ─── Step 1: Supplier ────────────────────────────────────────────────────

  const [supplierMode, setSupplierMode] = useState<SupplierMode>('search')
  const [supplierQuery, setSupplierQuery] = useState('')
  const [supplierResults, setSupplierResults] = useState<SupplierDtoV2[]>([])
  const [supplierSearching, setSupplierSearching] = useState(false)
  const [selectedSupplier, setSelectedSupplier] = useState<SupplierDtoV2 | null>(null)
  const [newSupplierForm, setNewSupplierForm] = useState<CreateSupplierRequestV2>({ name: '' })
  const [supplierError, setSupplierError] = useState('')
  const supplierTimer = useRef<ReturnType<typeof setTimeout> | null>(null)

  function searchSuppliers(q: string) {
    if (supplierTimer.current) clearTimeout(supplierTimer.current)
    setSupplierSearching(true)
    supplierTimer.current = setTimeout(async () => {
      try {
        const data = await supplierApi.list({ pageSize: 10, searchTerm: q || undefined })
        setSupplierResults(data.items)
      } catch { /* ignore */ } finally { setSupplierSearching(false) }
    }, 300)
  }

  function goToStep2() {
    setSupplierError('')
    if (supplierMode === 'search' && !selectedSupplier) {
      setSupplierError('Vui lòng chọn nhà cung cấp hoặc chuyển sang tạo mới.')
      return
    }
    if (supplierMode === 'create' && !newSupplierForm.name.trim()) {
      setSupplierError('Tên nhà cung cấp không được trống.')
      return
    }
    setStep(2)
  }

  // ─── Step 2: Product ─────────────────────────────────────────────────────

  const [productMode, setProductMode] = useState<ProductMode>('search')
  const [productQuery, setProductQuery] = useState('')
  const [productResults, setProductResults] = useState<ProductDto[]>([])
  const [productSearching, setProductSearching] = useState(false)
  const [selectedProduct, setSelectedProduct] = useState<ProductDto | null>(null)
  const [newProductForm, setNewProductForm] = useState<CreateProductRequestV2>({
    name: '', sku: '', price: 0, requiresBatchTracking: true,
  })
  const [productError, setProductError] = useState('')
  const productTimer = useRef<ReturnType<typeof setTimeout> | null>(null)

  function searchProducts(q: string) {
    if (productTimer.current) clearTimeout(productTimer.current)
    setProductSearching(true)
    productTimer.current = setTimeout(async () => {
      try {
        const data = await productApi.list({ pageSize: 10, searchTerm: q || undefined })
        setProductResults(data.items)
      } catch { /* ignore */ } finally { setProductSearching(false) }
    }, 300)
  }

  function goToStep3() {
    setProductError('')
    if (productMode === 'search' && !selectedProduct) {
      setProductError('Vui lòng chọn sản phẩm hoặc chuyển sang tạo mới.')
      return
    }
    if (productMode === 'create') {
      if (!newProductForm.name.trim()) { setProductError('Tên sản phẩm không được trống.'); return }
      if (!newProductForm.sku.trim()) { setProductError('Mã SKU không được trống.'); return }
    }
    setStep(3)
  }

  // ─── Step 3: Batch & details ─────────────────────────────────────────────

  const [batchForm, setBatchForm] = useState<AddBatchRequest>({ batchNumber: '', quantity: 1 })
  const [unitPrice, setUnitPrice] = useState(0)
  const [notes, setNotes] = useState('')
  const [batchError, setBatchError] = useState('')

  const requiresBatch = selectedProduct?.requiresBatchTracking ?? newProductForm.requiresBatchTracking

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setBatchError('')
    setSubmitError('')

    if (requiresBatch && !batchForm.batchNumber.trim()) {
      setBatchError('Số lô không được trống.')
      return
    }
    if (batchForm.quantity <= 0) {
      setBatchError('Số lượng phải lớn hơn 0.')
      return
    }

    setSubmitting(true)
    try {
      // A — Supplier: create if new, else use selected
      let supplierId: string
      if (supplierMode === 'create') {
        const s = await supplierApi.create(newSupplierForm)
        supplierId = s.id
      } else {
        supplierId = selectedSupplier!.id
      }

      // B — Product: create if new, else use selected
      let productId: string
      let needsBatch: boolean
      if (productMode === 'create') {
        const p = await productApi.create(newProductForm)
        productId = p.id
        needsBatch = newProductForm.requiresBatchTracking
      } else {
        productId = selectedProduct!.id
        needsBatch = selectedProduct!.requiresBatchTracking
      }

      // C — Batch: create if product requires batch tracking
      let productBatchId: string | undefined
      if (needsBatch && batchForm.batchNumber.trim()) {
        const b = await productApi.addBatch(productId, {
          batchNumber: batchForm.batchNumber.trim(),
          quantity: batchForm.quantity,
          costPrice: batchForm.costPrice,
          manufacturingDate: batchForm.manufacturingDate,
          expirationDate: batchForm.expirationDate,
        })
        productBatchId = b.id
      }

      // D — StockTransaction (Purchase)
      await stockTransactionApi.purchase({
        productId,
        productBatchId,
        supplierId,
        quantity: batchForm.quantity,
        unitPrice: unitPrice || 0,
        notes: notes || undefined,
      })

      success('Nhập hàng thành công')
      router.push('/inventory/stock-transactions')
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Nhập hàng thất bại.')
    } finally {
      setSubmitting(false)
    }
  }

  // ─── Render ──────────────────────────────────────────────────────────────

  return (
    <div className="space-y-6 max-w-2xl">

      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/inventory/products"
          className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
          <ArrowLeft size={18} />
        </Link>
        <div>
          <h1 className="text-xl font-bold text-gray-800">Nhập hàng</h1>
          <p className="text-sm text-gray-500 mt-0.5">Tiếp nhận hóa đơn nhập hàng theo từng bước</p>
        </div>
      </div>

      {/* Step indicator */}
      <div className="flex items-center gap-2 flex-wrap">
        {STEPS.map((s, i) => (
          <div key={s.id} className="flex items-center gap-2">
            <div className={cn(
              'flex items-center justify-center w-7 h-7 rounded-full text-xs font-semibold shrink-0',
              step > s.id
                ? 'bg-emerald-500 text-white'
                : step === s.id
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-400',
            )}>
              {step > s.id ? <Check size={13} /> : s.id}
            </div>
            <span className={cn(
              'text-sm',
              step === s.id ? 'font-medium text-gray-800' : step > s.id ? 'text-emerald-600' : 'text-gray-400',
            )}>
              {s.label}
            </span>
            {i < STEPS.length - 1 && <ChevronRight size={15} className="text-gray-300 mx-1 shrink-0" />}
          </div>
        ))}
      </div>

      {/* ── Step 1: Supplier ─────────────────────────────────────────────── */}
      {step === 1 && (
        <Card className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="font-semibold text-gray-700">Kiểm tra Nhà cung cấp</h2>
            <button
              type="button"
              onClick={() => {
                setSupplierMode(m => m === 'search' ? 'create' : 'search')
                setSupplierError('')
              }}
              className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 font-medium"
            >
              {supplierMode === 'search'
                ? <><Plus size={14} /> Tạo mới</>
                : <><Search size={14} /> Tìm kiếm</>}
            </button>
          </div>

          {supplierMode === 'search' ? (
            <div className="space-y-3">
              <input
                className={inputClass}
                placeholder="Nhập tên nhà cung cấp để tìm..."
                value={supplierQuery}
                autoComplete="off"
                onChange={e => { setSupplierQuery(e.target.value); searchSuppliers(e.target.value) }}
                onFocus={() => { if (!supplierResults.length) searchSuppliers('') }}
              />
              {supplierSearching && <p className="text-sm text-gray-400">Đang tìm...</p>}
              {!supplierSearching && supplierResults.length > 0 && (
                <div className="border border-gray-200 rounded-md divide-y divide-gray-100 max-h-52 overflow-y-auto">
                  {supplierResults.map(s => (
                    <button
                      key={s.id}
                      type="button"
                      onClick={() => { setSelectedSupplier(s); setSupplierQuery(s.name); setSupplierError('') }}
                      className={cn(
                        'w-full text-left px-4 py-3 text-sm transition-colors',
                        selectedSupplier?.id === s.id
                          ? 'bg-blue-50 text-blue-700'
                          : 'hover:bg-gray-50 text-gray-700',
                      )}
                    >
                      <p className="font-medium">{s.name}</p>
                      {(s.contactPhone || s.contactName) && (
                        <p className="text-xs text-gray-400 mt-0.5">
                          {[s.contactName, s.contactPhone].filter(Boolean).join(' · ')}
                        </p>
                      )}
                    </button>
                  ))}
                </div>
              )}
              {!supplierSearching && supplierQuery && supplierResults.length === 0 && (
                <p className="text-sm text-gray-400">
                  Không tìm thấy nhà cung cấp —{' '}
                  <button type="button" className="text-blue-600 hover:underline" onClick={() => {
                    setNewSupplierForm(f => ({ ...f, name: supplierQuery }))
                    setSupplierMode('create')
                    setSupplierError('')
                  }}>
                    tạo mới &quot;{supplierQuery}&quot;
                  </button>
                </p>
              )}
              {selectedSupplier && (
                <div className="flex items-center gap-2 px-3 py-2 bg-emerald-50 border border-emerald-200 rounded-md text-sm text-emerald-700">
                  <Check size={14} />
                  <span>Đã tồn tại: <strong>{selectedSupplier.name}</strong></span>
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-3">
              <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-md px-3 py-2">
                Chưa tồn tại — điền thông tin để tạo nhà cung cấp mới
              </p>
              <FormField label="Tên nhà cung cấp" required>
                <input
                  className={inputClass}
                  value={newSupplierForm.name}
                  onChange={e => setNewSupplierForm(f => ({ ...f, name: e.target.value }))}
                  placeholder="Tên công ty / cá nhân"
                />
              </FormField>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Người liên hệ">
                  <input
                    className={inputClass}
                    value={newSupplierForm.contactPerson ?? ''}
                    onChange={e => setNewSupplierForm(f => ({ ...f, contactPerson: e.target.value || undefined }))}
                  />
                </FormField>
                <FormField label="Số điện thoại">
                  <input
                    className={inputClass}
                    value={newSupplierForm.phoneNumber ?? ''}
                    onChange={e => setNewSupplierForm(f => ({ ...f, phoneNumber: e.target.value || undefined }))}
                  />
                </FormField>
              </div>
              <FormField label="Địa chỉ">
                <input
                  className={inputClass}
                  value={newSupplierForm.address ?? ''}
                  onChange={e => setNewSupplierForm(f => ({ ...f, address: e.target.value || undefined }))}
                />
              </FormField>
            </div>
          )}

          {supplierError && <FormError message={supplierError} />}

          <div className="flex justify-end pt-1">
            <Button type="button" onClick={goToStep2} className="gap-1.5">
              Tiếp theo <ChevronRight size={15} />
            </Button>
          </div>
        </Card>
      )}

      {/* ── Step 2: Product ──────────────────────────────────────────────── */}
      {step === 2 && (
        <Card className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="font-semibold text-gray-700">Kiểm tra Sản phẩm</h2>
            <button
              type="button"
              onClick={() => {
                setProductMode(m => m === 'search' ? 'create' : 'search')
                setProductError('')
              }}
              className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 font-medium"
            >
              {productMode === 'search'
                ? <><Plus size={14} /> Tạo mới</>
                : <><Search size={14} /> Tìm kiếm</>}
            </button>
          </div>

          {productMode === 'search' ? (
            <div className="space-y-3">
              <input
                className={inputClass}
                placeholder="Nhập tên hoặc SKU sản phẩm để tìm..."
                value={productQuery}
                autoComplete="off"
                onChange={e => { setProductQuery(e.target.value); searchProducts(e.target.value) }}
                onFocus={() => { if (!productResults.length) searchProducts('') }}
              />
              {productSearching && <p className="text-sm text-gray-400">Đang tìm...</p>}
              {!productSearching && productResults.length > 0 && (
                <div className="border border-gray-200 rounded-md divide-y divide-gray-100 max-h-52 overflow-y-auto">
                  {productResults.map(p => (
                    <button
                      key={p.id}
                      type="button"
                      onClick={() => { setSelectedProduct(p); setProductQuery(p.name); setProductError('') }}
                      className={cn(
                        'w-full text-left px-4 py-3 text-sm transition-colors',
                        selectedProduct?.id === p.id
                          ? 'bg-blue-50 text-blue-700'
                          : 'hover:bg-gray-50 text-gray-700',
                      )}
                    >
                      <p className="font-medium">{p.name}</p>
                      <p className="text-xs text-gray-400 mt-0.5 font-mono">{p.sku}</p>
                    </button>
                  ))}
                </div>
              )}
              {!productSearching && productQuery && productResults.length === 0 && (
                <p className="text-sm text-gray-400">
                  Không tìm thấy sản phẩm —{' '}
                  <button type="button" className="text-blue-600 hover:underline" onClick={() => {
                    setNewProductForm(f => ({ ...f, name: productQuery }))
                    setProductMode('create')
                    setProductError('')
                  }}>
                    tạo mới &quot;{productQuery}&quot;
                  </button>
                </p>
              )}
              {selectedProduct && (
                <div className="flex items-center gap-2 px-3 py-2 bg-emerald-50 border border-emerald-200 rounded-md text-sm text-emerald-700">
                  <Check size={14} />
                  <span>Đã tồn tại: <strong>{selectedProduct.name}</strong></span>
                  <span className="text-emerald-500 font-mono text-xs ml-auto">{selectedProduct.sku}</span>
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-3">
              <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-md px-3 py-2">
                Chưa tồn tại — điền thông tin để tạo sản phẩm mới
              </p>
              <FormField label="Tên sản phẩm" required>
                <input
                  className={inputClass}
                  value={newProductForm.name}
                  onChange={e => setNewProductForm(f => ({ ...f, name: e.target.value }))}
                  placeholder="VD: Bia Heineken"
                />
              </FormField>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="SKU" required>
                  <input
                    className={inputClass}
                    value={newProductForm.sku}
                    onChange={e => setNewProductForm(f => ({ ...f, sku: e.target.value }))}
                    placeholder="VD: BIA-HEI-330"
                  />
                </FormField>
                <FormField label="Giá bán">
                  <input
                    type="number"
                    min={0}
                    className={inputClass}
                    value={newProductForm.price || ''}
                    onChange={e => setNewProductForm(f => ({ ...f, price: Number(e.target.value) }))}
                  />
                </FormField>
              </div>
              <FormField label="Theo dõi theo lô hàng">
                <select
                  className={selectClass}
                  value={newProductForm.requiresBatchTracking ? '1' : '0'}
                  onChange={e => setNewProductForm(f => ({ ...f, requiresBatchTracking: e.target.value === '1' }))}
                >
                  <option value="1">Có — yêu cầu quản lý theo lô</option>
                  <option value="0">Không</option>
                </select>
              </FormField>
            </div>
          )}

          {productError && <FormError message={productError} />}

          <div className="flex justify-between pt-1">
            <Button type="button" variant="secondary" onClick={() => setStep(1)}>
              Quay lại
            </Button>
            <Button type="button" onClick={goToStep3} className="gap-1.5">
              Tiếp theo <ChevronRight size={15} />
            </Button>
          </div>
        </Card>
      )}

      {/* ── Step 3: Batch & Confirm ──────────────────────────────────────── */}
      {step === 3 && (
        <form onSubmit={handleSubmit}>
          <Card className="space-y-4">
            <h2 className="font-semibold text-gray-700">Tạo Lô hàng & Xác nhận</h2>

            {/* Summary of previous steps */}
            <div className="bg-gray-50 border border-gray-200 rounded-md px-4 py-3 space-y-1.5 text-sm">
              <p>
                <span className="text-gray-500">Nhà cung cấp:</span>{' '}
                <span className="font-medium text-gray-800">
                  {supplierMode === 'create' ? newSupplierForm.name : selectedSupplier?.name}
                </span>
                {supplierMode === 'create' && (
                  <span className="ml-2 text-xs text-amber-600 bg-amber-50 border border-amber-200 px-1.5 py-0.5 rounded">Tạo mới</span>
                )}
              </p>
              <p>
                <span className="text-gray-500">Sản phẩm:</span>{' '}
                <span className="font-medium text-gray-800">
                  {productMode === 'create' ? newProductForm.name : selectedProduct?.name}
                </span>
                {productMode === 'create' && (
                  <span className="ml-2 text-xs text-amber-600 bg-amber-50 border border-amber-200 px-1.5 py-0.5 rounded">Tạo mới</span>
                )}
              </p>
            </div>

            {/* Batch fields */}
            {requiresBatch ? (
              <>
                <FormField label="Số lô" required>
                  <input
                    className={inputClass}
                    value={batchForm.batchNumber}
                    onChange={e => setBatchForm(f => ({ ...f, batchNumber: e.target.value }))}
                    placeholder="VD: LOT-2025-001"
                  />
                </FormField>
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Ngày sản xuất">
                    <input
                      type="date"
                      className={inputClass}
                      value={batchForm.manufacturingDate ?? ''}
                      onChange={e => setBatchForm(f => ({ ...f, manufacturingDate: e.target.value || undefined }))}
                    />
                  </FormField>
                  <FormField label="Hạn sử dụng">
                    <input
                      type="date"
                      className={inputClass}
                      value={batchForm.expirationDate ?? ''}
                      onChange={e => setBatchForm(f => ({ ...f, expirationDate: e.target.value || undefined }))}
                    />
                  </FormField>
                </div>
              </>
            ) : (
              <p className="text-sm text-gray-400 bg-gray-50 rounded-md px-3 py-2">
                Sản phẩm này không yêu cầu theo dõi lô hàng.
              </p>
            )}

            <div className="grid grid-cols-2 gap-3">
              <FormField label="Số lượng nhập" required>
                <input
                  type="number"
                  min={1}
                  className={inputClass}
                  value={batchForm.quantity || ''}
                  onChange={e => setBatchForm(f => ({ ...f, quantity: Number(e.target.value) }))}
                />
              </FormField>
              <FormField label="Đơn giá nhập">
                <input
                  type="number"
                  min={0}
                  className={inputClass}
                  value={unitPrice || ''}
                  onChange={e => setUnitPrice(Number(e.target.value))}
                  placeholder="0"
                />
              </FormField>
            </div>

            <FormField label="Ghi chú">
              <textarea
                className={inputClass}
                rows={2}
                value={notes}
                onChange={e => setNotes(e.target.value)}
                placeholder="Ghi chú thêm về lô hàng..."
              />
            </FormField>

            {batchError && <FormError message={batchError} />}
            {submitError && <FormError message={submitError} />}

            <div className="flex justify-between pt-1">
              <Button type="button" variant="secondary" onClick={() => setStep(2)}>
                Quay lại
              </Button>
              <Button type="submit" loading={submitting} className="gap-1.5">
                <Check size={15} /> Xác nhận nhập hàng
              </Button>
            </div>
          </Card>
        </form>
      )}
    </div>
  )
}
