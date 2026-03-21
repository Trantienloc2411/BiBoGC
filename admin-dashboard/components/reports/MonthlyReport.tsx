'use client'

import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import { api } from '@/lib/api'
import { monthYear, formatCurrency, formatPercent } from '@/lib/utils'
import { ApiResponse, MonthlySalesReportDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { TopProducts } from '@/components/dashboard/TopProducts'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'

export function MonthlyReport() {
  const now = monthYear()
  const [year, setYear]   = useState(now.year)
  const [month, setMonth] = useState(now.month)
  const [data, setData]   = useState<MonthlySalesReportDto | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    api.get(`/api/finance/reports/sales/monthly?year=${year}&month=${month}`)
      .then(r => r.ok ? r.json() : null)
      .then((json: ApiResponse<MonthlySalesReportDto> | null) => {
        if (json) setData(json.data)
      })
      .finally(() => setLoading(false))
  }, [year, month])

  const chartData = (data?.dailyBreakdown ?? []).map(d => ({
    day: `${d.day}`,
    revenue: d.revenue,
  }))

  return (
    <div className="space-y-3">
      {/* Month/year picker + stats in one row on md+ */}
      <div className="grid grid-cols-2 md:grid-cols-6 gap-3">
        <Card className="col-span-2 flex gap-3 items-end">
          <div className="flex-1">
            <label className="text-xs text-gray-400 mb-1 block">Tháng</label>
            <select
              value={month}
              onChange={e => setMonth(Number(e.target.value))}
              className="w-full text-sm text-gray-700 focus:outline-none bg-transparent"
            >
              {Array.from({ length: 12 }, (_, i) => (
                <option key={i + 1} value={i + 1}>Tháng {i + 1}</option>
              ))}
            </select>
          </div>
          <div className="flex-1">
            <label className="text-xs text-gray-400 mb-1 block">Năm</label>
            <select
              value={year}
              onChange={e => setYear(Number(e.target.value))}
              className="w-full text-sm text-gray-700 focus:outline-none bg-transparent"
            >
              {[2024, 2025, 2026].map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>
          </div>
        </Card>

        {!loading && (
          <>
            <Card>
              <p className="text-xs text-gray-400">Doanh thu</p>
              <p className="text-lg font-bold text-blue-600 mt-0.5">{formatCurrency(data?.totalRevenue ?? 0)}</p>
            </Card>
            <Card>
              <p className="text-xs text-gray-400">Giao dịch</p>
              <p className="text-lg font-bold text-gray-800 mt-0.5">{data?.transactionCount ?? 0}</p>
            </Card>
            <Card>
              <p className="text-xs text-gray-400">So tháng trước</p>
              <p className={`text-sm font-semibold mt-0.5 ${(data?.previousMonthChangePercent ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
                {formatPercent(data?.previousMonthChangePercent ?? 0)}
              </p>
            </Card>
            <Card>
              <p className="text-xs text-gray-400">So cùng kỳ</p>
              <p className={`text-sm font-semibold mt-0.5 ${(data?.yoYChangePercent ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
                {formatPercent(data?.yoYChangePercent ?? 0)}
              </p>
            </Card>
          </>
        )}
      </div>

      {loading ? <LoadingSpinner /> : (
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-3">
          {/* Daily breakdown chart */}
          <Card className="lg:col-span-3">
            <h3 className="font-semibold text-gray-700 mb-3 text-sm">Doanh thu từng ngày</h3>
            <ResponsiveContainer width="100%" height={180}>
              <BarChart data={chartData} margin={{ top: 0, right: 0, left: -20, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
                <XAxis dataKey="day" tick={{ fontSize: 10, fill: '#9ca3af' }} />
                <YAxis tick={{ fontSize: 10, fill: '#9ca3af' }} tickFormatter={v => v >= 1e6 ? `${(v/1e6).toFixed(0)}tr` : `${(v/1e3).toFixed(0)}k`} />
                <Tooltip
                  formatter={(v: number) => [formatCurrency(v), 'Doanh thu']}
                  contentStyle={{ borderRadius: 6, border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)', fontSize: 12 }}
                />
                <Bar dataKey="revenue" fill="#6366f1" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </Card>

          <div className="lg:col-span-2">
            <TopProducts products={data?.topSellingProducts ?? []} />
          </div>
        </div>
      )}
    </div>
  )
}
