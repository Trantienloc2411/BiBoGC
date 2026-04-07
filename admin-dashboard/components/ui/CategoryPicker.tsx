'use client'

import { useEffect, useState } from 'react'
import { categoryApi } from '@/lib/api'
import type { CategoryDtoV2 } from '@/types'
import { ChevronRight, Folder, FolderOpen, X, Loader2, Home } from 'lucide-react'
import { cn } from '@/lib/utils'

interface CategoryPickerProps {
  value: string | null
  onChange: (id: string | null) => void
}

export function CategoryPicker({ value, onChange }: CategoryPickerProps) {
  // Stack of levels: levels[0] = root categories, levels[1] = children of selected root, …
  const [levels, setLevels] = useState<CategoryDtoV2[][]>([])
  // Breadcrumb path of clicked-into categories (not yet the final leaf selection)
  const [path, setPath] = useState<CategoryDtoV2[]>([])
  // The category that was ultimately selected (leaf or explicit pick)
  const [selected, setSelected] = useState<CategoryDtoV2 | null>(null)
  const [loadingId, setLoadingId] = useState<string | null>(null)
  const [rootLoading, setRootLoading] = useState(false)
  const [open, setOpen] = useState(false)

  // Load root categories on first open
  useEffect(() => {
    if (!open || levels.length > 0) return
    setRootLoading(true)
    categoryApi.list({ includeInactive: false })
      .then(data => setLevels([data.items ?? []]))
      .catch(() => {})
      .finally(() => setRootLoading(false))
  }, [open, levels.length])

  // Sync selected label when value is cleared externally
  useEffect(() => {
    if (value === null) setSelected(null)
  }, [value])

  function handleOpen() {
    // Reset drill-down to root each time picker is opened
    setLevels(prev => prev.length ? [prev[0]] : [])
    setPath([])
    setOpen(true)
  }

  function handleClear(e: React.MouseEvent) {
    e.stopPropagation()
    setSelected(null)
    onChange(null)
  }

  async function handleClick(cat: CategoryDtoV2, levelIndex: number) {
    setLoadingId(cat.id)
    try {
      // Always fetch to get latest children
      const detail = await categoryApi.getById(cat.id)
      const newPath = [...path.slice(0, levelIndex), cat]
      setPath(newPath)

      if ((detail.subCategories?.length ?? 0) > 0) {
        // Has children — drill into next level, trim any deeper levels
        setLevels(prev => [...prev.slice(0, levelIndex + 1), detail.subCategories])
      } else {
        // Leaf node — select it and close
        setSelected(cat)
        onChange(cat.id)
        setOpen(false)
      }
    } catch {
      // Fallback: treat as leaf if fetch fails
      setSelected(cat)
      onChange(cat.id)
      setOpen(false)
    } finally {
      setLoadingId(null)
    }
  }

  function handleBreadcrumb(index: number) {
    // index === -1 means "Home" (root)
    if (index === -1) {
      setPath([])
      setLevels(prev => prev.slice(0, 1))
    } else {
      setPath(prev => prev.slice(0, index + 1))
      setLevels(prev => prev.slice(0, index + 2))
    }
  }

  // Which level to display: the deepest one
  const currentLevel = levels[levels.length - 1] ?? []
  const currentLevelIndex = levels.length - 1

  return (
    <div>
      {/* Trigger */}
      <button
        type="button"
        onClick={handleOpen}
        className={cn(
          'w-full flex items-center justify-between px-3.5 py-2.5 rounded-md border text-sm transition-colors text-left',
          open
            ? 'border-blue-500 ring-2 ring-blue-500'
            : 'border-gray-300 hover:border-gray-400',
          selected ? 'text-gray-800' : 'text-gray-400',
        )}
      >
        <span className="flex items-center gap-2 truncate">
          {selected ? (
            <>
              <Folder size={15} className="text-blue-500 shrink-0" />
              <span className="truncate">{selected.name}</span>
            </>
          ) : 'Chọn danh mục...'}
        </span>
        {selected && (
          <span
            role="button"
            onClick={handleClear}
            className="ml-2 text-gray-400 hover:text-gray-600 shrink-0 cursor-pointer"
          >
            <X size={14} />
          </span>
        )}
      </button>

      {/* Drill-down panel */}
      {open && (
        <div className="mt-1.5 border border-gray-200 rounded-md bg-white shadow-sm overflow-hidden">
          {/* Breadcrumb */}
          <div className="flex items-center gap-1 px-3 py-2 border-b border-gray-100 bg-gray-50 overflow-x-auto flex-nowrap min-w-0">
            <button
              type="button"
              onClick={() => handleBreadcrumb(-1)}
              className="text-blue-600 hover:text-blue-800 shrink-0"
            >
              <Home size={13} />
            </button>
            {path.map((p, i) => (
              <span key={p.id} className="flex items-center gap-1 shrink-0 min-w-0">
                <ChevronRight size={12} className="text-gray-400" />
                <button
                  type="button"
                  onClick={() => handleBreadcrumb(i)}
                  className={cn(
                    'text-xs truncate max-w-[120px]',
                    i === path.length - 1 ? 'text-gray-700 font-medium' : 'text-blue-600 hover:text-blue-800',
                  )}
                >
                  {p.name}
                </button>
              </span>
            ))}
          </div>

          {/* Category list */}
          <div className="max-h-52 overflow-y-auto">
            {rootLoading ? (
              <div className="flex items-center justify-center py-6 text-gray-400">
                <Loader2 size={18} className="animate-spin" />
              </div>
            ) : currentLevel.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-5">Không có danh mục</p>
            ) : (
              currentLevel
                .filter(c => c.isActive)
                .map(cat => {
                  const isInPath = path[currentLevelIndex]?.id === cat.id
                  const isLoading = loadingId === cat.id
                  const hasChildren = (cat.subCategories?.length ?? 0) > 0

                  return (
                    <button
                      key={cat.id}
                      type="button"
                      disabled={isLoading}
                      onClick={() => handleClick(cat, currentLevelIndex)}
                      className={cn(
                        'w-full flex items-center gap-2.5 px-3 py-2.5 text-sm text-left transition-colors',
                        isInPath
                          ? 'bg-blue-50 text-blue-700 font-medium'
                          : 'text-gray-700 hover:bg-gray-50',
                      )}
                    >
                      {isLoading ? (
                        <Loader2 size={15} className="text-blue-500 shrink-0 animate-spin" />
                      ) : hasChildren ? (
                        <FolderOpen size={15} className={cn('shrink-0', isInPath ? 'text-blue-500' : 'text-amber-500')} />
                      ) : (
                        <Folder size={15} className={cn('shrink-0', isInPath ? 'text-blue-500' : 'text-gray-400')} />
                      )}
                      <span className="flex-1 truncate">{cat.name}</span>
                      {hasChildren && !isLoading && (
                        <ChevronRight size={14} className="text-gray-400 shrink-0" />
                      )}
                    </button>
                  )
                })
            )}
          </div>

          {/* Dismiss */}
          <div className="border-t border-gray-100 px-3 py-2 flex justify-end">
            <button
              type="button"
              onClick={() => setOpen(false)}
              className="text-xs text-gray-500 hover:text-gray-700"
            >
              Đóng
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
