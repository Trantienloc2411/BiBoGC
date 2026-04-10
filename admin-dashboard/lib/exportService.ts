import { downloadFile } from './download'

export const exportTaxReport = () =>
  downloadFile('/api/finance/reports/tax/export')

export const exportExistingProducts = () =>
  downloadFile('/api/products/export-existing')

export async function exportInvoicesZip(dateFrom: string, dateTo: string) {
  const params = new URLSearchParams()
  if (dateFrom) params.set('dateFrom', dateFrom)
  if (dateTo) params.set('dateTo', dateTo)
  const query = params.toString()
  await downloadFile(
    `/api/invoices/export/zip${query ? `?${query}` : ''}`,
    `hoa-don${dateFrom ? `_${dateFrom}` : ''}${dateTo ? `_den_${dateTo}` : ''}.zip`,
  )
}
