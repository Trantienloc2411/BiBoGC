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
    <Card className="flex flex-col gap-1">
      <div className="flex items-center justify-between">
        <span className="text-xs text-gray-500 font-medium">{label}</span>
        <span className={`text-base p-1.5 rounded-md ${colors[color]}`}>{icon}</span>
      </div>
      <p className="text-xl font-bold text-gray-800">{formatCurrency(value)}</p>
      {changePercent !== undefined && (
        <div className={`flex items-center gap-1 text-xs font-medium ${
          isZero ? 'text-gray-400' : isPositive ? 'text-emerald-600' : 'text-red-500'
        }`}>
          {isZero ? <Minus size={12} /> : isPositive ? <TrendingUp size={12} /> : <TrendingDown size={12} />}
          <span>{formatPercent(changePercent)} so với hôm qua</span>
        </div>
      )}
    </Card>
  )
}
