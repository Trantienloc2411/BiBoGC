'use client'

import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import { Card } from '@/components/ui/Card'
import { HourlySalesDto } from '@/types'

interface HourlyChartProps {
  data: HourlySalesDto[]
}

function formatMillions(value: number) {
  if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}tr`
  if (value >= 1_000) return `${(value / 1_000).toFixed(0)}k`
  return String(value)
}

export function HourlyChart({ data }: HourlyChartProps) {
  const chartData = data.map(d => ({
    hour: `${d.hour}h`,
    revenue: d.revenue,
    transactions: d.transactionCount,
  }))

  return (
    <Card className="h-full flex flex-col">
      <h3 className="font-semibold text-gray-700 mb-3 text-sm">Doanh thu theo giờ</h3>
      {chartData.length === 0 ? (
        <p className="text-center text-gray-400 py-6 text-sm">Chưa có dữ liệu hôm nay</p>
      ) : (
        <div className="flex-1 min-h-[160px]">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={chartData} margin={{ top: 0, right: 0, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
              <XAxis dataKey="hour" tick={{ fontSize: 11, fill: '#9ca3af' }} />
              <YAxis tickFormatter={formatMillions} tick={{ fontSize: 11, fill: '#9ca3af' }} />
              <Tooltip
                formatter={(v: number) => [`${formatMillions(v)}đ`, 'Doanh thu']}
                contentStyle={{ borderRadius: 6, border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)', fontSize: 13 }}
              />
              <Bar dataKey="revenue" fill="#3b82f6" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </Card>
  )
}
