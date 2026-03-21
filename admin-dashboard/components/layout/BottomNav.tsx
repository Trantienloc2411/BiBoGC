'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { LayoutDashboard, Receipt, BarChart2, ShieldCheck } from 'lucide-react'
import { cn } from '@/lib/utils'

const NAV_ITEMS = [
  { href: '/dashboard',  label: 'Dashboard', icon: LayoutDashboard },
  { href: '/expenses',   label: 'Chi phí',   icon: Receipt },
  { href: '/reports',    label: 'Báo cáo',   icon: BarChart2 },
  { href: '/auditlogs',  label: 'Audit',     icon: ShieldCheck },
]

export function BottomNav() {
  const pathname = usePathname()

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 flex z-10">
      {NAV_ITEMS.map(({ href, label, icon: Icon }) => {
        const active = pathname === href
        return (
          <Link
            key={href}
            href={href}
            className={cn(
              'flex-1 flex flex-col items-center gap-1 py-3 text-xs font-medium transition-colors',
              active ? 'text-blue-600' : 'text-gray-500 hover:text-gray-800'
            )}
          >
            <Icon size={22} strokeWidth={active ? 2.5 : 1.8} />
            {label}
          </Link>
        )
      })}
    </nav>
  )
}
