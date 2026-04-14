'use client'

import { X } from 'lucide-react'
import { Button } from '@/components/ui/Button'

interface FormDialogProps {
  open: boolean
  title: string
  loading?: boolean
  submitLabel?: string
  onSubmit: (e: React.FormEvent) => void
  onCancel: () => void
  children: React.ReactNode
}

export function FormDialog({ open, title, loading, submitLabel = 'Lưu lại', onSubmit, onCancel, children }: FormDialogProps) {
  if (!open) return null

  return (
    <div className="fixed inset-0 bg-black/40 z-50 flex items-end sm:items-center justify-center p-4">
      <div className="bg-white w-full max-w-md rounded-lg shadow-xl max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100 shrink-0">
          <h2 className="font-semibold text-gray-800 text-lg">{title}</h2>
          <button onClick={onCancel} className="p-1.5 hover:bg-gray-100 rounded-md transition-colors">
            <X size={18} />
          </button>
        </div>
        <form onSubmit={onSubmit} className="p-5 space-y-4 overflow-y-auto">
          {children}
          <div className="flex gap-3 pt-1">
            <Button type="button" variant="secondary" size="md" className="flex-1" onClick={onCancel}>
              Huỷ
            </Button>
            <Button type="submit" size="md" className="flex-1" loading={loading}>
              {submitLabel}
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}

export function FormField({ label, required, children }: { label: string; required?: boolean; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1.5">
        {label}{required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      {children}
    </div>
  )
}

export function FormError({ message }: { message: string }) {
  if (!message) return null
  const lines = message.split('\n').filter(Boolean)
  return (
    <div className="bg-red-50 border border-red-200 rounded-md px-4 py-3 text-sm text-red-600">
      {lines.length > 1 ? (
        <ul className="list-disc list-inside space-y-0.5">
          {lines.map((l, i) => <li key={i}>{l}</li>)}
        </ul>
      ) : (
        message
      )}
    </div>
  )
}

export const inputClass = 'w-full px-3.5 py-2.5 rounded-md border border-gray-300 text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500'
export const selectClass = 'w-full px-3.5 py-2.5 rounded-md border border-gray-300 text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500'
