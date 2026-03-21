'use client'

import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { todayISO, formatCurrency, formatPercent } from '@/lib/utils'
import { ApiResponse, DailySalesReportDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { HourlyChart } from '@/components/dashboard/HourlyChart'
import { TopProducts } from '@/components/dashboard/TopProducts'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { CalendarDays } from 'lucide-react'

export function DailyReport() {
  const [date, setDate]   = useState(todayISO())
  const [data, setData]   = useState<DailySalesReportDto | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    api.get(`/api/finance/reports/sales/daily?date=${date}`)
      .then(r => r.ok ? r.json() : null)
      .then((json: ApiResponse<DailySalesReportDto> | null) => {
        if (json) setData(json.data)
      })
      .finally(() => setLoading(false))
  }, [date])

  return (
    <div className="space-y-3">
      {/* Date picker + summary inline */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
        <div className="flex items-center gap-2 bg-white border border-gray-200 rounded-md px-3 py-1.5 md:col-span-1">
          <CalendarDays size={14} className="text-gray-400" />
          <input
            type="date"
            value={date}
            onChange={e => setDate(e.target.value)}
            className="flex-1 text-sm text-gray-700 focus:outline-none bg-transparent"
          />
        </div>

        {!loading && (
          <>
            <Card className="flex items-center justify-between py-2 md:py-0">
              <p className="text-xs text-gray-400">Doanh thu</p>
              <p className="text-lg font-bold text-blue-600">{formatCurrency(data?.totalRevenue ?? 0)}</p>
            </Card>
            <Card className="flex items-center justify-between py-2 md:py-0">
              <p className="text-xs text-gray-400">Giao dịch</p>
              <p className="text-lg font-bold text-gray-800">{data?.transactionCount ?? 0}</p>
            </Card>
            <Card className="flex items-center justify-between py-2 md:py-0">
              <p className="text-xs text-gray-400">So hôm qua</p>
              <p className={`text-sm font-semibold ${(data?.revenueChangePercent ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
                {formatPercent(data?.revenueChangePercent ?? 0)}
                <span className="text-gray-400 font-normal ml-1">({formatCurrency(data?.previousDayRevenue ?? 0)})</span>
              </p>
            </Card>
          </>
        )}
      </div>

      {loading ? <LoadingSpinner /> : (
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-3">
          <div className="lg:col-span-3">
            <HourlyChart data={data?.salesByHour ?? []} />
          </div>
          <div className="lg:col-span-2">
            <TopProducts products={data?.topSellingProducts ?? []} />
          </div>
        </div>
      )}
    </div>
  )
}
