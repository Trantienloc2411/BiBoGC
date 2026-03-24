'use client'

import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { todayISO, formatDate } from '@/lib/utils'
import { DailySalesReportDto, ApiResponse, DailyExpenseSummaryDto } from '@/types'
import { StatCard } from '@/components/dashboard/StatCard'
import { HourlyChart } from '@/components/dashboard/HourlyChart'
import { TopProducts } from '@/components/dashboard/TopProducts'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { CalendarDays } from 'lucide-react'

export default function DashboardPage() {
  const [sales, setSales] = useState<DailySalesReportDto | null>(null)
  const [expenses, setExpenses] = useState<DailyExpenseSummaryDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const today = todayISO()

  useEffect(() => {
    async function load() {
      setLoading(true)
      try {
        const [salesRes, expensesRes] = await Promise.all([
          api.get(`/api/finance/reports/sales/daily?date=${today}`),
          api.get(`/api/finance/expenses/daily?date=${today}`),
        ])

        if (salesRes.ok) {
          const json: ApiResponse<DailySalesReportDto> = await salesRes.json()
          setSales(json.data)
        }

        if (expensesRes.ok) {
          const json: ApiResponse<DailyExpenseSummaryDto> = await expensesRes.json()
          setExpenses(json.data)
        }
      } catch {
        setError('Không thể tải dữ liệu. Vui lòng thử lại.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [today])

  if (loading) return <LoadingSpinner />

  if (error) {
    return (
      <div className="text-center py-20 text-red-500">
        <p className="text-base">{error}</p>
        <button onClick={() => window.location.reload()} className="mt-4 text-blue-600 underline text-sm">
          Tải lại
        </button>
      </div>
    )
  }

  const revenue  = sales?.totalRevenue ?? 0
  const expense  = expenses?.totalAmount ?? 0
  const profit   = revenue - expense
  const change   = sales?.revenueChangePercent ?? 0

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2.5 text-gray-500">
        <CalendarDays size={18} />
        <span className="text-sm">{formatDate(today)}</span>
        {sales && (
          <span className="ml-auto bg-blue-50 rounded-md px-3.5 py-1.5 text-sm text-blue-700 font-medium">
            {sales.transactionCount} giao dịch
          </span>
        )}
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <StatCard label="Doanh thu hôm nay" value={revenue} changePercent={change} icon="💰" color="blue" />
        <StatCard label="Chi phí hôm nay"   value={expense} icon="💸" color="red" />
        <StatCard
          label="Lợi nhuận tạm tính"
          value={profit}
          icon="📈"
          color={profit >= 0 ? 'green' : 'red'}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
        <div className="lg:col-span-3">
          <HourlyChart data={sales?.salesByHour ?? []} />
        </div>
        <div className="lg:col-span-2">
          <TopProducts products={sales?.topSellingProducts ?? []} />
        </div>
      </div>
    </div>
  )
}
