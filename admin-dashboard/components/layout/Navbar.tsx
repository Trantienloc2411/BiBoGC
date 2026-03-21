'use client'

import { useRouter } from 'next/navigation'
import { LogOut, Store } from 'lucide-react'

export function Navbar() {
  const router = useRouter()

  async function handleLogout() {
    await fetch('/api/auth/logout', { method: 'POST' })
    router.push('/login')
    router.refresh()
  }

  return (
    <header className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between sticky top-0 z-10">
      <div className="flex items-center gap-2">
        <Store className="text-blue-600" size={22} />
        <span className="font-bold text-gray-800 text-lg">BiBo&apos;s Admin</span>
      </div>
      <button
        onClick={handleLogout}
        className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-red-600 transition-colors px-3 py-1.5 rounded-md hover:bg-red-50"
      >
        <LogOut size={16} />
        <span className="hidden sm:inline">Đăng xuất</span>
      </button>
    </header>
  )
}
