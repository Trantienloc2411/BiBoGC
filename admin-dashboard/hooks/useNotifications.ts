'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import { api } from '@/lib/api'
import { NotificationDto, NotificationPagedResult } from '@/types'
import { useSignalR } from './useSignalR'

const ROLE = 'Admin'
const PAGE_SIZE = 20
/** Fallback polling interval — longer now that SignalR delivers real-time updates */
const POLL_INTERVAL = 120_000

export function useNotifications() {
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const onNewRef = useRef<((n: NotificationDto) => void) | null>(null)

  // ── REST fetch (initial load + background refresh) ──────────────────────────
  const fetchNotifications = useCallback(async (silent = false) => {
    if (!silent) setLoading(true)
    try {
      const res = await api.get(
        `/api/notifications?role=${ROLE}&unreadOnly=false&page=1&pageSize=${PAGE_SIZE}`
      )
      if (!res.ok) return
      const json: NotificationPagedResult = await res.json()
      const items = json.items ?? []
      setNotifications(items)
      setUnreadCount(items.filter(n => !n.isRead).length)
    } catch {
      // silently swallow — SignalR keeps the feed live
    } finally {
      if (!silent) setLoading(false)
    }
  }, [])

  // ── SignalR real-time handler ────────────────────────────────────────────────
  const handleRealTimeNotification = useCallback((n: NotificationDto) => {
    setNotifications(prev => {
      if (prev.some(p => p.id === n.id)) return prev
      return [n, ...prev]
    })
    if (!n.isRead) setUnreadCount(prev => prev + 1)
    onNewRef.current?.(n)
  }, [])

  useSignalR(handleRealTimeNotification)

  // ── Mount + background polling fallback ─────────────────────────────────────
  useEffect(() => {
    fetchNotifications()
    const id = setInterval(() => fetchNotifications(true), POLL_INTERVAL)
    return () => clearInterval(id)
  }, [fetchNotifications])

  // ── Actions ─────────────────────────────────────────────────────────────────
  const markAsRead = useCallback(async (id: string) => {
    setNotifications(prev =>
      prev.map(n => n.id === id ? { ...n, isRead: true } : n)
    )
    setUnreadCount(prev => Math.max(0, prev - 1))
    try {
      await api.put(`/api/notifications/${id}/read`)
    } catch {
      fetchNotifications(true)
    }
  }, [fetchNotifications])

  const markAllAsRead = useCallback(async () => {
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })))
    setUnreadCount(0)
    try {
      await api.put(`/api/notifications/read-all?role=${ROLE}`)
    } catch {
      fetchNotifications(true)
    }
  }, [fetchNotifications])

  return {
    notifications,
    unreadCount,
    loading,
    markAsRead,
    markAllAsRead,
    onNewNotification: (fn: (n: NotificationDto) => void) => { onNewRef.current = fn },
  }
}
