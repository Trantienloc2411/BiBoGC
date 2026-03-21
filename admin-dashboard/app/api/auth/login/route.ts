import { NextRequest, NextResponse } from 'next/server'

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? ''

export async function POST(request: NextRequest) {
  const body = await request.json()

  let backendRes: Response
  try {
    backendRes = await fetch(`${API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
  } catch {
    return NextResponse.json({ message: 'Không thể kết nối đến máy chủ.' }, { status: 503 })
  }

  const data = await backendRes.json()

  if (!backendRes.ok) {
    return NextResponse.json(
      { message: data?.errors?.[0] ?? 'Đăng nhập thất bại.' },
      { status: backendRes.status }
    )
  }

  const { accessToken, refreshToken, role, userName, accessTokenExpirationAt } = data

  const response = NextResponse.json({ role, userName, accessTokenExpirationAt })

  // Store tokens in httpOnly cookies — JS cannot read these
  response.cookies.set('accessToken', accessToken, {
    httpOnly: false, // must be readable by JS to attach as Bearer header
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 15, // 15 minutes
  })

  response.cookies.set('refreshToken', refreshToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 60 * 24 * 7, // 7 days
  })

  // Role is non-sensitive — readable by JS for UI-level role checks
  response.cookies.set('role', role, {
    httpOnly: false,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 60 * 24 * 7,
  })

  return response
}
