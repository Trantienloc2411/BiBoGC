'use client'

import { useEffect, useState, useCallback, useRef } from 'react'
import { supplierApi } from '@/lib/api'
import type { SupplierDtoV2, CreateSupplierRequestV2 } from '@/types'
import { formatDate } from '@/lib/utils'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Pagination } from '@/components/ui/Pagination'
import { FormDialog, FormField, FormError, inputClass } from '@/components/ui/FormDialog'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { Plus, Pencil, Trash2, Search, X, Phone, Mail, MapPin, Building2, FileText, StickyNote } from 'lucide-react'

const PAGE_SIZE = 12

export default function SuppliersPage() {
  const { success, error: showError } = useToast()

  const [suppliers, setSuppliers] = useState<SupplierDtoV2[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')

  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<SupplierDtoV2 | null>(null)
  const [formLoading, setFormLoading] = useState(false)
  const [formError, setFormError] = useState('')
  const [form, setForm] = useState<CreateSupplierRequestV2>({ name: '' })

  const [deleteTarget, setDeleteTarget] = useState<SupplierDtoV2 | null>(null)
  const [deleting, setDeleting] = useState(false)

  const debounceRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined)

  useEffect(() => {
    clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => setSearchTerm(searchInput), 400)
    return () => clearTimeout(debounceRef.current)
  }, [searchInput])

  const load = useCallback(async (p: number) => {
    setLoading(true)
    try {
      const data = await supplierApi.list({ pageNumber: p, pageSize: PAGE_SIZE, searchTerm: searchTerm || undefined })
      setSuppliers(data.items)
      setTotalCount(data.totalCount)
      setTotalPages(data.totalPages)
    } catch {
      showError('Không thể tải danh sách nhà cung cấp')
    } finally { setLoading(false) }
  }, [searchTerm, showError])

  useEffect(() => { setPage(1); load(1) }, [searchTerm]) // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => { load(page) }, [page, load])

  function openCreate() {
    setEditing(null)
    setForm({ name: '', contactPerson: '', phone: '', email: '', address: '', taxCode: '', notes: '' })
    setFormError('')
    setShowForm(true)
  }

  function openEdit(s: SupplierDtoV2) {
    setEditing(s)
    setForm({
      name: s.name, contactPerson: s.contactPerson ?? '', phone: s.phone ?? '',
      email: s.email ?? '', address: s.address ?? '', taxCode: s.taxCode ?? '',
      notes: s.notes ?? '',
    })
    setFormError('')
    setShowForm(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!form.name.trim()) { setFormError('Tên nhà cung cấp không được trống.'); return }
    setFormLoading(true)
    try {
      const body: CreateSupplierRequestV2 = {
        name: form.name.trim(),
        contactPerson: form.contactPerson || undefined,
        phone: form.phone || undefined,
        email: form.email || undefined,
        address: form.address || undefined,
        taxCode: form.taxCode || undefined,
        notes: form.notes || undefined,
      }
      if (editing) {
        await supplierApi.update(editing.id, body)
        success('Cập nhật nhà cung cấp thành công')
      } else {
        await supplierApi.create(body)
        success('Thêm nhà cung cấp thành công')
      }
      setShowForm(false)
      load(page)
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await supplierApi.delete(deleteTarget.id)
      success('Đã xoá nhà cung cấp')
      load(page)
    } catch (err) {
      showError('Xoá thất bại', err instanceof Error ? err.message : undefined)
    } finally { setDeleting(false); setDeleteTarget(null) }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Nhà cung cấp</h1>
        <Button size="sm" onClick={openCreate} className="gap-1.5"><Plus size={15} /> Thêm NCC</Button>
      </div>

      <Card className="flex flex-wrap gap-3 items-end">
        <div className="flex-1 min-w-[220px]">
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Tìm kiếm</label>
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Tên, liên hệ, email..." value={searchInput} onChange={e => setSearchInput(e.target.value)}
              className="w-full text-sm text-gray-700 border border-gray-200 rounded-md pl-9 pr-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-400" />
          </div>
        </div>
        {searchInput && (
          <button onClick={() => { setSearchInput(''); setSearchTerm('') }} className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 pb-2">
            <X size={14} /> Xoá
          </button>
        )}
      </Card>

      <p className="text-sm text-gray-400 px-1">{totalCount.toLocaleString()} nhà cung cấp</p>

      {loading ? <LoadingSpinner /> : (
        <>
          {suppliers.length === 0 ? (
            <Card><p className="text-center text-gray-400 py-8 text-sm">Không có nhà cung cấp nào</p></Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {suppliers.map(s => (
                <Card key={s.id} className="flex flex-col gap-3">
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex items-center gap-2.5 min-w-0">
                      <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center shrink-0">
                        <Building2 size={18} className="text-blue-600" />
                      </div>
                      <div className="min-w-0">
                        <p className="font-semibold text-gray-800 truncate">{s.name}</p>
                        {s.contactPerson && <p className="text-xs text-gray-500 mt-0.5 truncate">{s.contactPerson}</p>}
                      </div>
                    </div>
                    <div className="flex items-center gap-1 shrink-0">
                      <Button size="sm" variant="ghost" onClick={() => openEdit(s)} className="h-8 w-8 p-0 flex items-center justify-center">
                        <Pencil size={14} />
                      </Button>
                      <Button size="sm" variant="ghost" onClick={() => setDeleteTarget(s)} className="h-8 w-8 p-0 flex items-center justify-center text-red-500">
                        <Trash2 size={14} />
                      </Button>
                    </div>
                  </div>

                  <div className="space-y-1.5 text-xs text-gray-500">
                    {s.phone && (
                      <a href={`tel:${s.phone}`} className="flex items-center gap-1.5 hover:text-blue-600 transition-colors">
                        <Phone size={12} className="shrink-0" /> {s.phone}
                      </a>
                    )}
                    {s.email && (
                      <a href={`mailto:${s.email}`} className="flex items-center gap-1.5 hover:text-blue-600 transition-colors truncate">
                        <Mail size={12} className="shrink-0" /> {s.email}
                      </a>
                    )}
                    {s.address && (
                      <span className="flex items-start gap-1.5">
                        <MapPin size={12} className="shrink-0 mt-0.5" /> {s.address}
                      </span>
                    )}
                    {s.taxCode && (
                      <span className="flex items-center gap-1.5">
                        <FileText size={12} className="shrink-0" /> MST: {s.taxCode}
                      </span>
                    )}
                    {s.notes && (
                      <span className="flex items-start gap-1.5 text-gray-400 italic">
                        <StickyNote size={12} className="shrink-0 mt-0.5" /> {s.notes}
                      </span>
                    )}
                  </div>

                  {s.createdAt && (
                    <p className="text-xs text-gray-300 mt-auto pt-2 border-t border-gray-50">
                      Thêm ngày {formatDate(s.createdAt)}
                    </p>
                  )}
                </Card>
              ))}
            </div>
          )}
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}

      <FormDialog open={showForm} title={editing ? 'Sửa nhà cung cấp' : 'Thêm nhà cung cấp'} loading={formLoading} onSubmit={handleSubmit} onCancel={() => setShowForm(false)}>
        <FormField label="Tên nhà cung cấp" required>
          <input className={inputClass} value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} placeholder="Nhập tên NCC" />
        </FormField>
        <FormField label="Người liên hệ">
          <input className={inputClass} value={form.contactPerson ?? ''} onChange={e => setForm(f => ({ ...f, contactPerson: e.target.value }))} />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Điện thoại">
            <input type="tel" className={inputClass} value={form.phone ?? ''} onChange={e => setForm(f => ({ ...f, phone: e.target.value }))} />
          </FormField>
          <FormField label="Email">
            <input type="email" className={inputClass} value={form.email ?? ''} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
          </FormField>
        </div>
        <FormField label="Địa chỉ">
          <input className={inputClass} value={form.address ?? ''} onChange={e => setForm(f => ({ ...f, address: e.target.value }))} />
        </FormField>
        <FormField label="Mã số thuế">
          <input className={inputClass} value={form.taxCode ?? ''} onChange={e => setForm(f => ({ ...f, taxCode: e.target.value }))} />
        </FormField>
        <FormField label="Ghi chú">
          <textarea className={inputClass} rows={2} value={form.notes ?? ''} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} />
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      <ConfirmDialog open={deleteTarget !== null} title="Xoá nhà cung cấp?"
        description={`Xoá "${deleteTarget?.name}"? Thao tác này không thể hoàn tác.`}
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><Trash2 size={24} className="text-red-500" /></div>}
        confirmLabel="Xoá" variant="danger" loading={deleting} onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)} />
    </div>
  )
}
