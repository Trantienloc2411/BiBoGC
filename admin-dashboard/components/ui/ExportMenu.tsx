'use client'

import { useState } from 'react'
import { Download, FileText, FileSpreadsheet, Loader2 } from 'lucide-react'
import { downloadFile } from '@/lib/download'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { useToast } from '@/components/ui/Toast'

interface ExportOption {
  label: string
  icon: React.ReactNode
  path: string
  filename?: string
}

interface ExportMenuProps {
  options: ExportOption[]
}

export function ExportMenu({ options }: ExportMenuProps) {
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState<string | null>(null)
  const [pendingExport, setPendingExport] = useState<ExportOption | null>(null)
  const { error: showError } = useToast()

  async function doExport(opt: ExportOption) {
    setLoading(opt.path)
    setPendingExport(null)
    try {
      await downloadFile(opt.path, opt.filename)
    } catch {
      showError('Xuất file thất bại', 'Không thể tải file từ server. Vui lòng thử lại.')
    } finally {
      setLoading(null)
      setOpen(false)
    }
  }

  function handleClick(opt: ExportOption) {
    setOpen(false)
    setPendingExport(opt)
  }

  if (options.length === 0) return null

  if (options.length === 1) {
    const opt = options[0]
    return (
      <>
        <button
          onClick={() => handleClick(opt)}
          disabled={loading !== null}
          className="inline-flex items-center gap-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 px-3.5 py-2 rounded-md shadow-sm transition-colors disabled:opacity-50"
        >
          {loading ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
          Xuất file
        </button>
        <ConfirmDialog
          open={pendingExport !== null}
          title="Xuất file?"
          description={`Bạn muốn tải file ${pendingExport?.filename ?? pendingExport?.label ?? ''} về máy?`}
          icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Download size={24} className="text-blue-600" /></div>}
          confirmLabel="Tải xuống"
          loading={loading !== null}
          onConfirm={() => pendingExport && doExport(pendingExport)}
          onCancel={() => setPendingExport(null)}
        />
      </>
    )
  }

  return (
    <>
      <div className="relative">
        <button
          onClick={() => setOpen(v => !v)}
          className="inline-flex items-center gap-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 px-3.5 py-2 rounded-md shadow-sm transition-colors"
        >
          <Download size={16} />
          Xuất file
        </button>

        {open && (
          <>
            <div className="fixed inset-0 z-20" onClick={() => setOpen(false)} />
            <div className="absolute right-0 top-full mt-1 z-30 bg-white border border-gray-200 rounded-lg shadow-lg py-1.5 min-w-[180px]">
              {options.map(opt => (
                <button
                  key={opt.path}
                  onClick={() => handleClick(opt)}
                  disabled={loading !== null}
                  className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                >
                  {loading === opt.path
                    ? <Loader2 size={16} className="animate-spin text-gray-400" />
                    : opt.icon}
                  {opt.label}
                </button>
              ))}
            </div>
          </>
        )}
      </div>

      <ConfirmDialog
        open={pendingExport !== null}
        title="Xuất file?"
        description={`Bạn muốn tải file ${pendingExport?.filename ?? pendingExport?.label ?? ''} về máy?`}
        icon={<div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center"><Download size={24} className="text-blue-600" /></div>}
        confirmLabel="Tải xuống"
        loading={loading !== null}
        onConfirm={() => pendingExport && doExport(pendingExport)}
        onCancel={() => setPendingExport(null)}
      />
    </>
  )
}

export function pdfIcon() { return <FileText size={16} className="text-red-500" /> }
export function excelIcon() { return <FileSpreadsheet size={16} className="text-green-600" /> }
export function wordIcon() { return <FileText size={16} className="text-blue-600" /> }
