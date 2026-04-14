'use client'

import { useEffect, useState, useCallback, useMemo } from 'react'
import { categoryApi } from '@/lib/api'
import type {
  CategoryTreeNode, CategoryDtoV2,
  CreateCategoryRequestV2, UpdateCategoryRequest,
} from '@/types'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { FormDialog, FormField, FormError, inputClass, selectClass } from '@/components/ui/FormDialog'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import {
  Plus, Pencil, Trash2, FolderOpen, Folder,
  ChevronDown, ChevronRight, ChevronsDownUp, ChevronsUpDown,
  Search, X, Tag, Package, Home,
} from 'lucide-react'
import { cn } from '@/lib/utils'

// ─── Tree algorithms ──────────────────────────────────────────────────────────

/** Returns the path from root down to the target node, or [] if not found. */
function findPath(nodes: CategoryTreeNode[], targetId: string): CategoryTreeNode[] {
  for (const node of nodes) {
    if (node.id === targetId) return [node]
    const childPath = findPath(node.subCategories, targetId)
    if (childPath.length > 0) return [node, ...childPath]
  }
  return []
}

/** Returns the first matching node anywhere in the tree. */
function findNode(nodes: CategoryTreeNode[], id: string): CategoryTreeNode | null {
  for (const node of nodes) {
    if (node.id === id) return node
    const found = findNode(node.subCategories, id)
    if (found) return found
  }
  return null
}

/** Flatten entire tree to a 1-D array. */
function flattenTree(nodes: CategoryTreeNode[]): CategoryTreeNode[] {
  return nodes.flatMap(n => [n, ...flattenTree(n.subCategories)])
}

/** Filter tree by name query; keeps nodes whose descendants also match. */
function filterTree(nodes: CategoryTreeNode[], q: string): CategoryTreeNode[] {
  if (!q.trim()) return nodes
  const lower = q.toLowerCase()
  return nodes.reduce<CategoryTreeNode[]>((acc, n) => {
    const filteredChildren = filterTree(n.subCategories, q)
    if (n.name.toLowerCase().includes(lower) || filteredChildren.length > 0) {
      acc.push({ ...n, subCategories: filteredChildren })
    }
    return acc
  }, [])
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type FormMode = 'create' | 'edit'

export default function CategoriesPage() {
  const { success, error: showError } = useToast()

  // ── Tree (from optimised tree API) ────────────────────────────────────────
  const [tree, setTree] = useState<CategoryTreeNode[]>([])
  const [treeLoading, setTreeLoading] = useState(true)
  const [includeInactive, setIncludeInactive] = useState(false)

  // ── Selection (selectedId drives breadcrumb + highlight) ──────────────────
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set())

  // ── Full details fetched on selection (for description/isActive/edit) ─────
  const [selectedDetail, setSelectedDetail] = useState<CategoryDtoV2 | null>(null)
  const [detailLoading, setDetailLoading] = useState(false)

  // ── Search ────────────────────────────────────────────────────────────────
  const [searchQuery, setSearchQuery] = useState('')

  // ── Forms ─────────────────────────────────────────────────────────────────
  const [showForm, setShowForm] = useState(false)
  const [formMode, setFormMode] = useState<FormMode>('create')
  const [formLoading, setFormLoading] = useState(false)
  const [formError, setFormError] = useState('')
  const [createForm, setCreateForm] = useState<CreateCategoryRequestV2>({ name: '' })
  const [editForm, setEditForm] = useState<UpdateCategoryRequest>({ name: '' })
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string; hasChildren: boolean } | null>(null)
  const [deleting, setDeleting] = useState(false)

  // ── Computed ──────────────────────────────────────────────────────────────

  /** Full path from root to selected node */
  const breadcrumbPath = useMemo(
    () => (selectedId ? findPath(tree, selectedId) : []),
    [tree, selectedId],
  )

  /** The selected leaf node from the tree (has productCount) */
  const selectedNode = useMemo(
    () => breadcrumbPath[breadcrumbPath.length - 1] ?? null,
    [breadcrumbPath],
  )

  /** IDs of all ancestors of the selected node (for lighter highlight) */
  const ancestorIds = useMemo(
    () => new Set(breadcrumbPath.slice(0, -1).map(n => n.id)),
    [breadcrumbPath],
  )

  const visibleTree = useMemo(
    () => filterTree(tree, searchQuery),
    [tree, searchQuery],
  )

  const totalCount = useMemo(() => flattenTree(tree).length, [tree])

  // ── Data loading ──────────────────────────────────────────────────────────

  const loadTree = useCallback(async () => {
    setTreeLoading(true)
    try {
      const data = await categoryApi.tree(includeInactive ? { includeInactive: true } : undefined)
      setTree(data)
    } catch {
      showError('Không thể tải danh mục')
    } finally {
      setTreeLoading(false)
    }
  }, [includeInactive, showError])

  useEffect(() => { loadTree() }, [loadTree])

  // Auto-expand all nodes when searching
  useEffect(() => {
    if (searchQuery.trim()) {
      setExpandedIds(new Set(flattenTree(tree).map(n => n.id)))
    }
  }, [searchQuery, tree])

  /** Select a node: expand ancestors, load full detail for right panel. */
  async function selectNode(id: string) {
    // Derive ancestors from current tree and expand them
    const path = findPath(tree, id)
    setSelectedId(id)
    setExpandedIds(prev => {
      const next = new Set(prev)
      path.slice(0, -1).forEach(n => next.add(n.id))
      return next
    })

    // Fetch full detail (description / isActive / displayOrder) for right panel
    setDetailLoading(true)
    setSelectedDetail(null)
    try {
      const detail = await categoryApi.getById(id)
      setSelectedDetail(detail)
    } catch {
      // Fall back to minimal display using tree node
    } finally {
      setDetailLoading(false)
    }
  }

  function toggleExpand(id: string) {
    setExpandedIds(prev => {
      const next = new Set(prev)
      next.has(id) ? next.delete(id) : next.add(id)
      return next
    })
  }

  function openCreate(parentCategoryId?: string) {
    setFormMode('create')
    setCreateForm({ name: '', description: '', parentCategoryId })
    setFormError('')
    setShowForm(true)
  }

  function openEdit(detail: CategoryDtoV2) {
    setFormMode('edit')
    setEditingId(detail.id)
    setEditForm({ name: detail.name, description: detail.description ?? '', isActive: detail.isActive, displayOrder: detail.displayOrder })
    setFormError('')
    setShowForm(true)
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault(); setFormError('')
    if (!createForm.name.trim()) { setFormError('Tên danh mục không được trống.'); return }
    setFormLoading(true)
    try {
      const created = await categoryApi.create({
        ...createForm,
        name: createForm.name.trim(),
        parentCategoryId: createForm.parentCategoryId || undefined,
      })
      success('Thêm danh mục thành công')
      setShowForm(false)
      await loadTree()
      // Auto-select the newly created node
      selectNode(created.id)
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
      await categoryApi.update(editingId, { ...editForm, name: editForm.name.trim() })
      success('Cập nhật danh mục thành công')
      setShowForm(false)
      await loadTree()
      if (selectedId === editingId) selectNode(editingId)
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
      await loadTree()
      if (selectedId === deleteTarget.id) {
        // Navigate to parent if exists
        const parentNode = breadcrumbPath[breadcrumbPath.length - 2]
        if (parentNode) {
          selectNode(parentNode.id)
        } else {
          setSelectedId(null)
          setSelectedDetail(null)
        }
      }
    } catch (err) {
      showError('Xoá thất bại', err instanceof Error ? err.message : undefined)
    } finally { setDeleting(false); setDeleteTarget(null) }
  }

  const depth = breadcrumbPath.length - 1

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className="space-y-5">

      {/* Page header */}
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">Danh mục</h1>
          <p className="text-sm text-gray-400 mt-0.5">{totalCount} danh mục</p>
        </div>
        <div className="flex items-center gap-2">
          <label className="flex items-center gap-2 text-sm text-gray-500 cursor-pointer select-none">
            <input type="checkbox" checked={includeInactive}
              onChange={e => setIncludeInactive(e.target.checked)}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
            Hiện danh mục ngưng
          </label>
          <Button size="sm" onClick={() => openCreate()} className="gap-1.5">
            <Plus size={15} /> Thêm danh mục
          </Button>
        </div>
      </div>

      {treeLoading ? <LoadingSpinner /> : (
        <div className="grid lg:grid-cols-10 gap-5 items-start">

          {/* ── LEFT: Tree (30%) ─────────────────────────────────────────── */}
          <div className="lg:col-span-3">
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">

              {/* Tree header */}
              <div className="px-4 py-3 border-b border-gray-100 bg-gray-50/50">
                <div className="flex items-center justify-between gap-2 mb-2.5">
                  <span className="text-sm font-semibold text-gray-700">Cây danh mục</span>
                  <div className="flex items-center gap-0.5">
                    <button
                      title="Mở rộng tất cả"
                      onClick={() => setExpandedIds(new Set(flattenTree(tree).map(n => n.id)))}
                      className="p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
                    >
                      <ChevronsUpDown size={13} />
                    </button>
                    <button
                      title="Thu gọn tất cả"
                      onClick={() => setExpandedIds(new Set())}
                      className="p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
                    >
                      <ChevronsDownUp size={13} />
                    </button>
                  </div>
                </div>
                {/* Search */}
                <div className="relative">
                  <Search size={13} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="text"
                    placeholder="Tìm danh mục..."
                    value={searchQuery}
                    onChange={e => setSearchQuery(e.target.value)}
                    className="w-full pl-8 pr-7 py-1.5 text-sm bg-white border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  {searchQuery && (
                    <button onClick={() => setSearchQuery('')}
                      className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-300 hover:text-gray-500">
                      <X size={12} />
                    </button>
                  )}
                </div>
              </div>

              {/* Tree body */}
              <div className="overflow-y-auto max-h-[calc(100vh-320px)]">
                {tree.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-12 text-gray-400">
                    <Tag size={28} className="mb-2 opacity-30" />
                    <p className="text-sm">Chưa có danh mục nào</p>
                    <button onClick={() => openCreate()}
                      className="mt-3 text-sm text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1">
                      <Plus size={13} /> Thêm danh mục đầu tiên
                    </button>
                  </div>
                ) : visibleTree.length === 0 ? (
                  <p className="text-center text-gray-400 py-8 text-sm">
                    Không tìm thấy &ldquo;{searchQuery}&rdquo;
                  </p>
                ) : (
                  <div className="py-1">
                    <TreeNodeList
                      nodes={visibleTree}
                      depth={0}
                      expandedIds={expandedIds}
                      selectedId={selectedId}
                      ancestorIds={ancestorIds}
                      searchQuery={searchQuery}
                      onSelect={n => selectNode(n.id)}
                      onToggle={toggleExpand}
                    />
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* ── RIGHT: Detail (70%) ──────────────────────────────────────── */}
          <div className="lg:col-span-7 space-y-4">

            {/* Sticky breadcrumb */}
            {breadcrumbPath.length > 0 && (
              <div className="sticky top-0 z-10 bg-white border border-gray-200 rounded-xl shadow-sm px-5 py-3">
                <Breadcrumb path={breadcrumbPath} onNavigate={n => selectNode(n.id)} />
              </div>
            )}

            {/* Detail content */}
            {detailLoading ? (
              <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-10 flex items-center justify-center">
                <LoadingSpinner />
              </div>
            ) : selectedNode ? (
              <div className="space-y-4">

                {/* Main info card */}
                <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-6">
                  <div className="flex items-start justify-between gap-4 mb-5">
                    <div className="flex items-center gap-3">
                      <div className={cn(
                        'w-12 h-12 rounded-xl flex items-center justify-center shrink-0',
                        selectedDetail?.isActive !== false ? 'bg-blue-50' : 'bg-gray-100',
                      )}>
                        <FolderOpen size={22} className={selectedDetail?.isActive !== false ? 'text-blue-500' : 'text-gray-400'} />
                      </div>
                      <div>
                        <div className="flex items-center gap-2 flex-wrap">
                          <h2 className="text-xl font-bold text-gray-800">{selectedNode.name}</h2>
                          {selectedDetail?.isActive === false && (
                            <span className="text-xs bg-gray-100 text-gray-500 px-2 py-0.5 rounded-full font-medium">
                              Ngưng hoạt động
                            </span>
                          )}
                        </div>
                        {/* Parent link */}
                        {breadcrumbPath.length > 1 && (
                          <button
                            onClick={() => selectNode(breadcrumbPath[breadcrumbPath.length - 2].id)}
                            className="mt-1 text-xs text-blue-500 hover:text-blue-700 flex items-center gap-1 font-medium"
                          >
                            <FolderOpen size={11} />
                            Thuộc: {breadcrumbPath[breadcrumbPath.length - 2].name}
                          </button>
                        )}
                      </div>
                    </div>

                    {/* Action buttons */}
                    <div className="flex items-center gap-2 shrink-0">
                      <button
                        onClick={() => openCreate(selectedNode.id)}
                        className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-blue-600 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors"
                      >
                        <Plus size={14} /> Thêm con
                      </button>
                      {selectedDetail && (
                        <>
                          <button
                            onClick={() => openEdit(selectedDetail)}
                            className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
                          >
                            <Pencil size={14} /> Sửa
                          </button>
                          <button
                            onClick={() => setDeleteTarget({ id: selectedNode.id, name: selectedNode.name, hasChildren: selectedNode.subCategories.length > 0 })}
                            className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-red-600 bg-red-50 hover:bg-red-100 rounded-lg transition-colors"
                          >
                            <Trash2 size={14} /> Xoá
                          </button>
                        </>
                      )}
                    </div>
                  </div>

                  {/* Description */}
                  {selectedDetail?.description ? (
                    <p className="text-sm text-gray-600 bg-gray-50 rounded-lg px-4 py-3 border-l-4 border-blue-200 mb-5">
                      {selectedDetail.description}
                    </p>
                  ) : (
                    <p className="text-sm text-gray-400 italic mb-5">Chưa có mô tả</p>
                  )}

                  {/* Stat chips */}
                  <div className="flex flex-wrap gap-3">
                    <StatChip
                      icon={<Package size={14} className="text-blue-400" />}
                      label="Sản phẩm"
                      value={selectedNode.productCount}
                      colorClass="bg-blue-50"
                    />
                    <StatChip
                      icon={<Folder size={14} className="text-amber-400" />}
                      label="Danh mục con"
                      value={selectedNode.subCategories.length}
                      colorClass="bg-amber-50"
                    />
                    <StatChip
                      icon={<Tag size={14} className="text-purple-400" />}
                      label="Cấp độ"
                      value={depth}
                      colorClass="bg-purple-50"
                    />
                    {selectedDetail && (
                      <StatChip
                        icon={<ChevronDown size={14} className="text-gray-400" />}
                        label="Thứ tự"
                        value={selectedDetail.displayOrder ?? 0}
                        colorClass="bg-gray-50"
                      />
                    )}
                  </div>
                </div>

                {/* Sub-categories */}
                {selectedNode.subCategories.length > 0 ? (
                  <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-5">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="text-sm font-semibold text-gray-700">
                        Danh mục con
                        <span className="ml-1.5 text-xs font-normal text-gray-400">
                          ({selectedNode.subCategories.length})
                        </span>
                      </h3>
                      <button
                        onClick={() => openCreate(selectedNode.id)}
                        className="text-sm text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1"
                      >
                        <Plus size={13} /> Thêm
                      </button>
                    </div>
                    <div className="grid sm:grid-cols-2 xl:grid-cols-3 gap-3">
                      {selectedNode.subCategories.map(sub => (
                        <SubCategoryCard
                          key={sub.id}
                          node={sub}
                          onClick={() => selectNode(sub.id)}
                        />
                      ))}
                    </div>
                  </div>
                ) : (
                  <div className="bg-white border border-dashed border-gray-200 rounded-xl p-6 text-center">
                    <Folder size={28} className="mx-auto text-gray-300 mb-2" />
                    <p className="text-sm text-gray-400 mb-3">Chưa có danh mục con</p>
                    <button
                      onClick={() => openCreate(selectedNode.id)}
                      className="inline-flex items-center gap-1.5 text-sm text-blue-600 hover:text-blue-800 font-medium"
                    >
                      <Plus size={14} /> Thêm danh mục con
                    </button>
                  </div>
                )}

              </div>
            ) : (
              /* Empty state */
              <div className="bg-white border border-dashed border-gray-200 rounded-xl p-16 flex flex-col items-center justify-center text-center">
                <div className="w-16 h-16 bg-gray-100 rounded-2xl flex items-center justify-center mb-4">
                  <FolderOpen size={28} className="opacity-40 text-gray-400" />
                </div>
                <p className="text-sm font-medium text-gray-500 mb-1">Chưa chọn danh mục</p>
                <p className="text-xs text-gray-400">Chọn một mục ở cây bên trái để xem chi tiết</p>
              </div>
            )}
          </div>

        </div>
      )}

      {/* Create / Edit modals */}
      <FormDialog
        open={showForm && formMode === 'create'}
        title={createForm.parentCategoryId ? 'Thêm danh mục con' : 'Thêm danh mục'}
        loading={formLoading} onSubmit={handleCreate} onCancel={() => setShowForm(false)}
      >
        <FormField label="Tên danh mục" required>
          <input className={inputClass} value={createForm.name} autoFocus
            onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))}
            placeholder="Nhập tên danh mục" />
        </FormField>
        <FormField label="Mô tả">
          <textarea className={inputClass} rows={2} value={createForm.description ?? ''}
            onChange={e => setCreateForm(f => ({ ...f, description: e.target.value || undefined }))} />
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      <FormDialog open={showForm && formMode === 'edit'} title="Sửa danh mục"
        loading={formLoading} onSubmit={handleEdit} onCancel={() => setShowForm(false)}>
        <FormField label="Tên danh mục" required>
          <input className={inputClass} value={editForm.name} autoFocus
            onChange={e => setEditForm(f => ({ ...f, name: e.target.value }))} />
        </FormField>
        <FormField label="Mô tả">
          <textarea className={inputClass} rows={2} value={editForm.description ?? ''}
            onChange={e => setEditForm(f => ({ ...f, description: e.target.value || undefined }))} />
        </FormField>
        <FormField label="Thứ tự hiển thị">
          <input type="number" min={0} className={inputClass} value={editForm.displayOrder ?? 0}
            onChange={e => setEditForm(f => ({ ...f, displayOrder: Number(e.target.value) }))} />
        </FormField>
        <FormField label="Trạng thái">
          <select className={selectClass} value={editForm.isActive ? '1' : '0'}
            onChange={e => setEditForm(f => ({ ...f, isActive: e.target.value === '1' }))}>
            <option value="1">Đang hoạt động</option>
            <option value="0">Ngưng hoạt động</option>
          </select>
        </FormField>
        <FormError message={formError} />
      </FormDialog>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Xoá danh mục?"
        description={
          deleteTarget?.hasChildren
            ? `Không thể xoá "${deleteTarget?.name}" vì còn danh mục con.`
            : `Xoá "${deleteTarget?.name}"? Thao tác này không thể hoàn tác.`
        }
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><Trash2 size={24} className="text-red-500" /></div>}
        confirmLabel={deleteTarget?.hasChildren ? undefined : 'Xoá'}
        cancelLabel={deleteTarget?.hasChildren ? 'Đóng' : 'Huỷ'}
        variant="danger" loading={deleting}
        onConfirm={deleteTarget?.hasChildren ? () => setDeleteTarget(null) : handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

// ─── Sub-components ───────────────────────────────────────────────────────────

// ── Breadcrumb ────────────────────────────────────────────────────────────────

interface BreadcrumbProps {
  path: CategoryTreeNode[]
  onNavigate: (node: CategoryTreeNode) => void
}

function Breadcrumb({ path, onNavigate }: BreadcrumbProps) {
  return (
    <nav className="flex items-center gap-1 flex-wrap text-sm">
      <span className="flex items-center gap-1 text-gray-400">
        <Home size={13} /> Danh mục
      </span>
      {path.map((node, i) => {
        const isLast = i === path.length - 1
        return (
          <span key={node.id} className="flex items-center gap-1">
            <ChevronRight size={13} className="text-gray-300 shrink-0" />
            {isLast ? (
              <span className="font-semibold text-gray-800">{node.name}</span>
            ) : (
              <button
                onClick={() => onNavigate(node)}
                className="text-blue-600 hover:text-blue-800 hover:underline font-medium transition-colors"
              >
                {node.name}
              </button>
            )}
          </span>
        )
      })}
    </nav>
  )
}

// ── StatChip ─────────────────────────────────────────────────────────────────

function StatChip({ icon, label, value, colorClass }: {
  icon: React.ReactNode
  label: string
  value: number
  colorClass: string
}) {
  return (
    <div className={cn('flex items-center gap-2.5 rounded-xl px-4 py-3', colorClass)}>
      {icon}
      <div>
        <p className="text-xs text-gray-400 font-medium leading-none">{label}</p>
        <p className="text-xl font-bold text-gray-800 leading-none mt-1">{value}</p>
      </div>
    </div>
  )
}

// ── SubCategoryCard ───────────────────────────────────────────────────────────

function SubCategoryCard({ node, onClick }: { node: CategoryTreeNode; onClick: () => void }) {
  return (
    <button
      onClick={onClick}
      className="group flex items-center gap-3 p-3.5 rounded-xl border border-gray-200 text-left hover:border-blue-300 hover:bg-blue-50/40 hover:shadow-sm transition-all"
    >
      <div className="w-9 h-9 rounded-lg bg-amber-50 group-hover:bg-amber-100 flex items-center justify-center shrink-0 transition-colors">
        <Folder size={16} className="text-amber-500" />
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-sm font-medium text-gray-800 truncate">{node.name}</p>
        <div className="flex items-center gap-3 mt-0.5">
          {node.subCategories.length > 0 && (
            <span className="text-xs text-gray-400">{node.subCategories.length} con</span>
          )}
          {node.productCount > 0 && (
            <span className="text-xs text-gray-400 flex items-center gap-0.5">
              <Package size={10} /> {node.productCount}
            </span>
          )}
        </div>
      </div>
      <ChevronRight size={14} className="text-gray-300 group-hover:text-blue-400 shrink-0 transition-colors" />
    </button>
  )
}

// ── TreeNodeList ──────────────────────────────────────────────────────────────

interface TreeNodeListProps {
  nodes: CategoryTreeNode[]
  depth: number
  expandedIds: Set<string>
  selectedId: string | null
  ancestorIds: Set<string>
  searchQuery: string
  onSelect: (node: CategoryTreeNode) => void
  onToggle: (id: string) => void
}

function highlightMatch(text: string, query: string) {
  if (!query.trim()) return <>{text}</>
  const idx = text.toLowerCase().indexOf(query.toLowerCase())
  if (idx === -1) return <>{text}</>
  return (
    <>
      {text.slice(0, idx)}
      <mark className="bg-yellow-100 text-yellow-800 rounded-sm px-0.5">{text.slice(idx, idx + query.length)}</mark>
      {text.slice(idx + query.length)}
    </>
  )
}

function TreeNodeList({ nodes, depth, expandedIds, selectedId, ancestorIds, searchQuery, onSelect, onToggle }: TreeNodeListProps) {
  return (
    <>
      {nodes.map(node => {
        const hasChildren = node.subCategories.length > 0
        const isExpanded = expandedIds.has(node.id)
        const isSelected = node.id === selectedId
        const isAncestor = ancestorIds.has(node.id)

        return (
          <div key={node.id}>
            <div
              className={cn(
                'flex items-center gap-1 py-1.5 pr-2 cursor-pointer select-none transition-colors',
                isSelected
                  ? 'bg-blue-50 border-r-[3px] border-blue-500'
                  : isAncestor
                  ? 'bg-blue-50/40 border-r-[3px] border-blue-200'
                  : 'hover:bg-gray-50 border-r-[3px] border-transparent',
              )}
              style={{ paddingLeft: `${10 + depth * 16}px` }}
            >
              {/* Expand / collapse toggle */}
              <button
                onClick={e => { e.stopPropagation(); hasChildren && onToggle(node.id) }}
                className={cn(
                  'shrink-0 w-5 h-5 flex items-center justify-center rounded transition-colors',
                  hasChildren ? 'text-gray-400 hover:text-gray-700 hover:bg-gray-200' : 'cursor-default',
                )}
              >
                {hasChildren
                  ? isExpanded ? <ChevronDown size={12} /> : <ChevronRight size={12} />
                  : <span className="w-1 h-1 rounded-full bg-gray-200 block" />
                }
              </button>

              {/* Folder icon + name */}
              <button onClick={() => onSelect(node)} className="flex-1 flex items-center gap-2 text-left min-w-0 py-0.5">
                {hasChildren
                  ? <FolderOpen size={14} className={cn('shrink-0 transition-colors', isSelected || isAncestor ? 'text-blue-500' : 'text-amber-400')} />
                  : <Folder size={14} className={cn('shrink-0', isSelected ? 'text-blue-400' : 'text-gray-300')} />
                }
                <span className={cn(
                  'text-sm truncate transition-colors',
                  isSelected ? 'font-semibold text-blue-700' : isAncestor ? 'font-medium text-blue-600' : 'text-gray-700',
                )}>
                  {highlightMatch(node.name, searchQuery)}
                </span>
                {/* Product count badge */}
                {node.productCount > 0 && (
                  <span className={cn(
                    'shrink-0 text-xs px-1.5 py-0.5 rounded-full font-medium ml-0.5',
                    isSelected ? 'bg-blue-100 text-blue-600' : 'bg-gray-100 text-gray-400',
                  )}>
                    {node.productCount}
                  </span>
                )}
              </button>

            </div>

            {/* Children with vertical guide line */}
            {hasChildren && isExpanded && (
              <div className="relative">
                <div className="absolute w-px bg-gray-200 top-1 bottom-2 pointer-events-none"
                  style={{ left: `${10 + depth * 16 + 12}px` }} />
                <TreeNodeList
                  nodes={node.subCategories} depth={depth + 1}
                  expandedIds={expandedIds} selectedId={selectedId}
                  ancestorIds={ancestorIds} searchQuery={searchQuery}
                  onSelect={onSelect} onToggle={onToggle}
                />
              </div>
            )}
          </div>
        )
      })}
    </>
  )
}
