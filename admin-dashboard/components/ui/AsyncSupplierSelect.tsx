'use client'

import { useState, useRef, useEffect, useCallback } from 'react'
import { supplierApi } from '@/lib/api'
import type { SupplierDtoV2 } from '@/types'
import { Search, X, ChevronDown, Building2, Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

export interface AsyncSupplierSelectProps {
  value: string
  onChange: (id: string) => void
  placeholder?: string
  className?: string
}

export function AsyncSupplierSelect({
  value,
  onChange,
  placeholder = 'Tìm nhà cung cấp...',
  className,
}: AsyncSupplierSelectProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<SupplierDtoV2[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedName, setSelectedName] = useState('')
  const [highlightedIndex, setHighlightedIndex] = useState(-1)

  const containerRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // When value is cleared externally, reset internal display name
  useEffect(() => {
    if (!value) setSelectedName('')
  }, [value])

  // Close when clicking outside
  useEffect(() => {
    function onMouseDown(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false)
        setQuery('')
        setHighlightedIndex(-1)
      }
    }
    document.addEventListener('mousedown', onMouseDown)
    return () => document.removeEventListener('mousedown', onMouseDown)
  }, [])

  const fetchResults = useCallback((q: string) => {
    if (timerRef.current) clearTimeout(timerRef.current)
    setLoading(true)
    timerRef.current = setTimeout(async () => {
      try {
        const data = await supplierApi.list({ searchTerm: q || undefined, pageSize: 10 })
        setResults(data.items ?? [])
      } catch {
        setResults([])
      } finally {
        setLoading(false)
      }
    }, 400)
  }, [])

  function open() {
    setIsOpen(true)
    fetchResults(query)
    setTimeout(() => inputRef.current?.focus(), 0)
  }

  function handleQueryChange(q: string) {
    setQuery(q)
    setHighlightedIndex(-1)
    fetchResults(q)
  }

  function select(supplier: SupplierDtoV2) {
    onChange(supplier.id)
    setSelectedName(supplier.name)
    setIsOpen(false)
    setQuery('')
    setHighlightedIndex(-1)
  }

  function clear(e: React.MouseEvent) {
    e.stopPropagation()
    onChange('')
    setSelectedName('')
    setQuery('')
    setResults([])
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (!isOpen) return
    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault()
        setHighlightedIndex(i => Math.min(i + 1, results.length - 1))
        break
      case 'ArrowUp':
        e.preventDefault()
        setHighlightedIndex(i => Math.max(i - 1, -1))
        break
      case 'Enter':
        e.preventDefault()
        if (highlightedIndex >= 0 && results[highlightedIndex]) {
          select(results[highlightedIndex])
        }
        break
      case 'Escape':
        setIsOpen(false)
        setQuery('')
        setHighlightedIndex(-1)
        break
    }
  }

  const displayLabel = value ? (selectedName || 'Đang tải...') : ''

  return (
    <div ref={containerRef} className={cn('relative', className)}>
      {/* Trigger button */}
      <div
        onClick={open}
        className={cn(
          'flex items-center gap-2 w-full text-sm border rounded-md px-3 py-2 cursor-pointer bg-white transition-all select-none',
          isOpen
            ? 'ring-1 ring-blue-400 border-blue-400'
            : 'border-gray-200 hover:border-gray-300',
        )}
      >
        {isOpen ? (
          <>
            <Search size={14} className="text-gray-400 shrink-0" />
            <input
              ref={inputRef}
              value={query}
              onChange={e => handleQueryChange(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder={placeholder}
              className="flex-1 outline-none text-gray-700 text-sm bg-transparent min-w-0"
              autoComplete="off"
            />
          </>
        ) : (
          <>
            <Search size={14} className="text-gray-400 shrink-0" />
            <span className={cn('flex-1 truncate text-sm', displayLabel ? 'text-gray-800' : 'text-gray-400')}>
              {displayLabel || placeholder}
            </span>
            {value ? (
              <button
                type="button"
                onClick={clear}
                className="shrink-0 text-gray-400 hover:text-gray-600 transition-colors p-0.5 rounded"
                tabIndex={-1}
                aria-label="Xoá lọc NCC"
              >
                <X size={13} />
              </button>
            ) : (
              <ChevronDown size={14} className="text-gray-400 shrink-0" />
            )}
          </>
        )}
      </div>

      {/* Dropdown */}
      {isOpen && (
        <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-md shadow-lg overflow-hidden">
          <div className="max-h-56 overflow-y-auto">
            {loading ? (
              <div className="flex items-center justify-center gap-2 py-5 text-sm text-gray-400">
                <Loader2 size={14} className="animate-spin" />
                Đang tìm...
              </div>
            ) : results.length === 0 ? (
              <div className="py-5 text-sm text-gray-400 text-center">
                {query ? `Không tìm thấy "${query}"` : 'Nhập tên để tìm kiếm'}
              </div>
            ) : (
              results.map((s, i) => (
                <button
                  key={s.id}
                  type="button"
                  onMouseDown={() => select(s)}
                  onMouseEnter={() => setHighlightedIndex(i)}
                  className={cn(
                    'w-full text-left px-3 py-2.5 text-sm flex items-center gap-2.5 transition-colors',
                    i === highlightedIndex ? 'bg-blue-50 text-blue-700' : 'hover:bg-gray-50 text-gray-700',
                  )}
                >
                  <Building2 size={13} className={cn('shrink-0', i === highlightedIndex ? 'text-blue-500' : 'text-gray-400')} />
                  <div className="min-w-0">
                    <p className="font-medium truncate">{s.name}</p>
                    {(s.contactName || s.contactPhone) && (
                      <p className="text-xs text-gray-400 mt-0.5 truncate">
                        {[s.contactName, s.contactPhone].filter(Boolean).join(' · ')}
                      </p>
                    )}
                  </div>
                </button>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  )
}
