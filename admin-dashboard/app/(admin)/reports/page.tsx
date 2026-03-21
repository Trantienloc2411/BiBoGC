'use client'

import { useState } from 'react'
import { cn } from '@/lib/utils'
import { DailyReport } from '@/components/reports/DailyReport'
import { MonthlyReport } from '@/components/reports/MonthlyReport'
import { FinancialReport } from '@/components/reports/FinancialReport'

const TABS = [
  { id: 'daily',     label: 'Hôm nay' },
  { id: 'monthly',   label: 'Tháng' },
  { id: 'financial', label: 'Tài chính' },
] as const

type TabId = typeof TABS[number]['id']

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<TabId>('daily')

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-lg font-bold text-gray-800">Báo cáo</h1>

        {/* Tab bar inline with header */}
        <div className="flex bg-gray-100 rounded-md p-0.5 gap-0.5">
          {TABS.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                'px-4 py-1.5 text-sm font-medium rounded transition-all',
                activeTab === tab.id
                  ? 'bg-white text-blue-600 shadow-sm'
                  : 'text-gray-500 hover:text-gray-700'
              )}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab content */}
      {activeTab === 'daily'     && <DailyReport />}
      {activeTab === 'monthly'   && <MonthlyReport />}
      {activeTab === 'financial' && <FinancialReport />}
    </div>
  )
}
