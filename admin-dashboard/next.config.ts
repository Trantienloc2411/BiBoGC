import type { NextConfig } from 'next'

const nextConfig: NextConfig = {
  // Allow images from API domain if needed
  output: 'standalone',
  images: {
    dangerouslyAllowSVG: true,
    contentDispositionType: 'attachment',
    contentSecurityPolicy: "default-src 'self'; script-src 'none'; sandbox;",
  },
}


export default nextConfig
