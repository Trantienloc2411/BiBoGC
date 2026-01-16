# BiBo's Grocery Store Management System

## Tổng quan dự án

**BiBo's GC** (BiBo's Grocery Controller) là hệ thống quản lý cửa hàng tạp hoá được xây dựng với kiến trúc Clean Architecture, sử dụng .NET 10 và PostgreSQL.

### Mục tiêu

- Quản lý tồn kho hiệu quả cho cửa hàng tạp hoá nhỏ
- Theo dõi hạn sử dụng sản phẩm (FEFO - First Expiry, First Out)
- Quản lý nhà cung cấp và giao dịch nhập/xuất kho
- Hỗ trợ báo cáo doanh thu và chi phí

### Tech Stack

| Component        | Technology                    |
| ---------------- | ----------------------------- |
| Backend          | .NET 10, ASP.NET Core Web API |
| Database         | PostgreSQL 16                 |
| ORM              | Entity Framework Core 10      |
| Architecture     | Clean Architecture, CQRS      |
| Orchestration    | .NET Aspire                   |
| Containerization | Docker                        |

---

## Cấu trúc Solution

```
BiBoGC/
├── BiBoGC/                          # API Layer (Presentation)
│   ├── Controllers/                 # API Controllers
│   ├── Middleware/                  # Custom middlewares
│   └── Models/                      # API response models
│
├── BiBoGC.AppHost/                  # .NET Aspire Host
│
├── BiBoGC.ServiceDefaults/          # Shared service configurations
│
├── InventoryManagement.Domain/      # Domain Layer
│   ├── Entities/                    # Domain entities
│   ├── Enums/                       # Domain enumerations
│   ├── Events/                      # Domain events
│   ├── Exceptions/                  # Domain exceptions
│   └── ValueObjects/                # Value objects
│
├── InventoryManagement.Application/ # Application Layer
│   ├── Commands/                    # CQRS Commands
│   ├── Queries/                     # CQRS Queries
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Interfaces/                  # Repository interfaces
│   └── Behaviors/                   # MediatR behaviors
│
├── InventoryManagement.Infrastructure/ # Infrastructure Layer
│   ├── Data/                        # DbContext, Configurations
│   └── Repositories/                # Repository implementations
│
├── Shared.Domain/                   # Shared domain components
├── Shared.Application/              # Shared application components
└── Shared.Contracts/                # Integration contracts
```

---

## Phases Development

### Phase 1: Inventory Management (Current - 80% Complete)

- [x] Product CRUD
- [x] Category CRUD
- [x] Supplier CRUD
- [x] Product Batch management (expiry tracking)
- [x] Stock Transactions (Purchase, Sale, Adjustment)
- [ ] Stock Transaction filters (supplierId, productId, dateRange)
- [ ] Low stock alerts
- [ ] Expiry alerts

### Phase 2: Sales Management (Upcoming)

- [ ] Sales orders
- [ ] Invoice generation
- [ ] Daily/Monthly reports
- [ ] Expense tracking

### Phase 3: Advanced Features (Future)

- [ ] Barcode scanning
- [ ] PDF invoice export
- [ ] Annual revenue calculation
- [ ] Tax handling

---

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- PostgreSQL 16 (or use Docker)

### Run with .NET Aspire

```bash
cd BiBoGC
dotnet run --project BiBoGC.AppHost
```

### API Base URL

- Development: `http://localhost:5020`
- Aspire Dashboard: `https://localhost:17062`

---

## Documentation Index

| Document                                    | Description                            |
| ------------------------------------------- | -------------------------------------- |
| [Architecture](./Architecture.md)           | System architecture và design patterns |
| [API Reference](./API-Reference.md)         | API endpoints và usage                 |
| [Database Schema](./Database-Schema.md)     | Entity relationships và tables         |
| [Development Guide](./Development-Guide.md) | Setup và contribution guide            |
| [Testing Guide](./Testing-Guide.md)         | Testing strategies và examples         |
| [Phase Roadmap](./Phase-Roadmap.md)         | Detailed roadmap cho từng phase        |

---

## Contact

- **Project Lead**: Tran Tien Loc
- **Repository**: [https://github.com/Trantienloc2411/BiBo-s-GC]
- **Documentation**: `/Documentation` folder

---

_Last updated: January 2026_
