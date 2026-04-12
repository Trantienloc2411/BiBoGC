'use client'

import { useState, useCallback, useRef, useEffect } from 'react'
import { useToast } from '@/components/ui/Toast'

/**
 * Wraps any async export call with loading state, double-click prevention,
 * and a success toast. Errors are handled globally via onApiError → ToastProvider.
 *
 * @example
 *   const { exportFile, loading } = useExportFile(exportTaxReport)
 *   <button onClick={exportFile} disabled={loading}>Export</button>
 */
export function useExportFile(apiCall: () => Promise<void>) {
  const [loading, setLoading] = useState(false)
  const { success } = useToast()

  // Keep latest apiCall reference without resetting exportFile identity
  const apiCallRef = useRef(apiCall)
  useEffect(() => { apiCallRef.current = apiCall }, [apiCall])

  // Use a ref to guard against double-click races (state update is async)
  const loadingRef = useRef(false)

  const exportFile = useCallback(async () => {
    if (loadingRef.current) return
    loadingRef.current = true
    setLoading(true)
    try {
      await apiCallRef.current()
      success('Tải xuống thành công', 'File đã được tải về máy của bạn.')
    } catch {
      // Server / network errors are already surfaced via onApiError → ToastProvider
    } finally {
      loadingRef.current = false
      setLoading(false)
    }
  }, [success])

  return { exportFile, loading }
}
