import { Card } from '@/components/ui/Card'
import { formatCurrency } from '@/lib/utils'
import { TopProductDto } from '@/types'
import { Trophy } from 'lucide-react'

const MEDALS = ['🥇', '🥈', '🥉']

export function TopProducts({ products }: { products: TopProductDto[] }) {
  return (
    <Card className="h-full flex flex-col">
      <div className="flex items-center gap-2 mb-3">
        <Trophy size={16} className="text-yellow-500" />
        <h3 className="font-semibold text-gray-700 text-sm">Top sản phẩm hôm nay</h3>
      </div>
      {products.length === 0 ? (
        <p className="text-center text-gray-400 py-4 text-sm">Chưa có giao dịch hôm nay</p>
      ) : (
        <ul className="divide-y divide-gray-50 flex-1 overflow-y-auto">
          {products.slice(0, 5).map((p, i) => (
            <li key={i} className="flex items-center justify-between py-2">
              <div className="flex items-center gap-2.5">
                <span className="text-base w-5">{MEDALS[i] ?? `${i + 1}.`}</span>
                <div>
                  <p className="text-sm font-medium text-gray-800">{p.productName}</p>
                  <p className="text-xs text-gray-400">{p.variantName} · {p.quantitySold} cái</p>
                </div>
              </div>
              <span className="text-sm font-semibold text-blue-600">{formatCurrency(p.revenue)}</span>
            </li>
          ))}
        </ul>
      )}
    </Card>
  )
}
