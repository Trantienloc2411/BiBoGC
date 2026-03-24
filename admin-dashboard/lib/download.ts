import { api } from './api'

export async function downloadFile(path: string, fallbackFilename?: string) {
  const { blob, filename } = await api.download(path)
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fallbackFilename ?? filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
