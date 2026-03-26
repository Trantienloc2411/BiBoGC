import { TrendingUp, TrendingDown, Minus } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { formatCurrency, formatPercent } from '@/lib/utils'

interface StatCardProps {
  label: string
  value: number
  changePercent?: number
  icon: string
  color: 'blue' | 'red' | 'green'
}

export function StatCard({ label, value, changePercent, icon, color }: StatCardProps) {
  const colors = {
    blue:  'bg-blue-50 text-blue-600',
    red:   'bg-red-50 text-red-600',
    green: 'bg-emerald-50 text-emerald-600',
  }

  const isPositive = (changePercent ?? 0) > 0
  const isZero     = (changePercent ?? 0) === 0

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <span className="text-sm text-gray-500 font-medium">{label}</span>
        <span className={`text-lg p-2 rounded-md ${colors[color]}`}>{icon}</span>
      </div>
      <p className="text-2xl font-bold text-gray-800">{formatCurrency(value)}</p>
      {changePercent !== undefined && (
        <div className={`flex items-center gap-1 text-sm font-medium ${
          isZero ? 'text-gray-400' : isPositive ? 'text-emerald-600' : 'text-red-500'
        }`}>
          {isZero ? <Minus size={14} /> : isPositive ? <TrendingUp size={14} /> : <TrendingDown size={14} />}
          <span>{formatPercent(changePercent)} so với hôm qua</span>
        </div>
      )}
    </Card>
  )
}
