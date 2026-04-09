import { downloadFile } from './download'

export const exportTaxReport = () =>
  downloadFile('/api/finance/reports/tax/export')

export const exportExistingProducts = () =>
  downloadFile('/api/products/export-existing')
