'use client'

import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import { api } from '@/lib/api'
import { formatCurrency, formatPercent } from '@/lib/utils'
import { ApiResponse, AnnualSalesReportDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'

export function AnnualReport() {
  const [year, setYear]       = useState(new Date().getFullYear())
  const [data, setData]       = useState<AnnualSalesReportDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError]     = useState('')

  useEffect(() => {
    setLoading(true)
    setError('')
    api.get(`/api/finance/reports/sales/annual?year=${year}`)
      .then(r => {
        if (!r.ok) throw new Error()
        return r.json()
      })
      .then((json: ApiResponse<AnnualSalesReportDto>) => setData(json.data))
      .catch(() => setError('Không thể tải báo cáo. Vui lòng thử lại.'))
      .finally(() => setLoading(false))
  }, [year])

  const chartData = (data?.monthlyBreakdowns ?? []).map(m => ({
    name: `T${m.month}`,
    revenue: m.revenue,
    transactions: m.transactionCount,
  }))

  const bestMonth = data?.monthlyBreakdowns.reduce((a, b) => b.revenue > a.revenue ? b : a,
    data.monthlyBreakdowns[0])

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="flex items-end gap-4">
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

        {!loading && !error && (
          <>
            <Card>
              <p className="text-sm text-gray-400">Tổng doanh thu năm</p>
              <p className="text-xl font-bold text-blue-600 mt-1">{formatCurrency(data?.totalRevenue ?? 0)}</p>
            </Card>
            <Card>
              <p className="text-sm text-gray-400">Tổng giao dịch</p>
              <p className="text-xl font-bold text-gray-800 mt-1">{data?.totalTransactions ?? 0}</p>
            </Card>
            {bestMonth && bestMonth.revenue > 0 && (
              <Card>
                <p className="text-sm text-gray-400">Tháng cao nhất</p>
                <p className="text-base font-bold text-emerald-600 mt-1">
                  {bestMonth.monthName} — {formatCurrency(bestMonth.revenue)}
                </p>
              </Card>
            )}
          </>
        )}
      </div>

      {error ? (
        <Card className="text-center py-8">
          <p className="text-sm text-red-500">{error}</p>
          <button onClick={() => setYear(year)} className="mt-3 text-sm text-blue-600 underline">Thử lại</button>
        </Card>
      ) : loading ? <LoadingSpinner /> : (
        <>
          <Card>
            <h3 className="font-semibold text-gray-700 mb-4 text-base">Doanh thu theo tháng — {year}</h3>
            <ResponsiveContainer width="100%" height={240}>
              <BarChart data={chartData} margin={{ top: 0, right: 0, left: -15, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 12, fill: '#9ca3af' }} />
                <YAxis tick={{ fontSize: 11, fill: '#9ca3af' }} tickFormatter={v => v >= 1e6 ? `${(v/1e6).toFixed(0)}tr` : `${(v/1e3).toFixed(0)}k`} />
                <Tooltip
                  formatter={(v: number) => [formatCurrency(v), 'Doanh thu']}
                  contentStyle={{ borderRadius: 6, border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)', fontSize: 13 }}
                />
                <Bar dataKey="revenue" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </Card>

          <Card>
            <h3 className="font-semibold text-gray-700 mb-4 text-base">Chi tiết từng tháng</h3>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-xs text-gray-400 border-b border-gray-200">
                    <th className="text-left py-3 font-medium">Tháng</th>
                    <th className="text-right py-3 font-medium">Doanh thu</th>
                    <th className="text-right py-3 font-medium">Giao dịch</th>
                    <th className="text-right py-3 font-medium">Tăng trưởng</th>
                  </tr>
                </thead>
                <tbody>
                  {(data?.monthlyBreakdowns ?? []).map(m => (
                    <tr key={m.month} className="border-b border-gray-50 hover:bg-gray-50/50">
                      <td className="py-3 text-gray-700">{m.monthName}</td>
                      <td className="py-3 text-right font-medium text-gray-800">{formatCurrency(m.revenue)}</td>
                      <td className="py-3 text-right text-gray-600">{m.transactionCount}</td>
                      <td className="py-3 text-right">
                        {m.moMGrowthPercent !== null ? (
                          <span className={m.moMGrowthPercent >= 0 ? 'text-emerald-600' : 'text-red-500'}>
                            {formatPercent(m.moMGrowthPercent)}
                          </span>
                        ) : (
                          <span className="text-gray-300">—</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        </>
      )}
    </div>
  )
}
