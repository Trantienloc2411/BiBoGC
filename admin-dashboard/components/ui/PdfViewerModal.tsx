'use client'

import { useEffect, useState } from 'react'
import { X, Download, Loader2 } from 'lucide-react'
import { api } from '@/lib/api'

interface PdfViewerModalProps {
  open: boolean
  title: string
  pdfUrl: string
  downloadFilename: string
  onClose: () => void
}

export function PdfViewerModal({ open, title, pdfUrl, downloadFilename, onClose }: PdfViewerModalProps) {
  const [objectUrl, setObjectUrl] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!open) return
    let revoke: string | null = null

    async function load() {
      setLoading(true)
      setError(null)
      try {
        const { blob } = await api.download(pdfUrl)
        const url = URL.createObjectURL(blob)
        revoke = url
        setObjectUrl(url)
      } catch {
        setError('Không thể tải PDF.')
      } finally {
        setLoading(false)
      }
    }

    load()
    return () => {
      if (revoke) URL.revokeObjectURL(revoke)
      setObjectUrl(null)
    }
  }, [open, pdfUrl])

  if (!open) return null

  function handleDownload() {
    if (!objectUrl) return
    const a = document.createElement('a')
    a.href = objectUrl
    a.download = downloadFilename
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
  }

  return (
    <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-4xl h-[85vh] flex flex-col">
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-gray-200">
          <h3 className="text-base font-semibold text-gray-800 truncate">{title}</h3>
          <div className="flex items-center gap-2">
            <button
              onClick={handleDownload}
              disabled={!objectUrl}
              className="p-2 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-md transition-colors disabled:opacity-40"
              title="Tải xuống"
            >
              <Download size={18} />
            </button>
            <button
              onClick={onClose}
              className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-md transition-colors"
            >
              <X size={18} />
            </button>
          </div>
        </div>
        <div className="flex-1 overflow-hidden bg-gray-100">
          {loading && (
            <div className="flex items-center justify-center h-full">
              <Loader2 size={24} className="animate-spin text-gray-400" />
            </div>
          )}
          {error && (
            <div className="flex items-center justify-center h-full text-sm text-gray-500">
              {error}
            </div>
          )}
          {objectUrl && (
            <iframe src={objectUrl} className="w-full h-full border-0" title={title} />
          )}
        </div>
      </div>
    </div>
  )
}
