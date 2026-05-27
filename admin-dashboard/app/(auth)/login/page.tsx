"use client";

import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import Image from "next/image";
import { Button } from "@/components/ui/Button";

const FEATURES = [
  { emoji: "📦", label: "Quản lý kho & hàng hóa" },
  { emoji: "🧾", label: "Xử lý đơn hàng & hoá đơn" },
  { emoji: "📊", label: "Báo cáo doanh thu tức thì" },
];

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    if (!username.trim() || !password.trim()) {
      setError("Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
      return;
    }

    setLoading(true);
    try {
      const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName: username, password }),
      });

      const data = await res.json();

      if (!res.ok) {
        setError(data.message ?? "Đăng nhập thất bại.");
        return;
      }

      window.location.href = "/dashboard";
    } catch {
      setError("Không thể kết nối đến máy chủ. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen flex">
      {/* ── Left brand panel (desktop only) ───────────────────────────────── */}
      <div className="hidden lg:flex lg:w-[46%] bg-[#0F172A] flex-col justify-between p-12 relative overflow-hidden select-none">
        {/* Dot grid texture */}
        <div
          className="absolute inset-0 opacity-[0.035]"
          style={{
            backgroundImage:
              "radial-gradient(circle, #ffffff 1px, transparent 1px)",
            backgroundSize: "28px 28px",
          }}
        />
        {/* Glow blobs */}
        <div className="absolute top-0 right-0 w-80 h-80 rounded-full bg-green-500 opacity-[0.06] translate-x-40 -translate-y-40 blur-3xl" />
        <div className="absolute bottom-0 left-0 w-96 h-96 rounded-full bg-blue-600 opacity-[0.08] -translate-x-48 translate-y-48 blur-3xl" />

        {/* Logo */}
        <div className="relative z-10">
          <Image
            src="/logo/bibo-gc-monochrome-light.svg"
            alt="BiBo's GC"
            width={180}
            height={36}
            priority
            unoptimized
          />
        </div>

        {/* Headline + features */}
        <div className="relative z-10">
          <h1 className="text-[2.5rem] font-bold text-white leading-[1.2] tracking-tight">
            Quản lý cửa hàng
            <br />
            <span className="text-green-400">thông minh hơn</span>
          </h1>
          <p className="text-slate-400 mt-4 text-[15px] leading-relaxed max-w-[300px]">
            Hệ thống quản lý tạp hóa toàn diện — kho hàng, đơn hàng và báo cáo
            tài chính trong một nơi.
          </p>

          <ul className="mt-10 space-y-3.5">
            {FEATURES.map((f) => (
              <li key={f.label} className="flex items-center gap-3.5">
                <span className="w-9 h-9 rounded-xl bg-white/[0.06] flex items-center justify-center text-[17px] shrink-0">
                  {f.emoji}
                </span>
                <span className="text-slate-300 text-sm">{f.label}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Footer */}
        <p className="relative z-10 text-slate-600 text-xs">
          © 2025 BiBo&apos;s GC · All rights reserved
        </p>
      </div>

      {/* ── Right form panel ───────────────────────────────────────────────── */}
      <div className="flex-1 flex items-center justify-center bg-slate-50 px-6 py-12">
        <div className="w-full max-w-[360px]">
          {/* Mobile-only logo */}
          <div className="lg:hidden flex flex-col items-center mb-10 gap-3">
            <Image
              src="/logo/bibo-gc-app-icon-96.svg"
              alt="BiBo's GC"
              width={72}
              height={72}
              className="rounded-2xl shadow-md"
              priority
              unoptimized
            />
            <span className="text-sm text-slate-500 font-medium">
              BiBo&apos;s GC Admin
            </span>
          </div>

          <h2 className="text-2xl font-bold text-gray-900 tracking-tight">
            Đăng nhập
          </h2>
          <p className="text-gray-400 text-sm mt-1 mb-8">
            Chào mừng trở lại — vui lòng đăng nhập để tiếp tục
          </p>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-[11px] font-semibold text-gray-400 uppercase tracking-widest mb-1.5">
                Tài khoản
              </label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Nhập tên tài khoản"
                autoComplete="username"
                autoFocus
                className="w-full px-4 py-3 rounded-xl border border-gray-200 bg-white text-gray-800 placeholder-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition shadow-sm"
              />
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-gray-400 uppercase tracking-widest mb-1.5">
                Mật khẩu
              </label>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Nhập mật khẩu"
                  autoComplete="current-password"
                  className="w-full px-4 py-3 pr-11 rounded-xl border border-gray-200 bg-white text-gray-800 placeholder-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition shadow-sm"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-300 hover:text-gray-500 transition"
                >
                  {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
                </button>
              </div>
            </div>

            {error && (
              <div className="flex items-start gap-2.5 bg-red-50 border border-red-100 rounded-xl px-3.5 py-3">
                <span className="text-red-400 text-sm mt-0.5 shrink-0">⚠</span>
                <p className="text-sm text-red-600 leading-snug">{error}</p>
              </div>
            )}

            <div className="pt-1">
              <Button
                type="submit"
                size="lg"
                loading={loading}
                className="!rounded-xl"
              >
                {loading ? "Đang đăng nhập..." : "Đăng nhập"}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
