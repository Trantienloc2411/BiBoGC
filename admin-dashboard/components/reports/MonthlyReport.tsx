'use client'

import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import { api } from '@/lib/api'
import { monthYear, formatCurrency, formatPercent } from '@/lib/utils'
import { ApiResponse, MonthlySalesReportDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { TopProducts } from '@/components/dashboard/TopProducts'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { ExportMenu, pdfIcon } from '@/components/ui/ExportMenu'

export function MonthlyReport() {
  const now = monthYear()
  const [year, setYear]       = useState(now.year)
  const [month, setMonth]     = useState(now.month)
  const [data, setData]       = useState<MonthlySalesReportDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError]     = useState('')

  useEffect(() => {
    setLoading(true)
    setError('')
    api.get(`/api/finance/reports/sales/monthly?year=${year}&month=${month}`)
      .then(r => {
        if (!r.ok) throw new Error()
        return r.json()
      })
      .then((json: ApiResponse<MonthlySalesReportDto>) => setData(json.data))
      .catch(() => setError('Không thể tải báo cáo. Vui lòng thử lại.'))
      .finally(() => setLoading(false))
  }, [year, month])

  const chartData = (data?.dailyBreakdown ?? []).map(d => ({
    day: `${d.day}`,
    revenue: d.revenue,
  }))

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 md:grid-cols-6 gap-4">
        <div className="col-span-2 flex gap-3 items-end">
          <Card className="flex gap-4 items-end flex-1">
            <div className="flex-1">
              <label className="text-xs text-gray-400 mb-1.5 block">Tháng</label>
              <select value={month} onChange={e => setMonth(Number(e.target.value))}
                className="w-full text-sm text-gray-700 focus:outline-none bg-transparent">
                {Array.from({ length: 12 }, (_, i) => (
                  <option key={i + 1} value={i + 1}>Tháng {i + 1}</option>
                ))}
              </select>
            </div>
            <div className="flex-1">
              <label className="text-xs text-gray-400 mb-1.5 block">Năm</label>
              <select value={year} onChange={e => setYear(Number(e.target.value))}
                className="w-full text-sm text-gray-700 focus:outline-none bg-transparent">
                {[2024, 2025, 2026].map(y => (
                  <option key={y} value={y}>{y}</option>
                ))}
              </select>
            </div>
          </Card>
          <ExportMenu options={[
            { label: 'PDF', icon: pdfIcon(), path: `/api/finance/reports/sales/monthly/export/pdf?year=${year}&month=${month}`, filename: `bao-cao-thang-${month}-${year}.pdf` },
          ]} />
        </div>

        {!loading && !error && (
          <>
            <Card>
              <p className="text-sm text-gray-400">Doanh thu</p>
              <p className="text-xl font-bold text-blue-600 mt-1">{formatCurrency(data?.totalRevenue ?? 0)}</p>
            </Card>
            <Card>
              <p className="text-sm text-gray-400">Giao dịch</p>
              <p className="text-xl font-bold text-gray-800 mt-1">{data?.transactionCount ?? 0}</p>
            </Card>
            <Card>
              <p className="text-sm text-gray-400">So tháng trước</p>
              <p className={`text-base font-semibold mt-1 ${(data?.previousMonthChangePercent ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
                {formatPercent(data?.previousMonthChangePercent ?? 0)}
              </p>
            </Card>
            <Card>
              <p className="text-sm text-gray-400">So cùng kỳ</p>
              <p className={`text-base font-semibold mt-1 ${(data?.yoYChangePercent ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
                {formatPercent(data?.yoYChangePercent ?? 0)}
              </p>
            </Card>
          </>
        )}
      </div>

      {error ? (
        <Card className="text-center py-8">
          <p className="text-sm text-red-500">{error}</p>
          <button onClick={() => { setMonth(month) }} className="mt-3 text-sm text-blue-600 underline">Thử lại</button>
        </Card>
      ) : loading ? <LoadingSpinner /> : (
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
          <Card className="lg:col-span-3">
            <h3 className="font-semibold text-gray-700 mb-4 text-base">Doanh thu từng ngày</h3>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={chartData} margin={{ top: 0, right: 0, left: -15, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
                <XAxis dataKey="day" tick={{ fontSize: 11, fill: '#9ca3af' }} />
                <YAxis tick={{ fontSize: 11, fill: '#9ca3af' }} tickFormatter={v => v >= 1e6 ? `${(v/1e6).toFixed(0)}tr` : `${(v/1e3).toFixed(0)}k`} />
                <Tooltip
                  formatter={(v: number) => [formatCurrency(v), 'Doanh thu']}
                  contentStyle={{ borderRadius: 6, border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)', fontSize: 13 }}
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
