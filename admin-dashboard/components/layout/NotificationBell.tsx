'use client'

import { useState, useEffect, useRef } from 'react'
import { Bell, CheckCheck, AlertCircle, AlertTriangle, Info } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useNotifications } from '@/hooks/useNotifications'
import { useToast } from '@/components/ui/Toast'
import { NotificationDto } from '@/types'

function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const mins = Math.floor(diff / 60_000)
  if (mins < 1) return 'Vừa xong'
  if (mins < 60) return `${mins} phút trước`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `${hours} giờ trước`
  const days = Math.floor(hours / 24)
  return `${days} ngày trước`
}

const TYPE_ICON = {
  info:    <Info size={16} className="text-blue-500 shrink-0 mt-0.5" />,
  warning: <AlertTriangle size={16} className="text-amber-500 shrink-0 mt-0.5" />,
  error:   <AlertCircle size={16} className="text-red-500 shrink-0 mt-0.5" />,
}

const TYPE_DOT = {
  info:    'bg-blue-500',
  warning: 'bg-amber-500',
  error:   'bg-red-500',
}

export function NotificationBell() {
  const { notifications, unreadCount, loading, markAsRead, markAllAsRead, onNewNotification } = useNotifications()
  const [open, setOpen] = useState(false)
  const panelRef = useRef<HTMLDivElement>(null)
  const toast = useToast()

  useEffect(() => {
    onNewNotification((n: NotificationDto) => {
      const toastType = n.type === 'error' ? 'error' : n.type === 'warning' ? 'info' : 'info'
      toast[toastType](n.title, n.message)
    })
  }, [toast, onNewNotification])

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    if (open) {
      document.addEventListener('mousedown', handleClickOutside)
      return () => document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [open])

  function handleItemClick(n: NotificationDto) {
    if (!n.isRead) markAsRead(n.id)
  }

  return (
    <div className="relative" ref={panelRef}>
      <button
        onClick={() => setOpen(v => !v)}
        className="relative p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
        aria-label="Thông báo"
      >
        <Bell size={20} strokeWidth={1.8} />
        {unreadCount > 0 && (
          <span className="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] flex items-center justify-center bg-red-500 text-white text-[10px] font-bold rounded-full px-1">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-full mt-2 w-80 sm:w-96 bg-white rounded-lg shadow-xl border border-gray-200 z-50 flex flex-col max-h-[28rem]">
          {/* Header */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
            <h3 className="text-sm font-semibold text-gray-800">Thông báo</h3>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                className="flex items-center gap-1.5 text-xs font-medium text-blue-600 hover:text-blue-700"
              >
                <CheckCheck size={14} />
                Đọc tất cả
              </button>
            )}
          </div>

          {/* List */}
          <div className="overflow-y-auto flex-1">
            {loading && notifications.length === 0 ? (
              <div className="py-10 text-center text-sm text-gray-400">Đang tải...</div>
            ) : notifications.length === 0 ? (
              <div className="py-10 text-center">
                <Bell size={32} className="mx-auto text-gray-300 mb-2" />
                <p className="text-sm text-gray-400">Không có thông báo</p>
              </div>
            ) : (
              <ul>
                {notifications.map(n => (
                  <li key={n.id}>
                    <button
                      onClick={() => handleItemClick(n)}
                      className={cn(
                        'w-full text-left px-4 py-3 flex gap-3 hover:bg-gray-50 transition-colors border-b border-gray-50',
                        !n.isRead && 'bg-blue-50/40'
                      )}
                    >
                      {TYPE_ICON[n.type]}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <p className={cn(
                            'text-sm truncate',
                            n.isRead ? 'text-gray-600' : 'text-gray-800 font-semibold'
                          )}>
                            {n.title}
                          </p>
                          {!n.isRead && (
                            <span className={cn('w-2 h-2 rounded-full shrink-0', TYPE_DOT[n.type])} />
                          )}
                        </div>
                        <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{n.message}</p>
                        <p className="text-[11px] text-gray-400 mt-1">{timeAgo(n.createdAt)}</p>
                      </div>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
