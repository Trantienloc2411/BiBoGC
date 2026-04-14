'use client'

import { useState } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { api } from '@/lib/api'
import { MoneyInput } from '@/components/ui/MoneyInput'
import { ExpenseCategory, ExpenseCategoryLabel, ExpensePaymentMethod, ExpensePaymentMethodLabel } from '@/types'

interface Props {
  onClose: () => void
  onSuccess: () => void
  defaultDate: string
}

export function AddExpenseModal({ onClose, onSuccess, defaultDate }: Props) {
  const [category, setCategory]       = useState<number>(ExpenseCategory.Other)
  const [amount, setAmount]           = useState(0)
  const [description, setDescription] = useState('')
  const [expenseDate, setExpenseDate] = useState(defaultDate)
  const [paymentMethod, setPayment]   = useState<number>(ExpensePaymentMethod.Cash)
  const [receiptNumber, setReceipt]   = useState('')
  const [loading, setLoading]         = useState(false)
  const [error, setError]             = useState('')

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')

    const amt = amount
    if (!amt || amt <= 0) { setError('Số tiền phải lớn hơn 0.'); return }
    if (!description.trim()) { setError('Vui lòng nhập mô tả.'); return }

    setLoading(true)
    try {
      const res = await api.post('/api/finance/expenses', {
        category,
        amount: amt,
        description: description.trim(),
        expenseDate: `${expenseDate}T00:00:00`,
        paymentMethod,
        receiptNumber: receiptNumber.trim() || undefined,
      })

      if (!res.ok) {
        const data = await res.json()
        setError(data?.message ?? 'Lỗi khi lưu chi phí.')
        return
      }

      onSuccess()
    } catch {
      setError('Không thể kết nối đến máy chủ.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/40 z-50 flex items-end sm:items-center justify-center p-4">
      <div className="bg-white w-full max-w-md rounded-lg shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
          <h2 className="font-semibold text-gray-800 text-lg">Thêm chi phí</h2>
          <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-md transition-colors">
            <X size={18} />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-5 space-y-4">
          {/* Category */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Danh mục</label>
            <select
              value={category}
              onChange={e => setCategory(Number(e.target.value))}
              className="w-full px-4 py-3 rounded-md border border-gray-300 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {Object.entries(ExpenseCategoryLabel).map(([val, label]) => (
                <option key={val} value={val}>{label}</option>
              ))}
            </select>
          </div>

          {/* Amount */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Số tiền (VNĐ)</label>
            <MoneyInput
              value={amount}
              onChange={setAmount}
              placeholder="100,000"
              className="w-full px-4 py-3 rounded-md border border-gray-300 text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Mô tả</label>
            <input
              type="text"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="Tiền điện tháng 3..."
              className="w-full px-4 py-3 rounded-md border border-gray-300 text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Ngày</label>
            <input
              type="date"
              value={expenseDate}
              onChange={e => setExpenseDate(e.target.value)}
              className="w-full px-4 py-3 rounded-md border border-gray-300 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Payment method */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Hình thức thanh toán</label>
            <div className="flex gap-2">
              {Object.entries(ExpensePaymentMethodLabel).map(([val, label]) => (
                <button
                  key={val}
                  type="button"
                  onClick={() => setPayment(Number(val))}
                  className={`flex-1 py-2.5 text-sm rounded-md border transition-all ${
                    paymentMethod === Number(val)
                      ? 'bg-blue-600 text-white border-blue-600 font-medium'
                      : 'border-gray-300 text-gray-600 hover:border-blue-400'
                  }`}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>

          {/* Receipt (optional) */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              Số hoá đơn <span className="text-gray-400 font-normal">(tuỳ chọn)</span>
            </label>
            <input
              type="text"
              value={receiptNumber}
              onChange={e => setReceipt(e.target.value)}
              placeholder="INV-001"
              className="w-full px-4 py-3 rounded-md border border-gray-300 text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Error */}
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-md px-4 py-3 text-sm text-red-600">
              ⚠ {error}
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-1">
            <Button type="button" variant="secondary" size="md" className="flex-1" onClick={onClose}>
              Huỷ
            </Button>
            <Button type="submit" size="md" className="flex-1" loading={loading}>
              Lưu lại
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}
