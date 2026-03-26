'use client'

import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { todayISO, formatCurrency, formatPercent } from '@/lib/utils'
import { ApiResponse, DailySalesReportDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { HourlyChart } from '@/components/dashboard/HourlyChart'
import { TopProducts } from '@/components/dashboard/TopProducts'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { ExportMenu, pdfIcon } from '@/components/ui/ExportMenu'
import { CalendarDays } from 'lucide-react'

export function DailyReport() {
  const [date, setDate]       = useState(todayISO())
  const [data, setData]       = useState<DailySalesReportDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError]     = useState('')

  useEffect(() => {
    setLoading(true)
    setError('')
    api.get(`/api/finance/reports/sales/daily?date=${date}`)
      .then(r => {
        if (!r.ok) throw new Error('Lỗi tải dữ liệu')
        return r.json()
      })
      .then((json: ApiResponse<DailySalesReportDto>) => setData(json.data))
      .catch(() => setError('Không thể tải báo cáo. Vui lòng thử lại.'))
      .finally(() => setLoading(false))
  }, [date])

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="flex items-center gap-2.5 md:col-span-1">
          <div className="flex items-center gap-2 bg-white border border-gray-200 rounded-md px-4 py-2.5 flex-1">
            <CalendarDays size={16} className="text-gray-400" />
            <input
              type="date"
              value={date}
              onChange={e => setDate(e.target.value)}
              className="flex-1 text-sm text-gray-700 focus:outline-none bg-transparent"
            />
          </div>
          <ExportMenu options={[
            { label: 'PDF', icon: pdfIcon(), path: `/api/finance/reports/sales/daily/export/pdf?date=${date}`, filename: `bao-cao-ngay-${date}.pdf` },
          ]} />
        </div>

        {!loading && !error && (
          <>
            <Card className="flex items-center justify-between">
              <p className="text-sm text-gray-400">Doanh thu</p>
              <p className="text-xl font-bold text-blue-600">{formatCurrency(data?.totalRevenue ?? 0)}</p>
            </Card>
            <Card className="flex items-center justify-between">
              <p className="text-sm text-gray-400">Giao dịch</p>
              <p className="text-xl font-bold text-gray-800">{data?.transactionCount ?? 0}</p>
            </Card>
            <Card className="flex items-center justify-between">
              <p className="text-sm text-gray-400">So hôm qua</p>
              <p className={`text-base font-semibold ${(data?.revenueChangePercent ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
                {formatPercent(data?.revenueChangePercent ?? 0)}
                <span className="text-gray-400 font-normal ml-1.5 text-sm">({formatCurrency(data?.previousDayRevenue ?? 0)})</span>
              </p>
            </Card>
          </>
        )}
      </div>

      {error ? (
        <Card className="text-center py-8">
          <p className="text-sm text-red-500">{error}</p>
          <button onClick={() => setDate(date)} className="mt-3 text-sm text-blue-600 underline">Thử lại</button>
        </Card>
      ) : loading ? <LoadingSpinner /> : (
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
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
