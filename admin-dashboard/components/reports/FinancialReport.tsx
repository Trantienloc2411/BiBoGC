'use client'

import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { monthYear, formatCurrency } from '@/lib/utils'
import { ApiResponse, MonthlyFinancialReportDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'

interface Row { label: string; value: number; color?: string; bold?: boolean }

export function FinancialReport() {
  const now = monthYear()
  const [year, setYear]   = useState(now.year)
  const [month, setMonth] = useState(now.month)
  const [data, setData]   = useState<MonthlyFinancialReportDto | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    api.get(`/api/finance/reports/financial/monthly?year=${year}&month=${month}`)
      .then(r => r.ok ? r.json() : null)
      .then((json: ApiResponse<MonthlyFinancialReportDto> | null) => {
        if (json) setData(json.data)
      })
      .finally(() => setLoading(false))
  }, [year, month])

  const rows: Row[] = data ? [
    { label: 'Doanh thu',         value: data.totalRevenue,  color: 'text-blue-600' },
    { label: 'Giá vốn hàng bán',  value: data.totalCogs,     color: 'text-orange-500' },
    { label: 'Lợi nhuận gộp',     value: data.grossProfit,   color: 'text-emerald-600', bold: true },
    { label: 'Chi phí hoạt động', value: data.totalExpenses, color: 'text-red-500' },
    { label: 'Lợi nhuận ròng',    value: data.netProfit,     color: data.netProfit >= 0 ? 'text-emerald-600' : 'text-red-600', bold: true },
  ] : []

  return (
    <div className="space-y-3">
      {/* Month / year picker */}
      <Card className="flex gap-3 items-end">
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

      {loading ? <LoadingSpinner /> : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          {/* P&L table */}
          <Card className="md:col-span-2">
            <h3 className="font-semibold text-gray-700 mb-3 text-sm">
              Báo cáo lãi/lỗ — Tháng {month}/{year}
            </h3>
            <ul className="divide-y divide-gray-50">
              {rows.map((row, i) => (
                <li key={i} className={`flex justify-between py-2.5 ${row.bold ? 'border-t-2 border-gray-200 mt-1' : ''}`}>
                  <span className={`text-sm ${row.bold ? 'font-semibold text-gray-800' : 'text-gray-500'}`}>
                    {row.label}
                  </span>
                  <span className={`text-sm font-semibold ${row.color ?? 'text-gray-800'}`}>
                    {formatCurrency(row.value)}
                  </span>
                </li>
              ))}
            </ul>
          </Card>

          {/* Profit margin */}
          {data && (
            <Card className="flex flex-col items-center justify-center">
              <p className="text-xs text-gray-400 mb-1">Biên lợi nhuận ròng</p>
              <p className={`text-4xl font-bold ${data.profitMarginPercent >= 0 ? 'text-emerald-600' : 'text-red-600'}`}>
                {data.profitMarginPercent.toFixed(1)}%
              </p>
            </Card>
          )}
        </div>
      )}
    </div>
  )
}
