import { NextRequest, NextResponse } from 'next/server'

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? ''

export async function POST(request: NextRequest) {
  const refreshToken = request.cookies.get('refreshToken')?.value

  if (!refreshToken) {
    return NextResponse.json({ message: 'No refresh token' }, { status: 401 })
  }

  let backendRes: Response
  try {
    backendRes = await fetch(`${API_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
  } catch {
    return NextResponse.json({ message: 'Không thể kết nối đến máy chủ.' }, { status: 503 })
  }

  if (!backendRes.ok) {
    // Refresh failed — clear cookies, force re-login
    const res = NextResponse.json({ message: 'Session hết hạn.' }, { status: 401 })
    res.cookies.delete('accessToken')
    res.cookies.delete('refreshToken')
    res.cookies.delete('role')
    return res
  }

  const data = await backendRes.json()
  const { accessToken, refreshToken: newRefreshToken, role } = data

  const response = NextResponse.json({ ok: true })

  response.cookies.set('accessToken', accessToken, {
    httpOnly: false, // must be readable by JS to attach as Bearer header
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 15,
  })

  response.cookies.set('refreshToken', newRefreshToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 60 * 24 * 7,
  })

  response.cookies.set('role', role, {
    httpOnly: false,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 60 * 24 * 7,
  })

  return response
}
