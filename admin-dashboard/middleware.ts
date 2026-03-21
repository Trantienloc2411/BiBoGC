import { NextRequest, NextResponse } from 'next/server'

const PUBLIC_PATHS = ['/login']
const API_PATHS    = ['/api/']

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl

  // Always allow API routes and Next.js internals
  if (API_PATHS.some(p => pathname.startsWith(p))) return NextResponse.next()

  const token = request.cookies.get('accessToken')?.value
  const isPublic = PUBLIC_PATHS.some(p => pathname === p || pathname.startsWith(p + '/'))

  // Not logged in → redirect to /login
  if (!isPublic && !token) {
    const url = request.nextUrl.clone()
    url.pathname = '/login'
    return NextResponse.redirect(url)
  }

  // Already logged in → redirect away from /login
  if (isPublic && token) {
    const url = request.nextUrl.clone()
    url.pathname = '/dashboard'
    return NextResponse.redirect(url)
  }

  return NextResponse.next()
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
}
