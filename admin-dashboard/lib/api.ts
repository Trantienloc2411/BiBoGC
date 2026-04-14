const API_URL = process.env.NEXT_PUBLIC_API_URL ?? ''

let isRefreshing = false

function getAccessToken(): string | null {
  if (typeof document === 'undefined') return null
  const match = document.cookie.match(/(?:^|;\s*)accessToken=([^;]+)/)
  return match ? decodeURIComponent(match[1]) : null
}

export type ApiErrorDetail = {
  status: number
  statusText: string
  path: string
  /** Server's ProblemDetails title or generic fallback */
  title?: string
  /** Validation error list from ProblemDetails errors[] */
  errors?: string[]
}

type ApiErrorListener = (err: ApiErrorDetail) => void
const listeners = new Set<ApiErrorListener>()

export function onApiError(fn: ApiErrorListener) {
  listeners.add(fn)
  return () => { listeners.delete(fn) }
}

function emitError(detail: ApiErrorDetail) {
  listeners.forEach(fn => fn(detail))
}

interface ProblemDetails {
  title?: string
  detail?: string
  message?: string
  errors?: string[] | Record<string, string[]>
}

function parseProblemDetails(json: ProblemDetails): { title?: string; errors: string[] } {
  let errors: string[] = []
  if (Array.isArray(json.errors)) {
    errors = json.errors
  } else if (json.errors && typeof json.errors === 'object') {
    errors = Object.values(json.errors as Record<string, string[]>).flat()
  }
  const title = json.detail ?? json.message ?? json.title
  return { title, errors }
}

async function extractErrorDetail(res: Response): Promise<{ title?: string; errors: string[] }> {
  try {
    const json = await res.clone().json()
    return parseProblemDetails(json)
  } catch {
    return { errors: [] }
  }
}

async function apiFetch(path: string, options: RequestInit = {}): Promise<Response> {
  const token = getAccessToken()

  let res: Response
  try {
    res = await fetch(`${API_URL}${path}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...options.headers,
      },
    })
  } catch (err) {
    emitError({
      status: 0,
      statusText: 'Network Error',
      path,
      title: 'Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.',
    })
    throw err
  }

  if (res.status === 401 && !isRefreshing) {
    isRefreshing = true
    try {
      const refreshed = await fetch('/api/auth/refresh', { method: 'POST' })
      if (refreshed.ok) {
        isRefreshing = false
        return apiFetch(path, options)
      }
    } catch {
      // refresh failed
    }
    isRefreshing = false
    if (typeof window !== 'undefined') {
      window.location.href = '/login'
    }
  }

  if (!res.ok && res.status !== 401) {
    const { title, errors } = await extractErrorDetail(res)
    emitError({
      status: res.status,
      statusText: res.statusText,
      path,
      title,
      errors: errors.length > 0 ? errors : undefined,
    })
  }

  return res
}

async function apiDownload(path: string): Promise<{ blob: Blob; filename: string }> {
  const token = getAccessToken()

  let res: Response
  try {
    res = await fetch(`${API_URL}${path}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    })
  } catch (err) {
    emitError({
      status: 0,
      statusText: 'Network Error',
      path,
      title: 'Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.',
    })
    throw err
  }

  if (res.status === 401 && !isRefreshing) {
    isRefreshing = true
    try {
      const refreshed = await fetch('/api/auth/refresh', { method: 'POST' })
      if (refreshed.ok) {
        isRefreshing = false
        return apiDownload(path)
      }
    } catch { /* */ }
    isRefreshing = false
    if (typeof window !== 'undefined') window.location.href = '/login'
  }

  if (!res.ok) {
    const { title, errors } = await extractErrorDetail(res)
    emitError({
      status: res.status,
      statusText: res.statusText,
      path,
      title,
      errors: errors.length > 0 ? errors : undefined,
    })
    throw new Error(`Export failed: ${res.status}`)
  }

  const disposition = res.headers.get('Content-Disposition') ?? ''
  const match = disposition.match(/filename\*?=(?:UTF-8''|")?([^";,\s]+)"?/)
    || disposition.match(/filename=([^";,\s]+)/)
  const contentType = res.headers.get('Content-Type') ?? ''
  const ext = contentType.includes('pdf') ? '.pdf'
    : contentType.includes('spreadsheet') ? '.xlsx'
    : contentType.includes('wordprocessing') ? '.docx'
    : ''
  const fallback = `export-${new Date().toISOString().slice(0, 10)}${ext}`
  const filename = match?.[1] ?? fallback

  const blob = await res.blob()
  return { blob, filename }
}

export const api = {
  get: (path: string) =>
    apiFetch(path, { method: 'GET' }),

  post: (path: string, body?: unknown) =>
    apiFetch(path, {
      method: 'POST',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }),

  put: (path: string, body?: unknown) =>
    apiFetch(path, {
      method: 'PUT',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }),

  delete: (path: string) =>
    apiFetch(path, { method: 'DELETE' }),

  download: apiDownload,
}

// ─── Typed API helpers ────────────────────────────────────────────────────────

function buildQuery(params?: Record<string, unknown>): string {
  if (!params) return ''
  const qs = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== '') qs.set(k, String(v))
  }
  const str = qs.toString()
  return str ? `?${str}` : ''
}

export async function apiGet<T>(path: string, params?: Record<string, unknown>): Promise<T> {
  const res = await apiFetch(`${path}${buildQuery(params)}`, { method: 'GET' })
  if (!res.ok) throw new Error(`GET ${path} failed: ${res.status}`)
  const json = await res.json()
  return (json.data ?? json) as T
}

export async function apiPost<T>(path: string, body?: unknown): Promise<T> {
  const res = await apiFetch(path, {
    method: 'POST',
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })
  if (!res.ok) {
    const json = await res.json().catch(() => ({}))
    const { title, errors } = parseProblemDetails(json)
    throw new Error(errors.length > 0 ? errors.join('\n') : (title ?? `POST ${path} failed: ${res.status}`))
  }
  const json = await res.json()
  return (json.data ?? json) as T
}

export async function apiPut<T>(path: string, body?: unknown): Promise<T> {
  const res = await apiFetch(path, {
    method: 'PUT',
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })
  if (!res.ok) {
    const json = await res.json().catch(() => ({}))
    const { title, errors } = parseProblemDetails(json)
    throw new Error(errors.length > 0 ? errors.join('\n') : (title ?? `PUT ${path} failed: ${res.status}`))
  }
  const json = await res.json()
  return (json.data ?? json) as T
}

export async function apiDelete(path: string): Promise<void> {
  const res = await apiFetch(path, { method: 'DELETE' })
  if (!res.ok) {
    const json = await res.json().catch(() => ({}))
    const { title, errors } = parseProblemDetails(json)
    throw new Error(errors.length > 0 ? errors.join('\n') : (title ?? `DELETE ${path} failed: ${res.status}`))
  }
}

// ─── Inventory API ────────────────────────────────────────────────────────────

import type {
  PaginatedResult,
  ProductDto,
  CreateProductRequestV2,
  UpdateProductRequest,
  ProductBatchDtoV2,
  AddBatchRequest,
  UpdateBatchRequest,
  ProductVariantDtoV2,
  CreateVariantRequestV2,
  UpdateVariantRequest,
  CategoryDtoV2,
  CategoryTreeNode,
  CreateCategoryRequestV2,
  UpdateCategoryRequest,
  SupplierDtoV2,
  CreateSupplierRequestV2,
  UpdateSupplierRequest,
  StockTransactionDtoV2,
  AdjustStockRequestV2,
} from '@/types'

export const productApi = {
  list: (params: {
    pageNumber?: number; pageSize?: number; searchTerm?: string;
    categoryId?: string; supplierId?: string; status?: number;
    lowStockOnly?: boolean
  }) => apiGet<PaginatedResult<ProductDto>>('/api/products', params as Record<string, unknown>),
  getById: (id: string) => apiGet<ProductDto>(`/api/products/${id}`),
  create: (data: CreateProductRequestV2) => apiPost<ProductDto>('/api/products', data),
  update: (id: string, data: UpdateProductRequest) => apiPut<ProductDto>(`/api/products/${id}`, data),
  delete: (id: string) => apiDelete(`/api/products/${id}`),
  lowStock: () =>
    apiGet<ProductDto[]>('/api/products/low-stock'),
  expiredBatches: () =>
    apiGet<ProductDto[]>('/api/products/expired-batches'),
  expiringSoon: (thresholdDays?: number) =>
    apiGet<ProductDto[]>('/api/products/expiring-soon', thresholdDays ? { thresholdDays } : undefined),
  getBatches: (productId: string, params?: { pageNumber?: number; pageSize?: number; includeExpired?: boolean; sortByExpiry?: boolean }) =>
    apiGet<PaginatedResult<ProductBatchDtoV2>>(`/api/products/${productId}/batches`, params as Record<string, unknown> | undefined),
  addBatch: (productId: string, data: AddBatchRequest) =>
    apiPost<ProductBatchDtoV2>(`/api/products/${productId}/batches`, data),
  updateBatch: (productId: string, batchId: string, data: UpdateBatchRequest) =>
    apiPut<ProductBatchDtoV2>(`/api/products/${productId}/batches/${batchId}`, data),
  deleteBatch: (productId: string, batchId: string) =>
    apiDelete(`/api/products/${productId}/batches/${batchId}`),
  getVariants: (productId: string) =>
    apiGet<ProductVariantDtoV2[]>(`/api/products/${productId}/variants`),
  createVariant: (productId: string, data: CreateVariantRequestV2) =>
    apiPost<ProductVariantDtoV2>(`/api/products/${productId}/variants`, data),
  updateVariant: (productId: string, variantId: string, data: UpdateVariantRequest) =>
    apiPut<ProductVariantDtoV2>(`/api/products/${productId}/variants/${variantId}`, data),
  deleteVariant: (productId: string, variantId: string) =>
    apiDelete(`/api/products/${productId}/variants/${variantId}`),
}

export const categoryApi = {
  list: (params?: { includeInactive?: boolean; parentCategoryId?: string }) =>
    apiGet<PaginatedResult<CategoryDtoV2>>('/api/categories', params as Record<string, unknown> | undefined),
  getById: (id: string) => apiGet<CategoryDtoV2>(`/api/categories/${id}`),
  create: (data: CreateCategoryRequestV2) => apiPost<CategoryDtoV2>('/api/categories', data),
  update: (id: string, data: UpdateCategoryRequest) => apiPut<CategoryDtoV2>(`/api/categories/${id}`, data),
  delete: (id: string) => apiDelete(`/api/categories/${id}`),
  /** Full category tree from optimised endpoint — no N+1, includes productCount */
  tree: (params?: { includeInactive?: boolean }) =>
    apiGet<CategoryTreeNode[]>('/api/categories/tree', params as Record<string, unknown> | undefined),
  /** Subtree rooted at a given node */
  subtree: (id: string, params?: { includeInactive?: boolean }) =>
    apiGet<CategoryTreeNode>(`/api/categories/${id}/tree`, params as Record<string, unknown> | undefined),
}

export const supplierApi = {
  list: (params: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) =>
    apiGet<PaginatedResult<SupplierDtoV2>>('/api/suppliers', params as Record<string, unknown>),
  getById: (id: string) => apiGet<SupplierDtoV2>(`/api/suppliers/${id}`),
  create: (data: CreateSupplierRequestV2) => apiPost<SupplierDtoV2>('/api/suppliers', data),
  update: (id: string, data: UpdateSupplierRequest) => apiPut<SupplierDtoV2>(`/api/suppliers/${id}`, data),
  delete: (id: string) => apiDelete(`/api/suppliers/${id}`),
  activate: (id: string) => apiPost<SupplierDtoV2>(`/api/suppliers/${id}/activate`),
  deactivate: (id: string) => apiPost<SupplierDtoV2>(`/api/suppliers/${id}/deactivate`),
}

export const stockTransactionApi = {
  // URL is /api/stocktransactions (no hyphen) per Postman
  list: (params: {
    pageNumber?: number; pageSize?: number; productId?: string;
    transactionType?: string; fromDate?: string; toDate?: string;
    supplierId?: string; sortBy?: string; sortDescending?: boolean
  }) => apiGet<PaginatedResult<StockTransactionDtoV2>>('/api/stocktransactions', params as Record<string, unknown>),
  getById: (id: string) => apiGet<StockTransactionDtoV2>(`/api/stocktransactions/${id}`),
  // Adjustment endpoint per Postman: POST /api/stocktransactions/adjustment
  adjust: (data: AdjustStockRequestV2) => apiPost<StockTransactionDtoV2>('/api/stocktransactions/adjustment', data),
  // Purchase shortcut per Postman
  purchase: (data: { productId: string; productBatchId?: string; supplierId?: string; quantity: number; unitPrice: number; notes?: string }) =>
    apiPost<StockTransactionDtoV2>('/api/stocktransactions/purchase', data),
  // Sale shortcut per Postman
  sale: (data: { productId: string; productBatchId?: string; quantity: number; unitPrice: number; notes?: string }) =>
    apiPost<StockTransactionDtoV2>('/api/stocktransactions/sale', data),
}
