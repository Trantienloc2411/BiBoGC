import { Navbar } from '@/components/layout/Navbar'
import { BottomNav } from '@/components/layout/BottomNav'

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="h-screen bg-slate-50 flex flex-col overflow-hidden">
      <Navbar />
      <main className="flex-1 overflow-y-auto pb-16 max-w-6xl w-full mx-auto px-4 py-4 md:px-6">
        {children}
      </main>
      <BottomNav />
    </div>
  )
}
