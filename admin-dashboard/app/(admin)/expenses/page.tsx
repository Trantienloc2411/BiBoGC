'use client'

import { useEffect, useState, useCallback } from 'react'
import { Plus, CalendarDays } from 'lucide-react'
import { api } from '@/lib/api'
import { todayISO, formatCurrency, formatDate } from '@/lib/utils'
import { ApiResponse, DailyExpenseSummaryDto, ExpenseCategoryLabel } from '@/types'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { AddExpenseModal } from '@/components/expenses/AddExpenseModal'

export default function ExpensesPage() {
  const [data, setData]         = useState<DailyExpenseSummaryDto | null>(null)
  const [date, setDate]         = useState(todayISO())
  const [loading, setLoading]   = useState(true)
  const [showModal, setModal]   = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const res = await api.get(`/api/finance/expenses/daily?date=${date}`)
      if (res.ok) {
        const json: ApiResponse<DailyExpenseSummaryDto> = await res.json()
        setData(json.data)
      }
    } finally {
      setLoading(false)
    }
  }, [date])

  useEffect(() => { load() }, [load])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-4">
        <h1 className="text-xl font-bold text-gray-800">Chi phí</h1>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 bg-white border border-gray-200 rounded-md px-3.5 py-2">
            <CalendarDays size={16} className="text-gray-400" />
            <input
              type="date"
              value={date}
              onChange={e => setDate(e.target.value)}
              className="text-sm text-gray-700 focus:outline-none bg-transparent"
            />
          </div>
          <Button size="sm" onClick={() => setModal(true)}>
            <Plus size={16} className="mr-1.5" /> Thêm
          </Button>
        </div>
      </div>

      {loading ? <LoadingSpinner /> : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Card className="bg-gradient-to-r from-red-50 to-orange-50 border-red-100">
              <p className="text-sm text-gray-500">Tổng chi phí ngày {formatDate(date)}</p>
              <p className="text-3xl font-bold text-red-600 mt-2">
                {formatCurrency(data?.totalAmount ?? 0)}
              </p>
            </Card>

            {(data?.breakdownByCategory?.length ?? 0) > 0 && (
              <Card>
                <h3 className="font-semibold text-gray-700 mb-3 text-sm uppercase tracking-wide">Theo danh mục</h3>
                <ul className="space-y-2.5">
                  {data!.breakdownByCategory.map((b, i) => {
                    const label = Object.entries(ExpenseCategoryLabel)
                      .find(([, v]) => v.toLowerCase() === b.category.toLowerCase())?.[1] ?? b.category
                    return (
                      <li key={i} className="flex items-center justify-between">
                        <span className="text-sm text-gray-600">{label} <span className="text-gray-400">({b.count})</span></span>
                        <span className="font-medium text-gray-800">{formatCurrency(b.total)}</span>
                      </li>
                    )
                  })}
                </ul>
              </Card>
            )}
          </div>

          <Card className="flex flex-col">
            <h3 className="font-semibold text-gray-700 mb-3 text-sm uppercase tracking-wide">Danh sách chi phí</h3>
            {(data?.expenses?.length ?? 0) === 0 ? (
              <p className="text-center text-gray-400 py-8 text-sm">Không có chi phí nào trong ngày này</p>
            ) : (
              <ul className="divide-y divide-gray-100 max-h-[calc(100vh-24rem)] overflow-y-auto">
                {data!.expenses.map(exp => (
                  <li key={exp.id} className="py-3.5 flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-800 truncate">{exp.description}</p>
                      <p className="text-xs text-gray-400 mt-1">{exp.paymentMethod}</p>
                    </div>
                    <span className="text-sm font-semibold text-red-600 shrink-0">
                      -{formatCurrency(exp.amount)}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </Card>
        </>
      )}

      {showModal && (
        <AddExpenseModal
          defaultDate={date}
          onClose={() => setModal(false)}
          onSuccess={() => { setModal(false); load() }}
        />
      )}
    </div>
  )
}
