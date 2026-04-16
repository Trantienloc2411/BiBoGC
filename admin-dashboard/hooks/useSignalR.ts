'use client'

import { useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import { NotificationDto } from '@/types'

const HUB_URL = `${process.env.NEXT_PUBLIC_API_URL ?? ''}/hubs/notifications`
const ROLE_GROUP = 'Admin'

function getAccessToken(): string | null {
  if (typeof document === 'undefined') return null
  const match = document.cookie.match(/(?:^|;\s*)accessToken=([^;]+)/)
  return match ? decodeURIComponent(match[1]) : null
}

/**
 * Establishes a SignalR connection to /hubs/notifications and
 * calls `onNotification` for every ReceiveNotification event.
 *
 * Returns a cleanup function that stops the connection.
 */
export function useSignalR(onNotification: (n: NotificationDto) => void) {
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const onNotificationRef = useRef(onNotification)
  onNotificationRef.current = onNotification

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => getAccessToken() ?? '',
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    connection.on('ReceiveNotification', (notification: NotificationDto) => {
      onNotificationRef.current(notification)
    })

    connection.onreconnected(async () => {
      try {
        await connection.invoke('JoinRoleGroup', ROLE_GROUP)
      } catch {
        // ignore
      }
    })

    connectionRef.current = connection

    connection
      .start()
      .then(() => connection.invoke('JoinRoleGroup', ROLE_GROUP))
      .catch(() => {
        // Connection failed — useNotifications will fall back to polling
      })

    return () => {
      connection.stop()
      connectionRef.current = null
    }
  }, []) // intentionally empty: connection lives for the component lifetime
}
