'use client'

import { useEffect, useState, useCallback } from 'react'
import { categoryApi } from '@/lib/api'
import type { CategoryDtoV2, CreateCategoryRequestV2, UpdateCategoryRequest } from '@/types'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { FormDialog, FormField, FormError, inputClass, selectClass } from '@/components/ui/FormDialog'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { Plus, Pencil, Trash2, FolderOpen, Folder, ChevronDown, ChevronRight, Package } from 'lucide-react'
import { cn } from '@/lib/utils'

type FormMode = 'create' | 'edit'

export default function CategoriesPage() {
  const { success, error: showError } = useToast()

  const [categories, setCategories] = useState<CategoryDtoV2[]>([])
  const [loading, setLoading] = useState(true)
  const [selected, setSelected] = useState<CategoryDtoV2 | null>(null)
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set())

  const [showForm, setShowForm] = useState(false)
  const [formMode, setFormMode] = useState<FormMode>('create')
  const [formLoading, setFormLoading] = useState(false)
  const [formError, setFormError] = useState('')
  const [createForm, setCreateForm] = useState<CreateCategoryRequestV2>({ name: '' })
  const [editForm, setEditForm] = useState<UpdateCategoryRequest>({ name: '' })
  const [editingId, setEditingId] = useState<string | null>(null)

  const [deleteTarget, setDeleteTarget] = useState<CategoryDtoV2 | null>(null)
  const [deleting, setDeleting] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await categoryApi.list()
      const arr = Array.isArray(data) ? data : []
      setCategories(arr)
    } catch {
      showError('Không thể tải danh mục')
    } finally { setLoading(false) }
  }, [showError])

  useEffect(() => { load() }, [load])

  function toggleExpand(id: string) {
    setExpandedIds(prev => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  function openCreate(parentCategoryId?: string) {
    setFormMode('create')
    setCreateForm({ name: '', description: '', parentCategoryId })
    setFormError('')
    setShowForm(true)
  }

  function openEdit(c: CategoryDtoV2) {
    setFormMode('edit')
    setEditingId(c.id)
    setEditForm({ name: c.name, description: c.description ?? '', parentCategoryId: c.parentCategoryId })
    setFormError('')
    setShowForm(true)
  }

  function flattenCats(cats: CategoryDtoV2[], result: CategoryDtoV2[] = []): CategoryDtoV2[] {
    for (const c of cats) {
      result.push(c)
      if (c.subCategories?.length) flattenCats(c.subCategories, result)
    }
    return result
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!createForm.name.trim()) { setFormError('Tên danh mục không được trống.'); return }
    setFormLoading(true)
    try {
      await categoryApi.create({ ...createForm, name: createForm.name.trim(), parentCategoryId: createForm.parentCategoryId || undefined })
      success('Thêm danh mục thành công')
      setShowForm(false)
      load()
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleEdit(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!editingId) return
    if (!editForm.name.trim()) { setFormError('Tên danh mục không được trống.'); return }
    setFormLoading(true)
    try {
      await categoryApi.update(editingId, { ...editForm, name: editForm.name.trim(), parentCategoryId: editForm.parentCategoryId ?? null })
      success('Cập nhật danh mục thành công')
      setShowForm(false)
      if (selected?.id === editingId) setSelected(null)
      load()
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Thao tác thất bại.')
    } finally { setFormLoading(false) }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await categoryApi.delete(deleteTarget.id)
      success('Đã xoá danh mục')
      if (selected?.id === deleteTarget.id) setSelected(null)
      load()
    } catch (err) {
      showError('Xoá thất bại', err instanceof Error ? err.message : undefined)
    } finally { setDeleting(false); setDeleteTarget(null) }
  }

  const allFlat = flattenCats(categories)
  const parentOptions = formMode === 'edit'
    ? allFlat.filter(c => c.id !== editingId)
    : allFlat

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Danh mục</h1>
        <Button size="sm" onClick={() => openCreate()} className="gap-1.5"><Plus size={15} /> Thêm danh mục</Button>
      </div>

      {loading ? <LoadingSpinner /> : (
        <div className="grid md:grid-cols-5 gap-4">
          {/* Tree panel — 40% */}
          <div className="md:col-span-2">
            <Card className="p-0 overflow-hidden">
              <div className="px-4 py-3 border-b border-gray-100 bg-gray-50/60">
                <h3 className="text-sm font-semibold text-gray-700">Cây danh mục</h3>
              </div>
              {categories.length === 0 ? (
                <p className="text-center text-gray-400 py-8 text-sm">Chưa có danh mục</p>
              ) : (
                <div className="overflow-y-auto max-h-[calc(100vh-300px)]">
                  <CategoryTree
                    cats={categories} depth={0}
                    expandedIds={expandedIds} selected={selected}
                    onToggle={toggleExpand}
                    onSelect={setSelected}
                    onEdit={openEdit}
                    onDelete={setDeleteTarget}
                    onAddChild={openCreate}
                  />
                </div>
              )}
            </Card>
          </div>

          {/* Detail panel — 60% */}
          <div className="md:col-span-3">
            {selected ? (
              <Card className="space-y-4">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-center gap-2.5">
                    <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center shrink-0">
                      <FolderOpen size={20} className="text-blue-600" />
                    </div>
                    <div>
                      <h2 className="text-lg font-bold text-gray-800">{selected.name}</h2>
                      {selected.parentCategoryName && (
                        <p className="text-xs text-gray-500 mt-0.5">Thuộc: {selected.parentCategoryName}</p>
                      )}
                    </div>
                  </div>
                  <div className="flex gap-1 shrink-0">
                    <Button size="sm" variant="secondary" onClick={() => openEdit(selected)} className="gap-1.5">
                      <Pencil size={14} /> Sửa
                    </Button>
                    <Button size="sm" variant="danger" onClick={() => setDeleteTarget(selected)} className="gap-1.5">
                      <Trash2 size={14} /> Xoá
                    </Button>
                  </div>
                </div>

                {selected.description && (
                  <p className="text-sm text-gray-600 border-l-2 border-blue-200 pl-3">{selected.description}</p>
                )}

                <div className="flex gap-4 text-sm">
                  <div className="bg-gray-50 rounded-lg px-4 py-3 text-center">
                    <p className="text-2xl font-bold text-gray-800">{selected.productCount}</p>
                    <p className="text-xs text-gray-500 mt-0.5 flex items-center gap-1"><Package size={11} /> Sản phẩm</p>
                  </div>
                  <div className="bg-gray-50 rounded-lg px-4 py-3 text-center">
                    <p className="text-2xl font-bold text-gray-800">{selected.subCategories?.length ?? 0}</p>
                    <p className="text-xs text-gray-500 mt-0.5 flex items-center gap-1"><Folder size={11} /> Danh mục con</p>
                  </div>
                </div>

                {(selected.subCategories?.length ?? 0) > 0 && (
                  <div>
                    <p className="text-sm font-medium text-gray-700 mb-2">Danh mục con</p>
                    <div className="space-y-1.5">
                      {selected.subCategories.map(sub => (
                        <button key={sub.id} onClick={() => setSelected(sub)}
                          className="w-full flex items-center gap-2.5 px-3 py-2 rounded-md text-sm text-gray-700 hover:bg-blue-50 hover:text-blue-700 transition-colors text-left">
                          <Folder size={15} className="text-blue-400 shrink-0" />
                          {sub.name}
                          <span className="ml-auto text-xs text-gray-400">{sub.productCount} SP</span>
                        </button>
                      ))}
                    </div>
                    <Button size="sm" variant="ghost" onClick={() => openCreate(selected.id)} className="mt-2 gap-1.5 text-blue-600">
                      <Plus size={14} /> Thêm danh mục con
                    </Button>
                  </div>
                )}

                {(selected.subCategories?.length ?? 0) === 0 && (
                  <Button size="sm" variant="ghost" onClick={() => openCreate(selected.id)} className="gap-1.5 text-blue-600">
                    <Plus size={14} /> Thêm danh mục con
                  </Button>
                )}
              </Card>
            ) : (
              <Card>
                <div className="flex flex-col items-center justify-center py-12 text-gray-400">
                  <FolderOpen size={40} className="mb-3 opacity-40" />
                  <p className="text-sm">Chọn một danh mục để xem chi tiết</p>
                </div>
              </Card>
            )}
          </div>
        </div>
      )}

      {/* Create modal */}
      <FormDialog open={showForm && formMode === 'create'} title="Thêm danh mục" loading={formLoading} onSubmit={handleCreate} onCancel={() => setShowForm(false)}>
        <FormField label="Tên danh mục" required>
          <input className={inputClass} value={createForm.name} onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))} placeholder="Nhập tên danh mục" />
        </FormField>
        <FormField label="Danh mục cha">
          <select className={selectClass} value={createForm.parentCategoryId ?? ''} onChange={e => setCreateForm(f => ({ ...f, parentCategoryId: e.target.value || undefined }))}>
            <option value="">— Gốc —</option>
            {parentOptions.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </FormField>
        <FormField label="Mô tả">
          <textarea className={inputClass} rows={2} value={createForm.description ?? ''} onChange={e => setCreateForm(f => ({ ...f, description: e.target.value || undefined }))} />
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      {/* Edit modal */}
      <FormDialog open={showForm && formMode === 'edit'} title="Sửa danh mục" loading={formLoading} onSubmit={handleEdit} onCancel={() => setShowForm(false)}>
        <FormField label="Tên danh mục" required>
          <input className={inputClass} value={editForm.name} onChange={e => setEditForm(f => ({ ...f, name: e.target.value }))} />
        </FormField>
        <FormField label="Danh mục cha">
          <select className={selectClass} value={editForm.parentCategoryId ?? ''} onChange={e => setEditForm(f => ({ ...f, parentCategoryId: e.target.value || null }))}>
            <option value="">— Gốc —</option>
            {parentOptions.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </FormField>
        <FormField label="Mô tả">
          <textarea className={inputClass} rows={2} value={editForm.description ?? ''} onChange={e => setEditForm(f => ({ ...f, description: e.target.value || undefined }))} />
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Xoá danh mục?"
        description={
          (deleteTarget?.subCategories?.length ?? 0) > 0
            ? `Không thể xoá "${deleteTarget?.name}" vì còn danh mục con.`
            : `Xoá "${deleteTarget?.name}"? Thao tác này không thể hoàn tác.`
        }
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><Trash2 size={24} className="text-red-500" /></div>}
        confirmLabel={(deleteTarget?.subCategories?.length ?? 0) > 0 ? undefined : 'Xoá'}
        cancelLabel={(deleteTarget?.subCategories?.length ?? 0) > 0 ? 'Đóng' : 'Huỷ'}
        variant="danger" loading={deleting}
        onConfirm={(deleteTarget?.subCategories?.length ?? 0) > 0 ? () => setDeleteTarget(null) : handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

// ─── Category Tree Component ──────────────────────────────────────────────────

interface TreeProps {
  cats: CategoryDtoV2[]
  depth: number
  expandedIds: Set<string>
  selected: CategoryDtoV2 | null
  onToggle: (id: string) => void
  onSelect: (c: CategoryDtoV2) => void
  onEdit: (c: CategoryDtoV2) => void
  onDelete: (c: CategoryDtoV2) => void
  onAddChild: (parentId: string) => void
}

function CategoryTree({ cats, depth, expandedIds, selected, onToggle, onSelect, onEdit, onDelete, onAddChild }: TreeProps) {
  return (
    <>
      {cats.map(c => {
        const hasChildren = (c.subCategories?.length ?? 0) > 0
        const isExpanded = expandedIds.has(c.id)
        const isSelected = selected?.id === c.id
        return (
          <div key={c.id}>
            <div className={cn(
              'flex items-center gap-1 px-3 py-2.5 hover:bg-gray-50 transition-colors cursor-pointer',
              isSelected && 'bg-blue-50',
              depth > 0 && 'border-l border-gray-100'
            )} style={{ paddingLeft: `${12 + depth * 20}px` }}>
              <button onClick={() => hasChildren && onToggle(c.id)} className={cn('shrink-0 w-5 h-5 flex items-center justify-center rounded', hasChildren ? 'text-gray-400 hover:text-gray-600' : 'text-transparent')}>
                {hasChildren
                  ? isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />
                  : <span className="w-2 h-2 rounded-full bg-gray-200" />
                }
              </button>
              <button onClick={() => onSelect(c)} className="flex-1 flex items-center gap-2 text-left min-w-0">
                {hasChildren
                  ? <FolderOpen size={15} className={cn(isSelected ? 'text-blue-600' : 'text-amber-500')} />
                  : <Folder size={15} className={cn(isSelected ? 'text-blue-600' : 'text-gray-400')} />
                }
                <span className={cn('text-sm truncate', isSelected ? 'font-semibold text-blue-700' : 'text-gray-700')}>{c.name}</span>
                {c.productCount > 0 && (
                  <span className="ml-auto text-xs text-gray-400 shrink-0">{c.productCount}</span>
                )}
              </button>
              <div className="flex items-center gap-0.5 shrink-0 opacity-0 group-hover:opacity-100 ml-1">
                <Button size="sm" variant="ghost" onClick={() => onAddChild(c.id)} className="h-7 w-7 p-0 flex items-center justify-center" title="Thêm con">
                  <Plus size={13} />
                </Button>
                <Button size="sm" variant="ghost" onClick={() => onEdit(c)} className="h-7 w-7 p-0 flex items-center justify-center">
                  <Pencil size={13} />
                </Button>
                <Button size="sm" variant="ghost" onClick={() => onDelete(c)} className="h-7 w-7 p-0 flex items-center justify-center text-red-500">
                  <Trash2 size={13} />
                </Button>
              </div>
            </div>
            {hasChildren && isExpanded && (
              <CategoryTree cats={c.subCategories} depth={depth + 1} expandedIds={expandedIds} selected={selected}
                onToggle={onToggle} onSelect={onSelect} onEdit={onEdit} onDelete={onDelete} onAddChild={onAddChild} />
            )}
          </div>
        )
      })}
    </>
  )
}
