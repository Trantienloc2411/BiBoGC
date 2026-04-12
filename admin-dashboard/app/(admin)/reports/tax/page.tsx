'use client'

import { FileSpreadsheet, Download, Info } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { useExportFile } from '@/hooks/useExportFile'
import { exportTaxReport } from '@/lib/exportService'

export default function TaxReportPage() {
  const { exportFile, loading } = useExportFile(exportTaxReport)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-bold text-gray-800">Báo cáo thuế</h1>
          <p className="text-sm text-gray-500 mt-1">Xuất dữ liệu báo cáo thuế ra file Excel / ZIP</p>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {/* Export card */}
        <Card className="flex flex-col gap-5">
          <div className="flex items-start gap-4">
            <div className="w-11 h-11 rounded-xl bg-blue-50 flex items-center justify-center shrink-0">
              <FileSpreadsheet size={22} className="text-blue-600" />
            </div>
            <div>
              <p className="font-semibold text-gray-800 text-sm">Xuất báo cáo thuế</p>
              <p className="text-sm text-gray-500 mt-0.5 leading-relaxed">
                Tải xuống file báo cáo thuế đầu ra (S2a-HKD) dưới dạng Excel hoặc ZIP nếu có nhiều
                file.
              </p>
            </div>
          </div>

          <Button
            onClick={exportFile}
            loading={loading}
            disabled={loading}
            className="gap-2 self-start"
          >
            <Download size={16} />
            {loading ? 'Đang xuất...' : 'Xuất báo cáo thuế'}
          </Button>
        </Card>

        {/* Info card */}
        <Card className="bg-blue-50/60 border-blue-100">
          <div className="flex gap-3">
            <Info size={18} className="text-blue-500 shrink-0 mt-0.5" />
            <div className="space-y-1.5 text-sm text-gray-600">
              <p className="font-semibold text-gray-700">Lưu ý khi xuất báo cáo</p>
              <ul className="space-y-1 list-disc list-inside">
                <li>File xuất ra có thể là <span className="font-medium">.xlsx</span> hoặc <span className="font-medium">.zip</span> tuỳ vào dữ liệu.</li>
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
