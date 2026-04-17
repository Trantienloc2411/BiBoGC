'use client'

import { useState } from 'react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import {
  LayoutDashboard, Receipt, BarChart2, ShieldCheck, Settings2,
  LogOut, Menu, X, ShoppingCart, FileText, ChevronDown,
  Package, FolderTree, Truck, ArrowLeftRight, Bell, PackagePlus,
} from 'lucide-react'
import Image from 'next/image'
import { cn } from '@/lib/utils'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { NotificationBell } from '@/components/layout/NotificationBell'

type IconType = React.ComponentType<{ size?: number; strokeWidth?: number; className?: string }>
type NavChild = { href: string; label: string; icon: IconType }
type NavItem = { href: string; label: string; icon: IconType; groupOnly?: boolean; children?: NavChild[] }

const NAV_ITEMS: NavItem[] = [
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  {
    href: '/orders', label: 'Đơn hàng', icon: ShoppingCart, groupOnly: true,
    children: [
      { href: '/orders', label: 'Đơn hàng', icon: ShoppingCart },
      { href: '/invoices', label: 'Hoá đơn', icon: FileText },
    ],
  },
  {
    href: '/inventory', label: 'Kho hàng', icon: Package, groupOnly: true,
    children: [
      { href: '/inventory/products', label: 'Sản phẩm', icon: Package },
      { href: '/inventory/categories', label: 'Danh mục', icon: FolderTree },
      { href: '/inventory/suppliers', label: 'Nhà cung cấp', icon: Truck },
      { href: '/inventory/import', label: 'Nhập hàng', icon: PackagePlus },
      { href: '/inventory/stock-transactions', label: 'Giao dịch kho', icon: ArrowLeftRight },
      { href: '/inventory/alerts', label: 'Cảnh báo', icon: Bell },
    ],
  },
  { href: '/expenses', label: 'Chi phí', icon: Receipt },
  { href: '/reports', label: 'Báo cáo', icon: BarChart2 },
  { href: '/tax-config', label: 'Thuế', icon: Settings2 },
  { href: '/auditlogs', label: 'Audit Log', icon: ShieldCheck },
]

function isGroupActive(item: NavItem, pathname: string) {
  return item.children?.some(c => pathname.startsWith(c.href)) ?? false
}

export function Sidebar() {
  const pathname = usePathname()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [showLogout, setShowLogout] = useState(false)
  const [loggingOut, setLoggingOut] = useState(false)

  const [openGroups, setOpenGroups] = useState<Record<string, boolean>>(() => {
    const init: Record<string, boolean> = {}
    NAV_ITEMS.filter(i => i.children).forEach(i => {
      init[i.href] = isGroupActive(i, pathname)
    })
    return init
  })

  function toggleGroup(href: string) {
    setOpenGroups(g => ({ ...g, [href]: !g[href] }))
  }

  async function handleLogout() {
    setLoggingOut(true)
    try { await fetch('/api/auth/logout', { method: 'POST' }) } catch { /* */ }
    window.location.href = '/login'
  }

  function isActive(href: string) {
    if (href === '/dashboard' || href === '/orders' || href === '/reports') return pathname === href
    return pathname.startsWith(href)
  }

  function renderLink(item: { href: string; label: string; icon: IconType }, indent = false) {
    const active = isActive(item.href)
    const Icon = item.icon
    return (
      <Link key={item.href} href={item.href} onClick={() => setMobileOpen(false)}
        className={cn(
          'flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium transition-colors',
          indent && 'pl-10',
          active ? 'bg-blue-600 text-white shadow-sm' : 'text-gray-600 hover:bg-gray-100 hover:text-gray-800',
        )}>
        <Icon size={indent ? 17 : 20} strokeWidth={active ? 2.4 : 1.8} />
        {item.label}
      </Link>
    )
  }

  const sidebarContent = (
    <div className="flex flex-col h-full">
      <div className="flex items-center px-5 py-4 border-b border-gray-100">
        <Image
          src="/logo/bibo-gc-with-dashboard-icon.svg"
          alt="BiBo's GC"
          width={160}
          height={32}
          priority
        />
      </div>

      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        {NAV_ITEMS.map(item => {
          if (!item.children) return renderLink(item)

          const groupActive = isGroupActive(item, pathname)
          const expanded = openGroups[item.href] ?? false
          const Icon = item.icon

          return (
            <div key={item.href}>
              <button
                onClick={() => toggleGroup(item.href)}
                className={cn(
                  'w-full flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium transition-colors',
                  groupActive
                    ? 'bg-blue-50 text-blue-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-800',
                )}
              >
                <Icon size={20} strokeWidth={groupActive ? 2.4 : 1.8} />
                <span className="flex-1 text-left">{item.label}</span>
                <ChevronDown size={16} className={cn('transition-transform shrink-0', expanded && 'rotate-180')} />
              </button>
              {expanded && (
                <div className="mt-1 space-y-0.5">
                  {item.children.map(child => renderLink(child, true))}
                </div>
              )}
            </div>
          )
        })}
      </nav>

      <div className="px-3 py-4 border-t border-gray-100">
        <button onClick={() => setShowLogout(true)}
          className="flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium text-gray-500 hover:text-red-600 hover:bg-red-50 transition-colors w-full">
          <LogOut size={20} strokeWidth={1.8} /> Đăng xuất
        </button>
      </div>
    </div>
  )

  return (
    <>
      <div className="md:hidden bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between sticky top-0 z-10">
        <Image
          src="/logo/bibo-gc-with-dashboard-icon.svg"
          alt="BiBo's GC"
          width={140}
          height={28}
          priority
        />
        <div className="flex items-center gap-1">
          <NotificationBell />
          <button onClick={() => setMobileOpen(true)} className="p-2 text-gray-600 hover:bg-gray-100 rounded-md">
            <Menu size={22} />
          </button>
        </div>
      </div>

      {mobileOpen && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div className="absolute inset-0 bg-black/40" onClick={() => setMobileOpen(false)} />
          <div className="absolute left-0 top-0 bottom-0 w-64 bg-white shadow-xl">
            <div className="absolute right-3 top-4">
              <button onClick={() => setMobileOpen(false)} className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md">
                <X size={20} />
              </button>
            </div>
            {sidebarContent}
          </div>
        </div>
      )}

      <aside className="hidden md:flex md:flex-col md:w-56 lg:w-60 bg-white border-r border-gray-200 shrink-0">
        {sidebarContent}
      </aside>

      <ConfirmDialog open={showLogout} title="Đăng xuất?" description="Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?"
        icon={<div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center"><LogOut size={24} className="text-red-500" /></div>}
        confirmLabel="Đăng xuất" variant="danger" loading={loggingOut} onConfirm={handleLogout} onCancel={() => setShowLogout(false)} />
    </>
  )
}
