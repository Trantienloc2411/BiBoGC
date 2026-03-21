import { NextRequest, NextResponse } from 'next/server'

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? ''

export async function POST(request: NextRequest) {
  const accessToken = request.cookies.get('accessToken')?.value

  // Best-effort: tell backend to revoke refresh tokens
  if (accessToken) {
    try {
      await fetch(`${API_URL}/api/auth/logout`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}` },
      })
    } catch {
      // ignore — we still clear cookies locally
    }
  }

  const response = NextResponse.json({ ok: true })
  response.cookies.delete('accessToken')
  response.cookies.delete('refreshToken')
  response.cookies.delete('role')

  return response
}
