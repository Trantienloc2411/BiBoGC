'use client'

import {
  createContext, useContext, useCallback, useState, useEffect, useRef,
} from 'react'
import { X, AlertCircle, CheckCircle2, Info } from 'lucide-react'
import { cn } from '@/lib/utils'
import { onApiError } from '@/lib/api'

type ToastType = 'error' | 'success' | 'info'

interface Toast {
  id: string
  type: ToastType
  title: string
  message?: string
  duration?: number
}

interface ToastContextValue {
  toast: (t: Omit<Toast, 'id'>) => void
  error: (title: string, message?: string) => void
  success: (title: string, message?: string) => void
  info: (title: string, message?: string) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx
}

let counter = 0

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const addToast = useCallback((t: Omit<Toast, 'id'>) => {
    const id = `toast-${++counter}`
    setToasts(prev => [...prev, { ...t, id }])
  }, [])

  const removeToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(t => t.id !== id))
  }, [])

  useEffect(() => {
    return onApiError((err) => {
      const title = `Lỗi server (${err.status})`
      const message = err.message ?? `${err.statusText} — ${err.path}`
      addToast({ type: 'error', title, message, duration: 6000 })
    })
  }, [addToast])

  const value: ToastContextValue = {
    toast: addToast,
    error: (title, message) => addToast({ type: 'error', title, message, duration: 6000 }),
    success: (title, message) => addToast({ type: 'success', title, message, duration: 4000 }),
    info: (title, message) => addToast({ type: 'info', title, message, duration: 4000 }),
  }

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="fixed bottom-6 right-6 z-[100] flex flex-col-reverse gap-2.5 w-96 pointer-events-none">
        {toasts.map(t => (
          <ToastItem key={t.id} toast={t} onDismiss={removeToast} />
        ))}
      </div>
    </ToastContext.Provider>
  )
}

const ICONS = {
  error:   <AlertCircle size={20} className="text-red-500 shrink-0" />,
  success: <CheckCircle2 size={20} className="text-emerald-500 shrink-0" />,
  info:    <Info size={20} className="text-blue-500 shrink-0" />,
}

const BORDER_COLORS = {
  error:   'border-l-red-500',
  success: 'border-l-emerald-500',
  info:    'border-l-blue-500',
}

function ToastItem({ toast, onDismiss }: { toast: Toast; onDismiss: (id: string) => void }) {
  const [visible, setVisible] = useState(false)
  const timerRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined)

  useEffect(() => {
    requestAnimationFrame(() => setVisible(true))

    const duration = toast.duration ?? 5000
    timerRef.current = setTimeout(() => {
      setVisible(false)
      setTimeout(() => onDismiss(toast.id), 300)
    }, duration)

    return () => clearTimeout(timerRef.current)
  }, [toast, onDismiss])

  function handleClose() {
    clearTimeout(timerRef.current)
    setVisible(false)
    setTimeout(() => onDismiss(toast.id), 300)
  }

  return (
    <div
      className={cn(
        'pointer-events-auto bg-white rounded-lg shadow-lg border border-gray-200 border-l-4 px-4 py-3.5 flex gap-3 items-start transition-all duration-300',
        BORDER_COLORS[toast.type],
        visible ? 'translate-y-0 opacity-100' : 'translate-y-4 opacity-0'
      )}
    >
      {ICONS[toast.type]}
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-gray-800">{toast.title}</p>
        {toast.message && (
          <p className="text-sm text-gray-500 mt-0.5 break-words">{toast.message}</p>
        )}
      </div>
      <button
        onClick={handleClose}
        className="text-gray-400 hover:text-gray-600 shrink-0 p-0.5"
      >
        <X size={16} />
      </button>
    </div>
  )
}
