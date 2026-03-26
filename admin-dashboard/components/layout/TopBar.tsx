'use client'

import { NotificationBell } from '@/components/layout/NotificationBell'

export function TopBar() {
  return (
    <div className="hidden md:flex items-center justify-end bg-white border-b border-gray-200 px-6 py-2.5 shrink-0">
      <NotificationBell />
    </div>
  )
}
