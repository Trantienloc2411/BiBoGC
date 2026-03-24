'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import { api } from '@/lib/api'
import { NotificationDto, NotificationPagedResult } from '@/types'

const ROLE = 'Admin'
const PAGE_SIZE = 20
const POLL_INTERVAL = 30_000

export function useNotifications() {
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const prevIdsRef = useRef<Set<string>>(new Set())
  const onNewRef = useRef<((n: NotificationDto) => void) | null>(null)

  const fetchNotifications = useCallback(async (silent = false) => {
    if (!silent) setLoading(true)
    try {
      const res = await api.get(
        `/api/notifications?role=${ROLE}&unreadOnly=false&page=1&pageSize=${PAGE_SIZE}`
      )
      if (!res.ok) return

      const json: NotificationPagedResult = await res.json()
      const items = json.items ?? []

      if (prevIdsRef.current.size > 0) {
        for (const item of items) {
          if (!prevIdsRef.current.has(item.id) && !item.isRead) {
            onNewRef.current?.(item)
          }
        }
      }

      prevIdsRef.current = new Set(items.map(i => i.id))
      setNotifications(items)
      setUnreadCount(items.filter(n => !n.isRead).length)
    } catch {
      // toast already handles API errors
    } finally {
      if (!silent) setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchNotifications()
    const id = setInterval(() => fetchNotifications(true), POLL_INTERVAL)
    return () => clearInterval(id)
  }, [fetchNotifications])

  const markAsRead = useCallback(async (id: string) => {
    setNotifications(prev =>
      prev.map(n => n.id === id ? { ...n, isRead: true } : n)
    )
    setUnreadCount(prev => Math.max(0, prev - 1))

    try {
      await api.put(`/api/notifications/${id}/read`)
    } catch {
      // revert on failure
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
