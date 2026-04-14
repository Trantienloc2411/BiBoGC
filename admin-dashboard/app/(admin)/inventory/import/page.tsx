'use client'

import { useState, useRef, useEffect, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { supplierApi, productApi, stockTransactionApi } from '@/lib/api'
import type {
  SupplierDtoV2, CreateSupplierRequestV2,
  ProductDto, CreateProductRequestV2,
  AddBatchRequest,
} from '@/types'
import { Button } from '@/components/ui/Button'
import { FormField, FormError, inputClass, selectClass } from '@/components/ui/FormDialog'
import { CategoryPicker } from '@/components/ui/CategoryPicker'
import { MoneyInput } from '@/components/ui/MoneyInput'
import { useToast } from '@/components/ui/Toast'
import { formatCurrency, formatNumber } from '@/lib/utils'
import {
  ArrowLeft, Check, Search, Plus, ChevronRight,
  Building2, Package, Layers, Receipt, AlertCircle,
  Pencil, Save, Clock,
} from 'lucide-react'
import { cn } from '@/lib/utils'

// ─── Draft types ──────────────────────────────────────────────────────────────

const DRAFT_KEY = 'goods_receiving_draft'

interface ImportDraft {
  step: Step
  supplierMode: SupplierMode
  supplierQuery: string
  selectedSupplier: SupplierDtoV2 | null
  newSupplierForm: CreateSupplierRequestV2
  productMode: ProductMode
  productQuery: string
  selectedProduct: ProductDto | null
  newProductForm: CreateProductRequestV2
  batchForm: AddBatchRequest
  unitPrice: number
  notes: string
  savedAt: string
}

function saveDraft(draft: ImportDraft) {
  try { localStorage.setItem(DRAFT_KEY, JSON.stringify(draft)) } catch { /* ignore */ }
}

function clearDraft() {
  try { localStorage.removeItem(DRAFT_KEY) } catch { /* ignore */ }
}

function loadDraft(): ImportDraft | null {
  try {
    const raw = localStorage.getItem(DRAFT_KEY)
    if (!raw) return null
    const draft: ImportDraft = JSON.parse(raw)
    const age = Date.now() - new Date(draft.savedAt).getTime()
    if (age > 48 * 60 * 60 * 1000) { clearDraft(); return null }
    return draft
  } catch { clearDraft(); return null }
}

const UNITS_OPTIONS: { value: number; label: string }[] = [
  { value: 1, label: 'Cái' }, { value: 2, label: 'Hộp' }, { value: 3, label: 'Chai' },
  { value: 4, label: 'Lon' }, { value: 5, label: 'Gói' }, { value: 6, label: 'Bịch' },
  { value: 7, label: 'Lốc' }, { value: 8, label: 'Thùng' }, { value: 9, label: 'Cuộn' },
  { value: 10, label: 'Vỉ' }, { value: 11, label: 'Cây' }, { value: 12, label: 'Thanh' },
  { value: 13, label: 'Túi' }, { value: 14, label: 'Bộ' }, { value: 15, label: 'Đôi' },
  { value: 16, label: 'Cân' }, { value: 21, label: 'Kg' }, { value: 22, label: 'Lạng' },
  { value: 31, label: 'Lít' }, { value: 40, label: 'Quả' }, { value: 41, label: 'Trái' },
]

type Step = 1 | 2 | 3
type SupplierMode = 'search' | 'create'
type ProductMode = 'search' | 'create'

const STEPS: { id: Step; label: string; icon: React.ReactNode }[] = [
  { id: 1, label: 'Nhà cung cấp', icon: <Building2 size={15} /> },
  { id: 2, label: 'Sản phẩm', icon: <Package size={15} /> },
  { id: 3, label: 'Lô hàng & Xác nhận', icon: <Layers size={15} /> },
]

export default function ImportProductPage() {
  const router = useRouter()
  const { success } = useToast()

  const [step, setStep] = useState<Step>(1)
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState('')

  // ─── Draft state ──────────────────────────────────────────────────────────
  const [showRestoreModal, setShowRestoreModal] = useState(false)
  const [draftToRestore, setDraftToRestore] = useState<ImportDraft | null>(null)
  const [showExitModal, setShowExitModal] = useState(false)
  const [lastSaved, setLastSaved] = useState<Date | null>(null)
  const autoSaveTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

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
    name: '', sku: '', price: 0, requiresBatchTracking: true, baseUnits: 1,
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
      if (!newProductForm.baseUnits) { setProductError('Vui lòng chọn đơn vị tính.'); return }
    }
    setStep(3)
  }

  // ─── Step 3: Batch & details ─────────────────────────────────────────────

  const [batchForm, setBatchForm] = useState<AddBatchRequest>({ batchNumber: '', quantity: 1 })
  const [unitPrice, setUnitPrice] = useState(0)
  const [notes, setNotes] = useState('')
  const [batchError, setBatchError] = useState('')

  const requiresBatch = selectedProduct?.requiresBatchTracking ?? newProductForm.requiresBatchTracking
  const supplierName = supplierMode === 'create' ? newSupplierForm.name : selectedSupplier?.name
  const productName = productMode === 'create' ? newProductForm.name : selectedProduct?.name
  const total = batchForm.quantity * unitPrice

  // ─── isDirty: true when any meaningful data has been entered ───────────────
  const isDirty = !!(
    selectedSupplier ||
    newSupplierForm.name ||
    selectedProduct ||
    newProductForm.name ||
    batchForm.batchNumber ||
    batchForm.quantity > 1 ||
    unitPrice > 0 ||
    notes
  )

  // ─── On mount: check for saved draft ──────────────────────────────────────
  useEffect(() => {
    const draft = loadDraft()
    if (draft) {
      setDraftToRestore(draft)
      setShowRestoreModal(true)
    }
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  // ─── Auto-save every 3s when form is dirty ────────────────────────────────
  const buildDraft = useCallback((): ImportDraft => ({
    step, supplierMode, supplierQuery, selectedSupplier,
    newSupplierForm, productMode, productQuery, selectedProduct,
    newProductForm, batchForm, unitPrice, notes,
    savedAt: new Date().toISOString(),
  }), [step, supplierMode, supplierQuery, selectedSupplier, newSupplierForm, productMode, productQuery, selectedProduct, newProductForm, batchForm, unitPrice, notes])

  useEffect(() => {
    if (!isDirty) return
    if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current)
    autoSaveTimerRef.current = setTimeout(() => {
      const draft = buildDraft()
      saveDraft(draft)
      setLastSaved(new Date())
    }, 3000)
    return () => { if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current) }
  }, [isDirty, buildDraft])

  // ─── Warn on browser close/refresh ────────────────────────────────────────
  useEffect(() => {
    function handleBeforeUnload(e: BeforeUnloadEvent) {
      if (isDirty) { e.preventDefault(); e.returnValue = '' }
    }
    window.addEventListener('beforeunload', handleBeforeUnload)
    return () => window.removeEventListener('beforeunload', handleBeforeUnload)
  }, [isDirty])

  // ─── Restore draft helper ─────────────────────────────────────────────────
  function restoreDraft(draft: ImportDraft) {
    setStep(draft.step)
    setSupplierMode(draft.supplierMode)
    setSupplierQuery(draft.supplierQuery)
    setSelectedSupplier(draft.selectedSupplier)
    setNewSupplierForm(draft.newSupplierForm)
    setProductMode(draft.productMode)
    setProductQuery(draft.productQuery)
    setSelectedProduct(draft.selectedProduct)
    setNewProductForm(draft.newProductForm)
    setBatchForm(draft.batchForm)
    setUnitPrice(draft.unitPrice)
    setNotes(draft.notes)
    setShowRestoreModal(false)
    setDraftToRestore(null)
  }

  // ─── Navigation protection ────────────────────────────────────────────────
  function handleNavigateAway(destination: string) {
    if (isDirty) {
      setShowExitModal(true)
      // Store destination in a ref so we can use it when modal confirms
      pendingNavRef.current = destination
    } else {
      router.push(destination)
    }
  }

  const pendingNavRef = useRef<string>('/inventory/products')

  function exitSaveDraft() {
    const draft = buildDraft()
    saveDraft(draft)
    setLastSaved(new Date())
    setShowExitModal(false)
    router.push(pendingNavRef.current)
  }

  function exitDiscardDraft() {
    clearDraft()
    setShowExitModal(false)
    router.push(pendingNavRef.current)
  }

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
      let supplierId: string
      if (supplierMode === 'create') {
        const s = await supplierApi.create(newSupplierForm)
        supplierId = s.id
      } else {
        supplierId = selectedSupplier!.id
      }

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

      await stockTransactionApi.purchase({
        productId,
        productBatchId,
        supplierId,
        quantity: batchForm.quantity,
        unitPrice: unitPrice || 0,
        notes: notes || undefined,
      })

      clearDraft()
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
    <div className="space-y-5">

      {/* ── Draft restore modal ────────────────────────────────────────────── */}
      {showRestoreModal && draftToRestore && (
        <div className="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6 space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 bg-blue-50 rounded-xl flex items-center justify-center shrink-0">
                <Clock size={20} className="text-blue-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-800">Có bản nháp chưa hoàn thành</h3>
                <p className="text-xs text-gray-500 mt-0.5">
                  Đã lưu lúc {new Date(draftToRestore.savedAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
            </div>
            <p className="text-sm text-gray-600">Bạn có muốn tiếp tục từ bản nháp không?</p>
            <div className="flex gap-2 pt-1">
              <Button className="flex-1" onClick={() => restoreDraft(draftToRestore)}>
                Tiếp tục nháp
              </Button>
              <Button variant="secondary" className="flex-1" onClick={() => { clearDraft(); setShowRestoreModal(false) }}>
                Bắt đầu mới
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* ── Exit confirmation modal ────────────────────────────────────────── */}
      {showExitModal && (
        <div className="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6 space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 bg-amber-50 rounded-xl flex items-center justify-center shrink-0">
                <Save size={20} className="text-amber-600" />
              </div>
              <h3 className="font-semibold text-gray-800">Lưu nháp trước khi thoát?</h3>
            </div>
            <p className="text-sm text-gray-600">Dữ liệu bạn đã nhập sẽ bị mất nếu không lưu nháp.</p>
            <div className="space-y-2 pt-1">
              <Button className="w-full gap-2" onClick={exitSaveDraft}>
                <Save size={14} /> Lưu nháp và thoát
              </Button>
              <Button variant="secondary" className="w-full" onClick={exitDiscardDraft}>
                Không lưu, thoát luôn
              </Button>
              <Button variant="ghost" className="w-full text-gray-500" onClick={() => setShowExitModal(false)}>
                Huỷ — tiếp tục nhập
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Page header */}
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={() => handleNavigateAway('/inventory/products')}
          className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">
          <ArrowLeft size={18} />
        </button>
        <div className="flex-1 min-w-0">
          <h1 className="text-xl font-bold text-gray-800">Nhập hàng</h1>
          <p className="text-sm text-gray-500 mt-0.5">Tiếp nhận hàng hóa theo từng bước</p>
        </div>
        {/* Draft saved indicator */}
        {lastSaved && (
          <div className="flex items-center gap-1.5 text-xs text-emerald-600 bg-emerald-50 border border-emerald-200 px-2.5 py-1 rounded-lg">
            <Save size={12} />
            Nháp lúc {lastSaved.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
          </div>
        )}
      </div>

      {/* Main 2-column layout */}
      <div className="grid lg:grid-cols-3 gap-5 items-start">

        {/* ── Left: Step workflow (65%) ──────────────────────────────────── */}
        <div className="lg:col-span-2 space-y-4">

          {/* Step indicator bar */}
          <div className="bg-white border border-gray-200 rounded-xl shadow-sm px-5 py-4">
            <div className="flex items-center gap-0">
              {STEPS.map((s, i) => (
                <div key={s.id} className="flex items-center flex-1">
                  <div className="flex items-center gap-2.5">
                    <div className={cn(
                      'flex items-center justify-center w-8 h-8 rounded-full text-sm font-semibold shrink-0 transition-all',
                      step > s.id
                        ? 'bg-emerald-500 text-white shadow-sm shadow-emerald-200'
                        : step === s.id
                        ? 'bg-blue-600 text-white shadow-sm shadow-blue-200'
                        : 'bg-gray-100 text-gray-400',
                    )}>
                      {step > s.id ? <Check size={14} /> : s.icon}
                    </div>
                    <div>
                      <p className={cn(
                        'text-xs font-medium leading-none',
                        step === s.id ? 'text-gray-800' : step > s.id ? 'text-emerald-600' : 'text-gray-400',
                      )}>
                        {s.label}
                      </p>
                      <p className={cn(
                        'text-xs mt-0.5',
                        step === s.id ? 'text-blue-500' : step > s.id ? 'text-emerald-400' : 'text-gray-300',
                      )}>
                        {step > s.id ? 'Hoàn thành' : step === s.id ? 'Đang thực hiện' : 'Chờ'}
                      </p>
                    </div>
                  </div>
                  {i < STEPS.length - 1 && (
                    <div className={cn(
                      'flex-1 h-px mx-3 transition-colors',
                      step > s.id ? 'bg-emerald-300' : 'bg-gray-200',
                    )} />
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* ── Step 1: Supplier ────────────────────────────────────────── */}
          {step === 1 && (
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm">
              <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 bg-blue-50 rounded-lg flex items-center justify-center">
                    <Building2 size={16} className="text-blue-600" />
                  </div>
                  <h2 className="font-semibold text-gray-800">Chọn Nhà cung cấp</h2>
                </div>
                <button
                  type="button"
                  onClick={() => { setSupplierMode(m => m === 'search' ? 'create' : 'search'); setSupplierError('') }}
                  className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1.5 font-medium bg-blue-50 hover:bg-blue-100 px-3 py-1.5 rounded-lg transition-colors"
                >
                  {supplierMode === 'search' ? <><Plus size={13} /> Tạo mới</> : <><Search size={13} /> Tìm kiếm</>}
                </button>
              </div>

              <div className="px-6 py-5 space-y-4">
                {supplierMode === 'search' ? (
                  <div className="space-y-3">
                    <div className="relative">
                      <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="Nhập tên nhà cung cấp để tìm..."
                        value={supplierQuery}
                        autoComplete="off"
                        onChange={e => { setSupplierQuery(e.target.value); searchSuppliers(e.target.value) }}
                        onFocus={() => { if (!supplierResults.length) searchSuppliers('') }}
                      />
                    </div>
                    {supplierSearching && <p className="text-sm text-gray-400 text-center py-2">Đang tìm...</p>}
                    {!supplierSearching && supplierResults.length > 0 && (
                      <div className="border border-gray-200 rounded-lg divide-y divide-gray-100 max-h-52 overflow-y-auto shadow-sm">
                        {supplierResults.map(s => (
                          <button key={s.id} type="button"
                            onClick={() => { setSelectedSupplier(s); setSupplierQuery(s.name); setSupplierError('') }}
                            className={cn(
                              'w-full text-left px-4 py-3 text-sm transition-colors flex items-center gap-3',
                              selectedSupplier?.id === s.id
                                ? 'bg-blue-50 text-blue-700'
                                : 'hover:bg-gray-50 text-gray-700',
                            )}
                          >
                            <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center shrink-0">
                              <Building2 size={14} className="text-gray-400" />
                            </div>
                            <div className="min-w-0">
                              <p className="font-medium truncate">{s.name}</p>
                              {(s.contactPhone || s.contactName) && (
                                <p className="text-xs text-gray-400 mt-0.5">{[s.contactName, s.contactPhone].filter(Boolean).join(' · ')}</p>
                              )}
                            </div>
                            {selectedSupplier?.id === s.id && <Check size={14} className="ml-auto shrink-0 text-blue-600" />}
                          </button>
                        ))}
                      </div>
                    )}
                    {!supplierSearching && supplierQuery && supplierResults.length === 0 && (
                      <div className="text-center py-4 text-sm text-gray-400">
                        <p>Không tìm thấy &ldquo;{supplierQuery}&rdquo;</p>
                        <button type="button" className="mt-1 text-blue-600 hover:underline font-medium" onClick={() => {
                          setNewSupplierForm(f => ({ ...f, name: supplierQuery }))
                          setSupplierMode('create')
                          setSupplierError('')
                        }}>
                          + Tạo mới &ldquo;{supplierQuery}&rdquo;
                        </button>
                      </div>
                    )}
                    {selectedSupplier && (
                      <div className="flex items-center gap-2.5 px-4 py-3 bg-emerald-50 border border-emerald-200 rounded-lg text-sm text-emerald-700">
                        <Check size={15} className="shrink-0" />
                        <span>Đã chọn: <strong>{selectedSupplier.name}</strong></span>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="space-y-4">
                    <div className="flex items-center gap-2 text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-4 py-2.5">
                      <AlertCircle size={15} className="shrink-0" />
                      Điền thông tin để tạo nhà cung cấp mới
                    </div>
                    <FormField label="Tên nhà cung cấp" required>
                      <input className={inputClass} value={newSupplierForm.name}
                        onChange={e => setNewSupplierForm(f => ({ ...f, name: e.target.value }))}
                        placeholder="Tên công ty / cá nhân" />
                    </FormField>
                    <div className="grid grid-cols-2 gap-3">
                      <FormField label="Người liên hệ">
                        <input className={inputClass} value={newSupplierForm.contactPerson ?? ''}
                          onChange={e => setNewSupplierForm(f => ({ ...f, contactPerson: e.target.value || undefined }))} />
                      </FormField>
                      <FormField label="Số điện thoại">
                        <input type="tel" className={inputClass} value={newSupplierForm.phoneNumber ?? ''}
                          onChange={e => setNewSupplierForm(f => ({ ...f, phoneNumber: e.target.value || undefined }))} />
                      </FormField>
                    </div>
                    <FormField label="Địa chỉ">
                      <input className={inputClass} value={newSupplierForm.address ?? ''}
                        onChange={e => setNewSupplierForm(f => ({ ...f, address: e.target.value || undefined }))} />
                    </FormField>
                  </div>
                )}
                {supplierError && <FormError message={supplierError} />}
              </div>

              <div className="px-6 py-4 border-t border-gray-100 bg-gray-50/50 flex justify-end">
                <Button type="button" onClick={goToStep2} className="gap-1.5">
                  Tiếp theo <ChevronRight size={15} />
                </Button>
              </div>
            </div>
          )}

          {/* ── Step 2: Product ──────────────────────────────────────────── */}
          {step === 2 && (
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm">
              <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 bg-blue-50 rounded-lg flex items-center justify-center">
                    <Package size={16} className="text-blue-600" />
                  </div>
                  <h2 className="font-semibold text-gray-800">Chọn Sản phẩm</h2>
                </div>
                <button
                  type="button"
                  onClick={() => { setProductMode(m => m === 'search' ? 'create' : 'search'); setProductError('') }}
                  className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1.5 font-medium bg-blue-50 hover:bg-blue-100 px-3 py-1.5 rounded-lg transition-colors"
                >
                  {productMode === 'search' ? <><Plus size={13} /> Tạo mới</> : <><Search size={13} /> Tìm kiếm</>}
                </button>
              </div>

              <div className="px-6 py-5 space-y-4">
                {productMode === 'search' ? (
                  <div className="space-y-3">
                    <div className="relative">
                      <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="Nhập tên hoặc SKU sản phẩm..."
                        value={productQuery}
                        autoComplete="off"
                        onChange={e => { setProductQuery(e.target.value); searchProducts(e.target.value) }}
                        onFocus={() => { if (!productResults.length) searchProducts('') }}
                      />
                    </div>
                    {productSearching && <p className="text-sm text-gray-400 text-center py-2">Đang tìm...</p>}
                    {!productSearching && productResults.length > 0 && (
                      <div className="border border-gray-200 rounded-lg divide-y divide-gray-100 max-h-52 overflow-y-auto shadow-sm">
                        {productResults.map(p => (
                          <button key={p.id} type="button"
                            onClick={() => { setSelectedProduct(p); setProductQuery(p.name); setProductError('') }}
                            className={cn(
                              'w-full text-left px-4 py-3 text-sm transition-colors flex items-center gap-3',
                              selectedProduct?.id === p.id
                                ? 'bg-blue-50 text-blue-700'
                                : 'hover:bg-gray-50 text-gray-700',
                            )}
                          >
                            <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center shrink-0">
                              <Package size={14} className="text-gray-400" />
                            </div>
                            <div className="min-w-0">
                              <p className="font-medium truncate">{p.name}</p>
                              <p className="text-xs text-gray-400 mt-0.5 font-mono">{p.sku}</p>
                            </div>
                            {selectedProduct?.id === p.id && <Check size={14} className="ml-auto shrink-0 text-blue-600" />}
                          </button>
                        ))}
                      </div>
                    )}
                    {!productSearching && productQuery && productResults.length === 0 && (
                      <div className="text-center py-4 text-sm text-gray-400">
                        <p>Không tìm thấy &ldquo;{productQuery}&rdquo;</p>
                        <button type="button" className="mt-1 text-blue-600 hover:underline font-medium" onClick={() => {
                          setNewProductForm(f => ({ ...f, name: productQuery }))
                          setProductMode('create')
                          setProductError('')
                        }}>
                          + Tạo mới &ldquo;{productQuery}&rdquo;
                        </button>
                      </div>
                    )}
                    {selectedProduct && (
                      <div className="flex items-center gap-2.5 px-4 py-3 bg-emerald-50 border border-emerald-200 rounded-lg text-sm text-emerald-700">
                        <Check size={15} className="shrink-0" />
                        <span>Đã chọn: <strong>{selectedProduct.name}</strong></span>
                        <span className="ml-auto font-mono text-xs text-emerald-500">{selectedProduct.sku}</span>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="space-y-4">
                    <div className="flex items-center gap-2 text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-4 py-2.5">
                      <AlertCircle size={15} className="shrink-0" />
                      Điền thông tin để tạo sản phẩm mới
                    </div>
                    <FormField label="Tên sản phẩm" required>
                      <input className={inputClass} value={newProductForm.name}
                        onChange={e => setNewProductForm(f => ({ ...f, name: e.target.value }))}
                        placeholder="VD: Bia Heineken" />
                    </FormField>
                    <div className="grid grid-cols-2 gap-3">
                      <FormField label="SKU" required>
                        <input className={inputClass} value={newProductForm.sku}
                          onChange={e => setNewProductForm(f => ({ ...f, sku: e.target.value }))}
                          placeholder="VD: BIA-HEI-330" />
                      </FormField>
                      <FormField label="Giá bán">
                        <MoneyInput className={inputClass} value={newProductForm.price ?? 0} onChange={v => setNewProductForm(f => ({ ...f, price: v }))} placeholder="0" />
                      </FormField>
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <FormField label="Đơn vị tính" required>
                        <select className={selectClass} value={newProductForm.baseUnits}
                          onChange={e => setNewProductForm(f => ({ ...f, baseUnits: Number(e.target.value) }))}>
                          {UNITS_OPTIONS.map(u => <option key={u.value} value={u.value}>{u.label}</option>)}
                        </select>
                      </FormField>
                      <FormField label="Theo dõi theo lô hàng">
                        <select className={selectClass} value={newProductForm.requiresBatchTracking ? '1' : '0'}
                          onChange={e => setNewProductForm(f => ({ ...f, requiresBatchTracking: e.target.value === '1' }))}>
                          <option value="1">Có — theo lô</option>
                          <option value="0">Không</option>
                        </select>
                      </FormField>
                    </div>
                    <FormField label="Danh mục">
                      <CategoryPicker value={newProductForm.categoryId ?? null}
                        onChange={id => setNewProductForm(f => ({ ...f, categoryId: id ?? undefined }))} />
                    </FormField>
                  </div>
                )}
                {productError && <FormError message={productError} />}
              </div>

              <div className="px-6 py-4 border-t border-gray-100 bg-gray-50/50 flex justify-between">
                <Button type="button" variant="secondary" onClick={() => setStep(1)}>Quay lại</Button>
                <Button type="button" onClick={goToStep3} className="gap-1.5">
                  Tiếp theo <ChevronRight size={15} />
                </Button>
              </div>
            </div>
          )}

          {/* ── Step 3: Batch & Confirm ──────────────────────────────────── */}
          {step === 3 && (
            <form onSubmit={handleSubmit}>
              <div className="bg-white border border-gray-200 rounded-xl shadow-sm">
                <div className="px-6 py-4 border-b border-gray-100 flex items-center gap-2.5">
                  <div className="w-8 h-8 bg-blue-50 rounded-lg flex items-center justify-center">
                    <Layers size={16} className="text-blue-600" />
                  </div>
                  <h2 className="font-semibold text-gray-800">Thông tin lô hàng</h2>
                </div>

                <div className="px-6 py-5 space-y-4">
                  {requiresBatch ? (
                    <>
                      <FormField label="Số lô" required>
                        <input className={inputClass} value={batchForm.batchNumber}
                          onChange={e => setBatchForm(f => ({ ...f, batchNumber: e.target.value }))}
                          placeholder="VD: LOT-2025-001" />
                      </FormField>
                      <div className="grid grid-cols-2 gap-3">
                        <FormField label="Ngày sản xuất">
                          <input type="date" className={inputClass} value={batchForm.manufacturingDate ?? ''}
                            max={batchForm.expirationDate || undefined}
                            onChange={e => {
                              const val = e.target.value
                              setBatchForm(f => ({
                                ...f,
                                manufacturingDate: val || undefined,
                                expirationDate: f.expirationDate && val > f.expirationDate ? undefined : f.expirationDate,
                              }))
                            }} />
                        </FormField>
                        <FormField label="Hạn sử dụng">
                          <input type="date" className={inputClass} value={batchForm.expirationDate ?? ''}
                            min={batchForm.manufacturingDate || undefined}
                            onChange={e => {
                              const val = e.target.value
                              setBatchForm(f => ({
                                ...f,
                                expirationDate: val || undefined,
                                manufacturingDate: f.manufacturingDate && val < f.manufacturingDate ? undefined : f.manufacturingDate,
                              }))
                            }} />
                        </FormField>
                      </div>
                    </>
                  ) : (
                    <div className="flex items-center gap-2 text-sm text-gray-500 bg-gray-50 rounded-lg px-4 py-3 border border-gray-200">
                      <Check size={14} className="text-gray-400 shrink-0" />
                      Sản phẩm này không yêu cầu theo dõi lô hàng.
                    </div>
                  )}

                  <div className="grid grid-cols-2 gap-3">
                    <FormField label="Số lượng nhập" required>
                      <input type="number" min={1} className={inputClass} value={batchForm.quantity || ''}
                        onChange={e => setBatchForm(f => ({ ...f, quantity: Number(e.target.value) }))} />
                    </FormField>
                    <FormField label="Đơn giá nhập (đ)">
                      <MoneyInput className={inputClass} value={unitPrice} onChange={setUnitPrice} placeholder="0" />
                    </FormField>
                  </div>

                  <FormField label="Ghi chú">
                    <textarea className={inputClass} rows={3} value={notes}
                      onChange={e => setNotes(e.target.value)}
                      placeholder="Ghi chú thêm về lô hàng..." />
                  </FormField>

                  {batchError && <FormError message={batchError} />}
                  {submitError && <FormError message={submitError} />}
                </div>

                <div className="px-6 py-4 border-t border-gray-100 bg-gray-50/50 flex justify-between">
                  <Button type="button" variant="secondary" onClick={() => setStep(2)}>Quay lại</Button>
                  <Button type="submit" loading={submitting} className="gap-1.5">
                    <Check size={15} /> Xác nhận nhập hàng
                  </Button>
                </div>
              </div>
            </form>
          )}
        </div>

        {/* ── Right: Sticky Summary Panel (35%) ─────────────────────────── */}
        <div className="lg:col-span-1">
          <div className="sticky top-6 space-y-4">

            {/* Summary card */}
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
              <div className="px-4 py-3.5 border-b border-gray-100 bg-gray-50/50 flex items-center gap-2">
                <Receipt size={15} className="text-gray-500" />
                <h3 className="text-sm font-semibold text-gray-700">Tóm tắt đơn nhập</h3>
              </div>

              <div className="p-4 space-y-3">
                {/* Supplier */}
                <div className={cn(
                  'rounded-lg p-3 space-y-1',
                  step >= 1 && supplierName ? 'bg-emerald-50 border border-emerald-100' : 'bg-gray-50 border border-dashed border-gray-200',
                )}>
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-1.5">
                      <Building2 size={13} className={supplierName ? 'text-emerald-500' : 'text-gray-300'} />
                      <span className="text-xs font-medium text-gray-500">Nhà cung cấp</span>
                    </div>
                    {step > 1 && (
                      <button onClick={() => setStep(1)}
                        className="text-xs text-blue-500 hover:text-blue-700 flex items-center gap-0.5">
                        <Pencil size={10} /> Sửa
                      </button>
                    )}
                  </div>
                  {supplierName ? (
                    <p className="text-sm font-semibold text-gray-800 break-words">{supplierName}</p>
                  ) : (
                    <p className="text-sm text-gray-400 italic">Chưa chọn</p>
                  )}
                  {supplierMode === 'create' && supplierName && (
                    <span className="text-xs text-amber-600 bg-amber-50 border border-amber-200 px-1.5 py-0.5 rounded">Tạo mới</span>
                  )}
                </div>

                {/* Product */}
                <div className={cn(
                  'rounded-lg p-3 space-y-1',
                  step >= 2 && productName ? 'bg-emerald-50 border border-emerald-100' : 'bg-gray-50 border border-dashed border-gray-200',
                )}>
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-1.5">
                      <Package size={13} className={productName ? 'text-emerald-500' : 'text-gray-300'} />
                      <span className="text-xs font-medium text-gray-500">Sản phẩm</span>
                    </div>
                    {step > 2 && (
                      <button onClick={() => setStep(2)}
                        className="text-xs text-blue-500 hover:text-blue-700 flex items-center gap-0.5">
                        <Pencil size={10} /> Sửa
                      </button>
                    )}
                  </div>
                  {productName ? (
                    <p className="text-sm font-semibold text-gray-800 break-words">{productName}</p>
                  ) : (
                    <p className="text-sm text-gray-400 italic">Chưa chọn</p>
                  )}
                  {productMode === 'create' && productName && (
                    <span className="text-xs text-amber-600 bg-amber-50 border border-amber-200 px-1.5 py-0.5 rounded">Tạo mới</span>
                  )}
                </div>

                {/* Quantity & price (shown from step 3) */}
                {step === 3 && (
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm py-1 border-t border-gray-100 pt-3">
                      <span className="text-gray-500">Số lượng</span>
                      <span className="font-medium text-gray-800">{batchForm.quantity > 0 ? formatNumber(batchForm.quantity) : '—'}</span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-500">Đơn giá</span>
                      <span className="font-medium text-gray-800">{unitPrice > 0 ? formatCurrency(unitPrice) : '—'}</span>
                    </div>
                    {(batchForm.quantity > 0 && unitPrice > 0) && (
                      <div className="flex items-center justify-between text-sm bg-blue-50 rounded-lg px-3 py-2.5 border border-blue-100 mt-2">
                        <span className="font-semibold text-blue-700">Thành tiền</span>
                        <span className="font-bold text-blue-800 text-base">{formatCurrency(total)}</span>
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>

            {/* Progress indicator */}
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
              <p className="text-xs font-medium text-gray-500 mb-3">Tiến trình</p>
              <div className="space-y-2.5">
                {STEPS.map(s => (
                  <div key={s.id} className="flex items-center gap-2.5">
                    <div className={cn(
                      'w-5 h-5 rounded-full flex items-center justify-center shrink-0',
                      step > s.id ? 'bg-emerald-500' : step === s.id ? 'bg-blue-600' : 'bg-gray-100',
                    )}>
                      {step > s.id
                        ? <Check size={10} className="text-white" />
                        : <span className={cn('text-xs font-bold', step === s.id ? 'text-white' : 'text-gray-400')}>{s.id}</span>
                      }
                    </div>
                    <span className={cn(
                      'text-sm',
                      step === s.id ? 'font-medium text-gray-800' : step > s.id ? 'text-emerald-600' : 'text-gray-400',
                    )}>
                      {s.label}
                    </span>
                  </div>
                ))}
              </div>
            </div>

            {/* Cancel button */}
            <button
              type="button"
              onClick={() => handleNavigateAway('/inventory/products')}
              className="w-full text-center text-sm text-gray-400 hover:text-gray-600 py-2 transition-colors"
            >
              Huỷ và quay lại
            </button>

          </div>
        </div>

      </div>
    </div>
  )
}
