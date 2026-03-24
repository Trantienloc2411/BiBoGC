'use client'

import { useEffect, useState, useCallback } from 'react'
import { api } from '@/lib/api'
import { downloadFile } from '@/lib/download'
import { monthYear, formatDate } from '@/lib/utils'
import { ApiResponse, TaxConfigsDto, TaxConfigDto } from '@/types'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'
import { FileSpreadsheet, FileText, Loader2, ToggleLeft, ToggleRight, Download } from 'lucide-react'

interface PendingDownload {
  key: string
  path: string
  filename: string
  label: string
}

export default function TaxConfigPage() {
  const [data, setData]       = useState<TaxConfigsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError]     = useState('')
  const [saving, setSaving]   = useState<string | null>(null)
  const { error: showError } = useToast()

  const now = monthYear()
  const [exportYear, setExportYear]       = useState(now.year)
  const [exportMonth, setExportMonth]     = useState(now.month)
  const [declMonthFrom, setDeclMonthFrom] = useState(1)
  const [declMonthTo, setDeclMonthTo]     = useState(now.month)
  const [downloading, setDownloading]     = useState<string | null>(null)
  const [pendingDl, setPendingDl]         = useState<PendingDownload | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const res = await api.get('/api/finance/tax-config')
      if (!res.ok) throw new Error()
      const json: ApiResponse<TaxConfigsDto> = await res.json()
      setData(json.data)
    } catch {
      setError('Không thể tải cấu hình thuế.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  async function handleToggle(taxType: 'VAT' | 'PIT', current: TaxConfigDto) {
    setSaving(taxType)
    try {
      const res = await api.put('/api/finance/tax-config', {
        taxType,
        rate: current.rate,
        isEnabled: !current.isEnabled,
      })
      if (res.ok) await load()
    } finally {
      setSaving(null)
    }
  }

  async function handleRateChange(taxType: 'VAT' | 'PIT', current: TaxConfigDto, newRate: number) {
    setSaving(taxType)
    try {
      const res = await api.put('/api/finance/tax-config', {
        taxType,
        rate: newRate,
        isEnabled: current.isEnabled,
      })
      if (res.ok) await load()
    } finally {
      setSaving(null)
    }
  }

  async function doDownload(dl: PendingDownload) {
    setDownloading(dl.key)
    setPendingDl(null)
    try {
      await downloadFile(dl.path, dl.filename)
    } catch {
      showError('Tải file thất bại', 'Không thể tải file từ server. Vui lòng thử lại.')
    } finally {
      setDownloading(null)
    }
  }

  if (loading) return <LoadingSpinner />
  if (error) {
    return (
      <div className="space-y-4">
        <h1 className="text-xl font-bold text-gray-800">Cấu hình thuế</h1>
        <Card className="text-center py-8">
          <p className="text-sm text-red-500">{error}</p>
          <button onClick={load} className="mt-3 text-sm text-blue-600 underline">Thử lại</button>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-bold text-gray-800">Cấu hình thuế</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {data && (['vat', 'pit'] as const).map(key => {
          const cfg = data[key]
          const taxType = key.toUpperCase() as 'VAT' | 'PIT'
          const isSaving = saving === taxType
          return (
            <Card key={key}>
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h3 className="text-base font-semibold text-gray-800">{cfg.name}</h3>
                  <p className="text-sm text-gray-400 mt-1">
                    Hiệu lực từ {formatDate(cfg.effectiveFrom.slice(0, 10))}
                  </p>
                </div>
                <button
                  onClick={() => handleToggle(taxType, cfg)}
                  disabled={isSaving}
                  className="text-gray-500 hover:text-blue-600 transition-colors disabled:opacity-50"
                >
                  {cfg.isEnabled
                    ? <ToggleRight size={32} className="text-blue-600" />
                    : <ToggleLeft size={32} className="text-gray-400" />
                  }
                </button>
              </div>

              <div className="flex items-center gap-4">
                <label className="text-sm text-gray-500">Thuế suất:</label>
                <div className="flex items-center gap-2">
                  <input
                    type="number"
                    step="0.001"
                    min="0"
                    max="1"
                    value={cfg.rate}
                    onChange={e => {
                      const v = parseFloat(e.target.value)
                      if (!isNaN(v) && v >= 0 && v <= 1) handleRateChange(taxType, cfg, v)
                    }}
                    className="w-24 px-3 py-2 text-sm border border-gray-300 rounded-md text-right focus:outline-none focus:ring-2 focus:ring-blue-500"
                    disabled={isSaving}
                  />
                  <span className="text-sm text-gray-500">({(cfg.rate * 100).toFixed(1)}%)</span>
                </div>
              </div>

              <p className={`text-sm mt-3 ${cfg.isEnabled ? 'text-emerald-600' : 'text-gray-400'}`}>
                {cfg.isEnabled ? 'Đang áp dụng' : 'Đã tắt — dùng thuế suất mặc định theo TT 40/2021'}
              </p>
            </Card>
          )
        })}
      </div>

      <Card>
        <h3 className="font-semibold text-gray-700 mb-4 text-base">Xuất báo cáo thuế</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div className="space-y-3">
            <p className="text-sm text-gray-500 font-medium uppercase tracking-wide">Bảng tổng hợp thuế (Excel)</p>
            <div className="flex items-center gap-2.5">
              <select value={exportMonth} onChange={e => setExportMonth(Number(e.target.value))}
                className="text-sm border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500">
                {Array.from({ length: 12 }, (_, i) => (
                  <option key={i + 1} value={i + 1}>Tháng {i + 1}</option>
                ))}
              </select>
              <select value={exportYear} onChange={e => setExportYear(Number(e.target.value))}
                className="text-sm border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500">
                {[2024, 2025, 2026].map(y => (
                  <option key={y} value={y}>{y}</option>
                ))}
              </select>
              <Button
                size="sm"
                onClick={() => setPendingDl({
                  key: 'tax-excel',
                  path: `/api/finance/reports/tax/export?year=${exportYear}&month=${exportMonth}`,
                  filename: `thue-thang-${exportMonth}-${exportYear}.xlsx`,
                  label: 'Bảng tổng hợp thuế Excel',
                })}
                disabled={downloading === 'tax-excel'}
                className="bg-emerald-600 hover:bg-emerald-700 focus:ring-emerald-500"
              >
                {downloading === 'tax-excel'
                  ? <Loader2 size={16} className="animate-spin mr-1.5" />
                  : <FileSpreadsheet size={16} className="mr-1.5" />
                }
                Tải Excel
              </Button>
            </div>
          </div>

          <div className="space-y-3">
            <p className="text-sm text-gray-500 font-medium uppercase tracking-wide">Tờ khai thuế (Word)</p>
            <div className="flex items-center gap-2.5 flex-wrap">
              <select value={declMonthFrom} onChange={e => {
                const v = Number(e.target.value)
                setDeclMonthFrom(v)
                if (declMonthTo < v) setDeclMonthTo(v)
              }}
                className="text-sm border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500">
                {Array.from({ length: 12 }, (_, i) => (
                  <option key={i + 1} value={i + 1}>Từ T{i + 1}</option>
                ))}
              </select>
              <span className="text-gray-400 text-sm">→</span>
              <select value={declMonthTo} onChange={e => setDeclMonthTo(Number(e.target.value))}
                className="text-sm border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500">
                {Array.from({ length: 12 - declMonthFrom + 1 }, (_, i) => {
                  const m = declMonthFrom + i
                  return <option key={m} value={m}>Đến T{m}</option>
                })}
              </select>
              <select value={exportYear} onChange={e => setExportYear(Number(e.target.value))}
                className="text-sm border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500">
                {[2024, 2025, 2026].map(y => (
                  <option key={y} value={y}>{y}</option>
                ))}
              </select>
              <Button
                size="sm"
                onClick={() => setPendingDl({
                  key: 'tax-word',
                  path: `/api/finance/reports/tax/declaration/export?year=${exportYear}&monthFrom=${declMonthFrom}&monthTo=${declMonthTo}`,
                  filename: `to-khai-thue-T${declMonthFrom}-T${declMonthTo}-${exportYear}.docx`,
                  label: 'Tờ khai thuế Word',
                })}
                disabled={downloading === 'tax-word'}
              >
                {downloading === 'tax-word'
                  ? <Loader2 size={16} className="animate-spin mr-1.5" />
                  : <FileText size={16} className="mr-1.5" />
                }
                Tải Word
              </Button>
            </div>
          </div>
        </div>
      </Card>

      <ConfirmDialog
        open={pendingDl !== null}
        title="Xuất file?"
        description={`Bạn muốn tải file ${pendingDl?.filename ?? ''} về máy?`}
        icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Download size={24} className="text-blue-600" /></div>}
        confirmLabel="Tải xuống"
        loading={downloading !== null}
        onConfirm={() => pendingDl && doDownload(pendingDl)}
        onCancel={() => setPendingDl(null)}
      />
    </div>
  )
}
