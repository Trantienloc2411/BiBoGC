[![CI/CD .NET Build & Test](https://github.com/Trantienloc2411/BiBo-s-GC/actions/workflows/Build-Test.yml/badge.svg)](https://github.com/Trantienloc2411/BiBo-s-GC/actions/workflows/Build-Test.yml)
[![Build Flutter APK](https://github.com/Trantienloc2411/BiBo-s-GC/actions/workflows/Build-Apk.yml/badge.svg)](https://github.com/Trantienloc2411/BiBo-s-GC/actions/workflows/Build-Apk.yml)

# BiBo's Grocery Store Management System

**BiBo's GC** (BiBo's Grocery Controller) là hệ thống quản lý cửa hàng tạp hoá toàn diện, bao gồm backend API, ứng dụng mobile Flutter, và admin dashboard web. Hệ thống được xây dựng theo kiến trúc Clean Architecture với .NET 10.

### Mục tiêu

- Quản lý tồn kho hiệu quả với chiến lược FEFO (First Expiry, First Out)
- Xử lý đơn hàng bán hàng và xuất hoá đơn
- Theo dõi thuế và báo cáo doanh thu/chi phí
- Hỗ trợ quét mã vạch và xác thực sinh trắc học trên mobile

---

## Tech Stack

| Component        | Technology                                          |
| ---------------- | --------------------------------------------------- |
| Backend API      | .NET 10, ASP.NET Core Web API                       |
| Database         | PostgreSQL 16, Entity Framework Core 10             |
| Architecture     | Clean Architecture, CQRS, MediatR                   |
| Orchestration    | .NET Aspire                                         |
| Containerization | Docker                                              |
| Mobile App       | Flutter (Dart), BLoC, go_router, Dio                |
| Admin Dashboard  | Next.js 16, React 19, TypeScript, Tailwind CSS      |
| CI/CD            | GitHub Actions                                      |

---

## Cấu trúc Repository

```
BiBo-s-GC/
├── BiBoGC/                              # Backend Solution (.NET 10)
│   ├── BiBoGC/                          # API Layer — Controllers, Middleware
│   ├── BiBoGC.AppHost/                  # .NET Aspire Host
│   ├── BiBoGC.ServiceDefaults/          # Shared service configurations
│   │
│   ├── InventoryManagement.Domain/      # Domain entities, enums, events
│   ├── InventoryManagement.Application/ # CQRS Commands/Queries, DTOs
│   ├── InventoryManagement.Infrastructure/ # DbContext, Repositories
│   │
│   ├── Sale.Domain/                     # Sales domain
│   ├── Sale.Application/
│   ├── Sale.Infrastructure/
│   │
│   ├── Finance.Domain/                  # Finance & invoicing domain
│   ├── Finance.Application/
│   ├── Finance.Infrastructure/
│   │
│   ├── AuthorizationModule.*            # Auth domain
│   ├── Notification.*                   # Notification domain
│   ├── Catalog.Domain/                  # Product catalog
│   │
│   ├── Shared.Domain/                   # Shared domain components
│   ├── Shared.Application/              # Shared application components
│   ├── Shared.Contracts/                # Integration contracts
│   └── Tests/                           # Unit & integration tests
│
├── FrontEnd/bibogc/                     # Flutter Mobile App
│   ├── lib/                             # Dart source code
│   └── android/                         # Android build config (signed release)
│
├── admin-dashboard/                     # Next.js Admin Web App
│   ├── app/                             # Next.js App Router pages
│   ├── components/                      # Reusable UI components
│   └── hooks/                           # Custom React hooks
│
└── .github/workflows/                   # CI/CD Pipelines
    ├── Build-Test.yml                   # .NET build & test
    ├── Build-Apk.yml                    # Flutter APK build & GitHub Release
    └── deploy.yml                       # Server deployment via Cloudflare
```

---

## Phases Development

### Phase 1: Inventory Management ✅ Complete

- [x] Product CRUD & category management
- [x] Supplier CRUD
- [x] Product batch management (expiry date tracking)
- [x] Stock transactions (Purchase, Sale, Adjustment)
- [x] FEFO batch deduction strategy
- [x] Product status updates
- [x] Export product data to Excel/ZIP

### Phase 2: Sales Management ✅ Complete

- [x] Sales orders with order rollback support
- [x] Invoice generation & ZIP export
- [x] Tax handling & export
- [x] Daily/Monthly revenue reports

### Phase 3: Mobile App ✅ In Progress

- [x] Flutter mobile app with BLoC state management
- [x] Biometric authentication (fingerprint/face)
- [x] Barcode scanning (`mobile_scanner`)
- [x] PDF invoice printing (`printing`)
- [x] Network routing & deep link support
- [x] Signed release APK via GitHub Actions CI/CD

### Phase 4: Admin Dashboard (In Progress)

- [x] Next.js admin web app
- [x] Revenue charts (Recharts)
- [ ] Full CRUD management UI
- [ ] Real-time notifications

---

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- Flutter SDK (stable channel)
- Node.js 20+

### Backend — Run with .NET Aspire

```bash
cd BiBoGC
dotnet run --project BiBoGC.AppHost
```

- API: `http://localhost:5020`
- Aspire Dashboard: `https://localhost:17062`

### Mobile App — Run Flutter

```bash
cd FrontEnd/bibogc
flutter pub get
flutter run
```

### Admin Dashboard — Run Next.js

```bash
cd admin-dashboard
npm install
npm run dev
```

- Dashboard: `http://localhost:3000`

---

## CI/CD Pipelines

| Workflow | Trigger | What it does |
|---|---|---|
| `Build-Test.yml` | Push / PR to `main` | Build & test .NET backend |
| `Build-Apk.yml` | Push to `main`/`development`, tag `v*.*.*` | Build signed Flutter APK; publish to GitHub Releases on tag |
| `deploy.yml` | Push to `main` | Deploy backend to server via Cloudflare SSH |

### Publishing a Mobile Release

```bash
git tag v1.0.0
git push origin v1.0.0
```

This triggers `Build-Apk.yml` which builds a signed APK and creates a GitHub Release automatically.

#### Required GitHub Secrets for APK signing

| Secret | Description |
|---|---|
| `KEYSTORE_BASE64` | Base64-encoded `.jks` keystore file |
| `STORE_PASSWORD` | Keystore password |
| `KEY_ALIAS` | Key alias |
| `KEY_PASSWORD` | Key password |

---

## Documentation

| Document | Description |
|---|---|
| [Architecture](./Documentation/Architecture.md) | System architecture & design patterns |
| [API Reference](./Documentation/API-Reference.md) | API endpoints & usage |
| [Database Schema](./Documentation/Database-Schema.md) | Entity relationships & tables |
| [Development Guide](./Documentation/Development-Guide.md) | Setup & contribution guide |

---

## Contact

- **Project Lead**: Tran Tien Loc
- **Repository**: [https://github.com/Trantienloc2411/BiBo-s-GC](https://github.com/Trantienloc2411/BiBo-s-GC)

---

_Last updated: April 2026_
