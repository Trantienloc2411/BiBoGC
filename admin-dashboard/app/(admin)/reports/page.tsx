'use client'

import { useState } from 'react'
import { cn } from '@/lib/utils'
import { DailyReport } from '@/components/reports/DailyReport'
import { MonthlyReport } from '@/components/reports/MonthlyReport'
import { FinancialReport } from '@/components/reports/FinancialReport'
import { AnnualReport } from '@/components/reports/AnnualReport'
import { FileSpreadsheet, Download, Info } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { useExportFile } from '@/hooks/useExportFile'
import { exportTaxReport, exportExistingProducts } from '@/lib/exportService'

const TABS = [
  { id: 'daily',     label: 'Hôm nay' },
  { id: 'monthly',   label: 'Tháng' },
  { id: 'annual',    label: 'Năm' },
  { id: 'financial', label: 'Tài chính' },
  { id: 'export',    label: 'Xuất file' },
] as const

type TabId = typeof TABS[number]['id']

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<TabId>('daily')

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-4">
        <h1 className="text-xl font-bold text-gray-800">Báo cáo</h1>

        <div className="flex bg-gray-100 rounded-md p-1 gap-1">
          {TABS.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                'px-4 py-2 text-sm font-medium rounded-md transition-all cursor-pointer',
                activeTab === tab.id
                  ? 'bg-white text-blue-600 shadow-sm'
                  : 'text-gray-500 hover:text-gray-700'
              )}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {activeTab === 'daily'     && <DailyReport />}
      {activeTab === 'monthly'   && <MonthlyReport />}
      {activeTab === 'annual'    && <AnnualReport />}
      {activeTab === 'financial' && <FinancialReport />}
      {activeTab === 'export'    && <ExportTab />}
    </div>
  )
}

// ─── Export Tab ───────────────────────────────────────────────────────────────

function ExportTab() {
  const { exportFile: exportTax,      loading: taxLoading }      = useExportFile(exportTaxReport)
  const { exportFile: exportProducts, loading: productsLoading } = useExportFile(exportExistingProducts)

  return (
    <div className="space-y-6">
      <p className="text-sm text-gray-500">
        Chọn mẫu cần xuất. File trả về sẽ là <span className="font-medium">.xlsx</span> hoặc{' '}
        <span className="font-medium">.zip</span> tuỳ theo số lượng dữ liệu.
      </p>

      <div className="grid gap-4 md:grid-cols-2">
        {/* Tax report */}
        <Card className="flex flex-col gap-5">
          <div className="flex items-start gap-4">
            <div className="w-11 h-11 rounded-xl bg-blue-50 flex items-center justify-center shrink-0">
              <FileSpreadsheet size={22} className="text-blue-600" />
            </div>
            <div>
              <p className="font-semibold text-gray-800 text-sm">Sổ chi tiết doanh thu (S2a-HKD)</p>
              <p className="text-sm text-gray-500 mt-0.5 leading-relaxed">
                Xuất báo cáo thuế đầu ra theo mẫu S2a-HKD.xlsx. Dùng để kê khai thuế hộ kinh doanh.
              </p>
            </div>
          </div>
          <Button
            onClick={exportTax}
            loading={taxLoading}
            disabled={taxLoading}
            className="gap-2 self-start"
          >
            <Download size={16} />
            {taxLoading ? 'Đang xuất...' : 'Xuất S2a-HKD'}
          </Button>
        </Card>

        {/* Existing products */}
        <Card className="flex flex-col gap-5">
          <div className="flex items-start gap-4">
            <div className="w-11 h-11 rounded-xl bg-green-50 flex items-center justify-center shrink-0">
              <FileSpreadsheet size={22} className="text-green-600" />
            </div>
            <div>
              <p className="font-semibold text-gray-800 text-sm">Biên bản kiểm kê hàng tồn kho</p>
              <p className="text-sm text-gray-500 mt-0.5 leading-relaxed">
                Xuất toàn bộ hàng tồn kho hiện tại theo mẫu ProductExists.xlsx.
              </p>
            </div>
          </div>
          <Button
            onClick={exportProducts}
            loading={productsLoading}
            disabled={productsLoading}
            variant="secondary"
            className="gap-2 self-start"
          >
            <Download size={16} />
            {productsLoading ? 'Đang xuất...' : 'Xuất hàng tồn kho'}
          </Button>
        </Card>

        {/* Info */}
        <Card className="md:col-span-2 bg-blue-50/60 border-blue-100">
          <div className="flex gap-3">
            <Info size={18} className="text-blue-500 shrink-0 mt-0.5" />
            <div className="space-y-1.5 text-sm text-gray-600">
              <p className="font-semibold text-gray-700">Lưu ý khi xuất file</p>
              <ul className="space-y-1 list-disc list-inside">
                <li>File xuất ra có thể là <span className="font-medium">.xlsx</span> hoặc <span className="font-medium">.zip</span> tuỳ vào số lượng dữ liệu.</li>
                <li>Tên file được lấy tự động từ server.</li>
                <li>Không tắt trình duyệt trong khi đang xuất.</li>
                <li>Nếu tải thất bại, vui lòng thử lại sau ít giây.</li>
              </ul>
            </div>
          </div>
        </Card>
      </div>
    </div>
  )
}
