const API_URL = process.env.NEXT_PUBLIC_API_URL ?? ''

let isRefreshing = false

function getAccessToken(): string | null {
  if (typeof document === 'undefined') return null
  const match = document.cookie.match(/(?:^|;\s*)accessToken=([^;]+)/)
  return match ? decodeURIComponent(match[1]) : null
}

async function apiFetch(path: string, options: RequestInit = {}): Promise<Response> {
  const token = getAccessToken()

  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })

  // Auto-refresh on 401
  if (res.status === 401 && !isRefreshing) {
    isRefreshing = true
    try {
      const refreshed = await fetch('/api/auth/refresh', { method: 'POST' })
      if (refreshed.ok) {
        isRefreshing = false
        return apiFetch(path, options) // retry with new token
      }
    } catch {
      // refresh failed
    }
    isRefreshing = false
    if (typeof window !== 'undefined') {
      window.location.href = '/login'
    }
  }

  return res
}

export const api = {
  get: (path: string) =>
    apiFetch(path, { method: 'GET' }),

  post: (path: string, body?: unknown) =>
    apiFetch(path, {
      method: 'POST',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }),
}
