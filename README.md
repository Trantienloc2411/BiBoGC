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
Directory structure:
└── trantienloc2411-bibogc/
    ├── README.md
    ├── admin-dashboard/
    │   ├── next-env.d.ts
    │   ├── next.config.ts
    │   ├── package.json
    │   ├── postcss.config.mjs
    │   ├── proxy.ts
    │   ├── tsconfig.json
    │   ├── .env.local.example
    │   ├── app/
    │   │   ├── globals.css
    │   │   ├── layout.tsx
    │   │   ├── page.tsx
    │   │   ├── (admin)/
    │   │   │   ├── layout.tsx
    │   │   │   ├── auditlogs/
    │   │   │   │   └── page.tsx
    │   │   │   ├── dashboard/
    │   │   │   │   └── page.tsx
    │   │   │   ├── expenses/
    │   │   │   │   └── page.tsx
    │   │   │   ├── inventory/
    │   │   │   │   ├── alerts/
    │   │   │   │   │   └── page.tsx
    │   │   │   │   ├── categories/
    │   │   │   │   │   └── page.tsx
    │   │   │   │   ├── import/
    │   │   │   │   │   └── page.tsx
    │   │   │   │   ├── products/
    │   │   │   │   │   ├── page.tsx
    │   │   │   │   │   └── [id]/
    │   │   │   │   │       └── page.tsx
    │   │   │   │   ├── stock-transactions/
    │   │   │   │   │   └── page.tsx
    │   │   │   │   └── suppliers/
    │   │   │   │       └── page.tsx
    │   │   │   ├── invoices/
    │   │   │   │   ├── page.tsx
    │   │   │   │   └── [id]/
    │   │   │   │       └── page.tsx
    │   │   │   ├── orders/
    │   │   │   │   ├── page.tsx
    │   │   │   │   └── [id]/
    │   │   │   │       └── page.tsx
    │   │   │   ├── reports/
    │   │   │   │   ├── page.tsx
    │   │   │   │   └── tax/
    │   │   │   │       └── page.tsx
    │   │   │   └── tax-config/
    │   │   │       └── page.tsx
    │   │   ├── (auth)/
    │   │   │   └── login/
    │   │   │       └── page.tsx
    │   │   └── api/
    │   │       └── auth/
    │   │           ├── login/
    │   │           │   └── route.ts
    │   │           ├── logout/
    │   │           │   └── route.ts
    │   │           └── refresh/
    │   │               └── route.ts
    │   ├── components/
    │   │   ├── dashboard/
    │   │   │   ├── HourlyChart.tsx
    │   │   │   ├── StatCard.tsx
    │   │   │   └── TopProducts.tsx
    │   │   ├── expenses/
    │   │   │   └── AddExpenseModal.tsx
    │   │   ├── layout/
    │   │   │   ├── NotificationBell.tsx
    │   │   │   ├── Sidebar.tsx
    │   │   │   └── TopBar.tsx
    │   │   ├── reports/
    │   │   │   ├── AnnualReport.tsx
    │   │   │   ├── DailyReport.tsx
    │   │   │   ├── FinancialReport.tsx
    │   │   │   └── MonthlyReport.tsx
    │   │   └── ui/
    │   │       ├── AsyncSupplierSelect.tsx
    │   │       ├── Button.tsx
    │   │       ├── Card.tsx
    │   │       ├── CategoryPicker.tsx
    │   │       ├── ConfirmDialog.tsx
    │   │       ├── ExportMenu.tsx
    │   │       ├── FormDialog.tsx
    │   │       ├── LoadingSpinner.tsx
    │   │       ├── MoneyInput.tsx
    │   │       ├── Pagination.tsx
    │   │       ├── PdfViewerModal.tsx
    │   │       ├── StatusBadge.tsx
    │   │       └── Toast.tsx
    │   ├── hooks/
    │   │   ├── useExportFile.ts
    │   │   ├── useNotifications.ts
    │   │   └── useSignalR.ts
    │   ├── lib/
    │   │   ├── api.ts
    │   │   ├── download.ts
    │   │   ├── exportService.ts
    │   │   └── utils.ts
    │   ├── types/
    │   │   └── index.ts
    │   └── .claude/
    │       └── settings.local.json
    ├── BiBoGC/
    │   ├── BiBoGC.slnx
    │   ├── logo-preview.html
    │   ├── .dockerignore
    │   ├── AuthorizationModule.Application/
    │   │   ├── AuthorizationModule.Application.csproj
    │   │   ├── DependencyInjection.cs
    │   │   ├── Command/
    │   │   │   ├── Login/
    │   │   │   │   ├── LoginCommand.cs
    │   │   │   │   └── LoginCommandHandler.cs
    │   │   │   ├── Logout/
    │   │   │   │   ├── LogoutCommand.cs
    │   │   │   │   └── LogoutCommandHandler.cs
    │   │   │   ├── RefreshToken/
    │   │   │   │   ├── RefreshTokenCommand.cs
    │   │   │   │   └── RefreshTokenCommandHandler.cs
    │   │   │   └── RevokeToken/
    │   │   │       ├── RevokeCommand.cs
    │   │   │       └── RevokeCommandHandler.cs
    │   │   ├── DTOs/
    │   │   │   ├── AuditLogDto.cs
    │   │   │   ├── AuthResponseDto.cs
    │   │   │   ├── LoginRequestDto.cs
    │   │   │   └── RefreshTokenRequestDto.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IAuditLogService.cs
    │   │   │   ├── IAuthService.cs
    │   │   │   ├── IJwtTokenGenerator.cs
    │   │   │   └── IPasswordHasher.cs
    │   │   └── Queries/
    │   │       └── GetAuditLogs/
    │   │           ├── GetAuditLogsQuery.cs
    │   │           └── GetAuditLogsQueryHandler.cs
    │   ├── AuthorizationModule.Domain/
    │   │   ├── AuthorizationModule.Domain.csproj
    │   │   ├── Entities/
    │   │   │   ├── AuditLog.cs
    │   │   │   ├── RefreshToken.cs
    │   │   │   └── User.cs
    │   │   └── Enums/
    │   │       └── UserRole.cs
    │   ├── AuthorizationModule.Infrastructure/
    │   │   ├── AuthorizationModule.Infrastructure.csproj
    │   │   ├── DependencyInjection.cs
    │   │   ├── Data/
    │   │   │   ├── AuthorizationDataSeeder.cs
    │   │   │   ├── AuthorizationDbContext.cs
    │   │   │   ├── DesignTimeDbContextFactory.cs
    │   │   │   ├── Configurations/
    │   │   │   │   ├── AuditLogConfiguration.cs
    │   │   │   │   ├── RefreshTokenConfiguration.cs
    │   │   │   │   └── UserConfiguration.cs
    │   │   │   └── Migrations/
    │   │   │       ├── 20260203072749_Initial_Auth.cs
    │   │   │       ├── 20260203072749_Initial_Auth.Designer.cs
    │   │   │       ├── 20260320043106_Add_AuditLog.cs
    │   │   │       ├── 20260320043106_Add_AuditLog.Designer.cs
    │   │   │       ├── 20260321091620_Add_AuditLog_Indexes.cs
    │   │   │       ├── 20260321091620_Add_AuditLog_Indexes.Designer.cs
    │   │   │       └── AuthorizationDbContextModelSnapshot.cs
    │   │   ├── Security/
    │   │   │   ├── JwtTokenGenerator.cs
    │   │   │   └── PasswordHasher.cs
    │   │   └── Services/
    │   │       ├── AuditLoggerAdapter.cs
    │   │       ├── AuditLogService.cs
    │   │       └── AuthService.cs
    │   ├── BiBoGC/
    │   │   ├── appsettings.Development.json
    │   │   ├── appsettings.json
    │   │   ├── BiBoGC.csproj
    │   │   ├── Dockerfile
    │   │   ├── Program.cs
    │   │   ├── Controllers/
    │   │   │   ├── AuditLogsController.cs
    │   │   │   ├── AuthController.cs
    │   │   │   ├── CategoriesController.cs
    │   │   │   ├── FinanceController.cs
    │   │   │   ├── InvoicesController.cs
    │   │   │   ├── NotificationController.cs
    │   │   │   ├── ProductsController.cs
    │   │   │   ├── ProductVariantsController.cs
    │   │   │   ├── SalesOrdersController.cs
    │   │   │   ├── StockTransactionsController.cs
    │   │   │   └── SuppliersController.cs
    │   │   ├── Hubs/
    │   │   │   ├── NotificationHub.cs
    │   │   │   └── SignalRNotificationPusher.cs
    │   │   ├── Middleware/
    │   │   │   ├── AuditLogMiddleware.cs
    │   │   │   └── ExceptionHandlingMiddleware.cs
    │   │   ├── Models/
    │   │   │   └── ApiResponse.cs
    │   │   └── Properties/
    │   │       └── launchSettings.json
    │   ├── BiBoGC.AppHost/
    │   │   ├── AppHost.cs
    │   │   ├── appsettings.Development.json
    │   │   ├── appsettings.json
    │   │   ├── BiBoGC.AppHost.csproj
    │   │   ├── Properties/
    │   │   │   └── launchSettings.json
    │   │   └── .claude/
    │   │       └── settings.local.json
    │   ├── BiBoGC.ServiceDefaults/
    │   │   ├── BiBoGC.ServiceDefaults.csproj
    │   │   └── Extensions.cs
    │   ├── Catalog.Domain/
    │   │   └── Catalog.Domain.csproj
    │   ├── Documentation/
    │   │   ├── README.md
    │   │   ├── API-Reference.md
    │   │   ├── Architecture.md
    │   │   ├── Database-Schema.md
    │   │   ├── Development-Guide.md
    │   │   ├── Phase-Roadmap.md
    │   │   ├── Testing-Guide.md
    │   │   ├── Flow/
    │   │   │   └── FlowImportProduct.drawio
    │   │   └── Jira-Import/
    │   │       └── IMPORT-GUIDE.md
    │   ├── Finance.Application/
    │   │   ├── DependencyInjection.cs
    │   │   ├── Finance.Application.csproj
    │   │   ├── Commands/
    │   │   │   ├── CreateExpense/
    │   │   │   │   ├── CreateExpenseCommand.cs
    │   │   │   │   ├── CreateExpenseCommandHandler.cs
    │   │   │   │   └── CreateExpenseCommandValidator.cs
    │   │   │   └── UpdateTaxConfig/
    │   │   │       ├── UpdateTaxConfigCommand.cs
    │   │   │       ├── UpdateTaxConfigCommandHandler.cs
    │   │   │       └── UpdateTaxConfigCommandValidator.cs
    │   │   ├── DTOs/
    │   │   │   ├── AnnualRevenueReportDto.cs
    │   │   │   ├── DailyExpenseSummaryDto.cs
    │   │   │   ├── DailySalesReportDto.cs
    │   │   │   ├── ExpenseDto.cs
    │   │   │   ├── MonthlyFinancialReportDto.cs
    │   │   │   ├── MonthlySalesReportDto.cs
    │   │   │   └── TaxConfigDto.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IExpenseRepository.cs
    │   │   │   ├── IFinanceUnitOfWork.cs
    │   │   │   ├── IInvoiceDataReader.cs
    │   │   │   ├── IReportPdfExportService.cs
    │   │   │   ├── ISalesDataReader.cs
    │   │   │   ├── IStockDataReader.cs
    │   │   │   ├── ITaxConfigRepository.cs
    │   │   │   ├── ITaxDeclarationExportService.cs
    │   │   │   └── ITaxReportExportService.cs
    │   │   ├── Queries/
    │   │   │   ├── ExportDailySalesReportPdf/
    │   │   │   │   ├── ExportDailySalesReportPdfQuery.cs
    │   │   │   │   └── ExportDailySalesReportPdfQueryHandler.cs
    │   │   │   ├── ExportFinancialReportPdf/
    │   │   │   │   ├── ExportFinancialReportPdfQuery.cs
    │   │   │   │   └── ExportFinancialReportPdfQueryHandler.cs
    │   │   │   ├── ExportMonthlySalesReportPdf/
    │   │   │   │   ├── ExportMonthlySalesReportPdfQuery.cs
    │   │   │   │   └── ExportMonthlySalesReportPdfQueryHandler.cs
    │   │   │   ├── ExportTaxDeclaration/
    │   │   │   │   ├── ExportTaxDeclarationQuery.cs
    │   │   │   │   └── ExportTaxDeclarationQueryHandler.cs
    │   │   │   ├── ExportTaxReport/
    │   │   │   │   ├── ExportTaxReportQuery.cs
    │   │   │   │   └── ExportTaxReportQueryHandler.cs
    │   │   │   ├── GetAnnualRevenueReport/
    │   │   │   │   ├── GetAnnualRevenueReportQuery.cs
    │   │   │   │   └── GetAnnualRevenueReportQueryHandler.cs
    │   │   │   ├── GetDailyExpenseSummary/
    │   │   │   │   ├── GetDailyExpenseSummaryQuery.cs
    │   │   │   │   └── GetDailyExpenseSummaryQueryHandler.cs
    │   │   │   ├── GetDailySalesReport/
    │   │   │   │   ├── GetDailySalesReportQuery.cs
    │   │   │   │   └── GetDailySalesReportQueryHandler.cs
    │   │   │   ├── GetMonthlyFinancialReport/
    │   │   │   │   ├── GetMonthlyFinancialReportQuery.cs
    │   │   │   │   └── GetMonthlyFinancialReportQueryHandler.cs
    │   │   │   ├── GetMonthlySalesReport/
    │   │   │   │   ├── GetMonthlySalesReportQuery.cs
    │   │   │   │   └── GetMonthlySalesReportQueryHandler.cs
    │   │   │   └── GetTaxConfig/
    │   │   │       ├── GetTaxConfigQuery.cs
    │   │   │       └── GetTaxConfigQueryHandler.cs
    │   │   └── ReadModels/
    │   │       └── SalesReadModels.cs
    │   ├── Finance.Domain/
    │   │   ├── Finance.Domain.csproj
    │   │   ├── Entities/
    │   │   │   ├── Expense.cs
    │   │   │   └── TaxConfiguration.cs
    │   │   └── Enums/
    │   │       ├── ExpenseCategory.cs
    │   │       └── ExpensePaymentMethod.cs
    │   ├── Finance.Infrastructure/
    │   │   ├── DependencyInjection.cs
    │   │   ├── Finance.Infrastructure.csproj
    │   │   ├── Data/
    │   │   │   ├── DesignTimeDbContextFactory.cs
    │   │   │   ├── FinanceDbContext.cs
    │   │   │   └── Configurations/
    │   │   │       ├── ExpenseConfiguration.cs
    │   │   │       └── TaxConfigurationConfiguration.cs
    │   │   ├── Migrations/
    │   │   │   ├── 20260319113429_InitialFinance.cs
    │   │   │   ├── 20260319113429_InitialFinance.Designer.cs
    │   │   │   ├── 20260324051242_AddTaxConfiguration.cs
    │   │   │   ├── 20260324051242_AddTaxConfiguration.Designer.cs
    │   │   │   └── FinanceDbContextModelSnapshot.cs
    │   │   ├── Pdf/
    │   │   │   ├── DailySalesReportDocument.cs
    │   │   │   ├── FinancialReportDocument.cs
    │   │   │   └── MonthlySalesReportDocument.cs
    │   │   ├── Persistence/
    │   │   │   └── FinanceUnitOfWork.cs
    │   │   ├── Repositories/
    │   │   │   ├── ExpenseRepository.cs
    │   │   │   └── TaxConfigRepository.cs
    │   │   ├── Services/
    │   │   │   ├── InvoiceDataReader.cs
    │   │   │   ├── ReportPdfExportService.cs
    │   │   │   ├── SalesDataReader.cs
    │   │   │   ├── StockDataReader.cs
    │   │   │   ├── TaxConfigService.cs
    │   │   │   ├── TaxDeclarationExportService.cs
    │   │   │   └── TaxReportExportService.cs
    │   │   └── Templates/
    │   │       ├── Khai-thue-ho-kinh-doanh-template.docx
    │   │       └── S2a-HKD.xlsx
    │   ├── InventoryManagement.Application/
    │   │   ├── DependencyInjection.cs
    │   │   ├── InventoryManagement.Application.csproj
    │   │   ├── Behaviors/
    │   │   │   └── ValidationBehavior.cs
    │   │   ├── Commands/
    │   │   │   ├── AddBatch/
    │   │   │   │   ├── AddBatchCommand.cs
    │   │   │   │   ├── AddBatchCommandHandler.cs
    │   │   │   │   └── AddBatchCommandValidator.cs
    │   │   │   ├── CreateCategory/
    │   │   │   │   ├── CreateCategoryCommand.cs
    │   │   │   │   ├── CreateCategoryCommandHandler.cs
    │   │   │   │   └── CreateCategoryCommandValidator.cs
    │   │   │   ├── CreateProduct/
    │   │   │   │   ├── CreateProductCommand.cs
    │   │   │   │   ├── CreateProductCommandHandler.cs
    │   │   │   │   └── CreateProductCommandValidator.cs
    │   │   │   ├── CreateProductVariant/
    │   │   │   │   ├── CreateProductVariantCommand.cs
    │   │   │   │   ├── CreateProductVariantCommandHandler.cs
    │   │   │   │   └── CreateProductVariantCommandValidator.cs
    │   │   │   ├── CreateStockTransaction/
    │   │   │   │   ├── CreateStockTransactionCommand.cs
    │   │   │   │   ├── CreateStockTransactionCommandHandler.cs
    │   │   │   │   └── CreateStockTransactionCommandValidator.cs
    │   │   │   ├── CreateSupplier/
    │   │   │   │   ├── CreateSupplierCommand.cs
    │   │   │   │   ├── CreateSupplierCommandHandler.cs
    │   │   │   │   └── CreateSupplierCommandValidator.cs
    │   │   │   ├── DeleteBatch/
    │   │   │   │   ├── DeleteBatchCommand.cs
    │   │   │   │   ├── DeleteBatchCommandHandler.cs
    │   │   │   │   └── DeleteBatchCommandValidator.cs
    │   │   │   ├── DeleteCategory/
    │   │   │   │   ├── DeleteCategoryCommand.cs
    │   │   │   │   ├── DeleteCategoryCommandHandler.cs
    │   │   │   │   └── DeleteCategoryCommandValidator.cs
    │   │   │   ├── DeleteProduct/
    │   │   │   │   ├── DeleteProductCommand.cs
    │   │   │   │   ├── DeleteProductCommandHandler.cs
    │   │   │   │   └── DeleteProductCommandValidator.cs
    │   │   │   ├── DeleteProductVariant/
    │   │   │   │   ├── DeleteProductVariantCommand.cs
    │   │   │   │   ├── DeleteProductVariantCommandHandler.cs
    │   │   │   │   └── DeleteProductVariantCommandValidator.cs
    │   │   │   ├── DeleteSupplier/
    │   │   │   │   ├── DeleteSupplierCommand.cs
    │   │   │   │   ├── DeleteSupplierCommandHandler.cs
    │   │   │   │   └── DeleteSupplierCommandValidator.cs
    │   │   │   ├── ImportProducts/
    │   │   │   │   ├── ImportProductsCommand.cs
    │   │   │   │   └── ImportProductsCommandHandler.cs
    │   │   │   ├── UpdateBatch/
    │   │   │   │   ├── UpdateBatchCommand.cs
    │   │   │   │   ├── UpdateBatchCommandHandler.cs
    │   │   │   │   └── UpdateBatchCommandValidator.cs
    │   │   │   ├── UpdateCategory/
    │   │   │   │   ├── UpdateCategoryCommand.cs
    │   │   │   │   └── UpdateCategoryCommandHandler.cs
    │   │   │   ├── UpdateProduct/
    │   │   │   │   ├── UpdateProductCommand.cs
    │   │   │   │   ├── UpdateProductCommandHandler.cs
    │   │   │   │   └── UpdateProductCommandValidator.cs
    │   │   │   ├── UpdateProductVariant/
    │   │   │   │   ├── UpdateProductVariantCommand.cs
    │   │   │   │   ├── UpdateProductVariantCommandHandler.cs
    │   │   │   │   └── UpdateProductVariantCommandValidator.cs
    │   │   │   └── UpdateSupplier/
    │   │   │       ├── UpdateSupplierCommand.cs
    │   │   │       ├── UpdateSupplierCommandHandler.cs
    │   │   │       └── UpdateSupplierCommandValidator.cs
    │   │   ├── DTOs/
    │   │   │   ├── CategoryDto.cs
    │   │   │   ├── ImportProductsResultDto.cs
    │   │   │   ├── PaginatedResult.cs
    │   │   │   ├── ProductBatchDto.cs
    │   │   │   ├── ProductDto.cs
    │   │   │   ├── ProductVariantDto.cs
    │   │   │   ├── StockTransactionDto.cs
    │   │   │   └── SupplierDto.cs
    │   │   ├── Helper/
    │   │   │   └── AutoGenerateSkuUnique.cs
    │   │   ├── Interfaces/
    │   │   │   ├── ICategoryRepository.cs
    │   │   │   ├── IProductBatchRepository.cs
    │   │   │   ├── IProductExportService.cs
    │   │   │   ├── IProductImportService.cs
    │   │   │   ├── IProductRepository.cs
    │   │   │   ├── IProductVariantRepository.cs
    │   │   │   ├── IStockTransactionRepository.cs
    │   │   │   └── ISupplierRepository.cs
    │   │   └── Queries/
    │   │       ├── ExportExistingProducts/
    │   │       │   ├── ExportExistingProductsQuery.cs
    │   │       │   └── ExportExistingProductsQueryHandler.cs
    │   │       ├── GetBatch/
    │   │       │   ├── GetBatchQuery.cs
    │   │       │   ├── GetBatchQueryHandler.cs
    │   │       │   └── GetBatchQueryValidator.cs
    │   │       ├── GetBatches/
    │   │       │   ├── GetBatchesQuery.cs
    │   │       │   ├── GetBatchesQueryHandler.cs
    │   │       │   └── GetBatchesQueryValidator.cs
    │   │       ├── GetCategories/
    │   │       │   ├── GetCategoriesQuery.cs
    │   │       │   └── GetCategoriesQueryHandler.cs
    │   │       ├── GetCategory/
    │   │       │   ├── GetCategoryQuery.cs
    │   │       │   └── GetCategoryQueryHandler.cs
    │   │       ├── GetCategoryTree/
    │   │       │   ├── GetCategoryTreeQuery.cs
    │   │       │   └── GetCategoryTreeQueryHandler.cs
    │   │       ├── GetExpiredBatchProducts/
    │   │       │   ├── GetExpiredBatchProductsQuery.cs
    │   │       │   └── GetExpiredBatchProductsQueryHandler.cs
    │   │       ├── GetExpiringSoonProducts/
    │   │       │   ├── GetExpiringSoonProductsQuery.cs
    │   │       │   └── GetExpiringSoonProductsQueryHandler.cs
    │   │       ├── GetLowStockProducts/
    │   │       │   ├── GetLowStockProductsQuery.cs
    │   │       │   └── GetLowStockProductsQueryHandler.cs
    │   │       ├── GetProduct/
    │   │       │   ├── GetProductQuery.cs
    │   │       │   ├── GetProductQueryHandler.cs
    │   │       │   └── GetProductQueryValidator.cs
    │   │       ├── GetProducts/
    │   │       │   ├── GetProductsQuery.cs
    │   │       │   ├── GetProductsQueryHandler.cs
    │   │       │   └── GetProductsQueryValidator.cs
    │   │       ├── GetProductVariant/
    │   │       │   ├── GetProductVariantQuery.cs
    │   │       │   └── GetProductVariantQueryHandler.cs
    │   │       ├── GetProductVariants/
    │   │       │   ├── GetProductVariantsQuery.cs
    │   │       │   └── GetProductVariantsQueryHandler.cs
    │   │       ├── GetProductVariantsByProductId/
    │   │       │   ├── GetProductVariantsByProductIdQuery.cs
    │   │       │   └── GetProductVariantsByProductIdQueryHandler.cs
    │   │       ├── GetStockTransaction/
    │   │       │   ├── GetStockTransactionQuery.cs
    │   │       │   ├── GetStockTransactionQueryHandler.cs
    │   │       │   └── GetStockTransactionQueryValidator.cs
    │   │       ├── GetStockTransactions/
    │   │       │   ├── GetStockTransactionsQuery.cs
    │   │       │   ├── GetStockTransactionsQueryHandler.cs
    │   │       │   └── GetStockTransactionsQueryValidator.cs
    │   │       ├── GetSupplier/
    │   │       │   ├── GetSupplierQuery.cs
    │   │       │   └── GetSupplierQueryHandler.cs
    │   │       ├── GetSuppliers/
    │   │       │   ├── GetSuppliersQuery.cs
    │   │       │   ├── GetSuppliersQueryHandler.cs
    │   │       │   └── GetSuppliersQueryValidation.cs
    │   │       └── GetVariantByBarcode/
    │   │           ├── GetVariantByBarcodeQuery.cs
    │   │           └── GetVariantByBarcodeQueryHandler.cs
    │   ├── InventoryManagement.Domain/
    │   │   ├── InventoryManagement.Domain.csproj
    │   │   ├── Entities/
    │   │   │   ├── Category.cs
    │   │   │   ├── Product.cs
    │   │   │   ├── ProductBatch.cs
    │   │   │   ├── ProductVariant.cs
    │   │   │   ├── StockTransaction.cs
    │   │   │   └── Supplier.cs
    │   │   ├── Enums/
    │   │   │   ├── ProductStatuses.cs
    │   │   │   ├── StockTransactionType.cs
    │   │   │   └── Units.cs
    │   │   ├── Events/
    │   │   │   ├── CategoryCreatedEvent.cs
    │   │   │   ├── StockTransactionCreatedEvent.cs
    │   │   │   ├── BatchEvents/
    │   │   │   │   └── BatchAddedEvent.cs
    │   │   │   ├── ProductEvents/
    │   │   │   │   ├── ProductCreatedEvent.cs
    │   │   │   │   ├── ProductDiscontinuedEvent.cs
    │   │   │   │   ├── ProductLowStockEvent.cs
    │   │   │   │   └── ProductOutOfStockEvent.cs
    │   │   │   └── ProductVariantEvents/
    │   │   │       ├── ProductVariantCreatedEvent.cs
    │   │   │       └── ProductVariantPriceChangedEvent.cs
    │   │   ├── Exceptions/
    │   │   │   ├── InsufficientStockException.cs
    │   │   │   └── ProductNotFoundException.cs
    │   │   └── ValueObjects/
    │   │       ├── Money.cs
    │   │       ├── Quantity.cs
    │   │       └── Sku.cs
    │   ├── InventoryManagement.Infrastructure/
    │   │   ├── DependencyInjection.cs
    │   │   ├── InventoryManagement.Infrastructure.csproj
    │   │   ├── Data/
    │   │   │   ├── DataSeeder.cs
    │   │   │   ├── DesignTImeDbContextFactory.cs
    │   │   │   ├── InventoryDbContext.cs
    │   │   │   ├── Configurations/
    │   │   │   │   ├── CategoryConfiguration.cs
    │   │   │   │   ├── ProductBatchConfiguration.cs
    │   │   │   │   ├── ProductConfiguration.cs
    │   │   │   │   ├── ProductVariantConfiguration.cs
    │   │   │   │   ├── StockTransactionConfiguration.cs
    │   │   │   │   └── SupplierConfiguration.cs
    │   │   │   └── Migrations/
    │   │   │       ├── 20260126093647_Initial.cs
    │   │   │       ├── 20260126093647_Initial.Designer.cs
    │   │   │       ├── 20260304110649_MakeStockTransactionFKsNullable.cs
    │   │   │       ├── 20260304110649_MakeStockTransactionFKsNullable.Designer.cs
    │   │   │       ├── 20260704054829_RemoveProductBasePriceAndAverageCostPrice.cs
    │   │   │       ├── 20260704054829_RemoveProductBasePriceAndAverageCostPrice.Designer.cs
    │   │   │       └── InventoryDbContextModelSnapshot.cs
    │   │   ├── Repositories/
    │   │   │   ├── CategoryRepository.cs
    │   │   │   ├── ProductBatchRepository.cs
    │   │   │   ├── ProductRepository.cs
    │   │   │   ├── ProductVariantRepository.cs
    │   │   │   ├── StockTransactionRepository.cs
    │   │   │   └── SupplierRepository.cs
    │   │   └── Services/
    │   │       ├── ProductExportService.cs
    │   │       └── ProductImportService.cs
    │   ├── Notification.Application/
    │   │   ├── DependencyInjection.cs
    │   │   ├── Notification.Application.csproj
    │   │   ├── Commands/
    │   │   │   ├── CreateNotification/
    │   │   │   │   ├── CreateNotificationCommand.cs
    │   │   │   │   └── CreateNotificationCommandHandler.cs
    │   │   │   ├── MarkAllAsRead/
    │   │   │   │   ├── MarkAllAsReadCommand.cs
    │   │   │   │   └── MarkAllAsReadCommandHandler.cs
    │   │   │   └── MarkAsRead/
    │   │   │       ├── MarkAsReadCommand.cs
    │   │   │       └── MarkAsReadCommandHandler.cs
    │   │   ├── DTOs/
    │   │   │   ├── NotificationDto.cs
    │   │   │   └── NotificationListDto.cs
    │   │   ├── Interfaces/
    │   │   │   ├── INotificationRepository.cs
    │   │   │   ├── INotificationUnitOfWork.cs
    │   │   │   └── IRealTimeNotificationPusher.cs
    │   │   └── Queries/
    │   │       └── GetNotifications/
    │   │           ├── GetNotificationsQuery.cs
    │   │           └── GetNotificationsQueryHandler.cs
    │   ├── Notification.Domain/
    │   │   ├── Notification.Domain.csproj
    │   │   └── Entities/
    │   │       └── Notification.cs
    │   ├── Notification.Infrastructure/
    │   │   ├── DependencyInjection.cs
    │   │   ├── Notification.Infrastructure.csproj
    │   │   ├── Data/
    │   │   │   ├── DesignTimeDbContextFactory.cs
    │   │   │   ├── NotificationDbContext.cs
    │   │   │   └── Configurations/
    │   │   │       └── NotificationConfiguration.cs
    │   │   ├── Migrations/
    │   │   │   ├── 20260324092314_InitialNotification.cs
    │   │   │   ├── 20260324092314_InitialNotification.Designer.cs
    │   │   │   └── NotificationDbContextModelSnapshot.cs
    │   │   ├── Persistence/
    │   │   │   └── NotificationUnitOfWork.cs
    │   │   ├── Repositories/
    │   │   │   └── NotificationRepository.cs
    │   │   └── Services/
    │   │       └── NotificationService.cs
    │   ├── Sale.Application/
    │   │   ├── DependencyInjection.cs
    │   │   ├── Sale.Application.csproj
    │   │   ├── Commands/
    │   │   │   ├── AddItemToOrder/
    │   │   │   │   ├── AddItemToOrderCommand.cs
    │   │   │   │   ├── AddItemToOrderCommandHandler.cs
    │   │   │   │   └── AddItemToOrderCommandValidator.cs
    │   │   │   ├── ApplyDiscount/
    │   │   │   │   ├── ApplyDiscountCommand.cs
    │   │   │   │   ├── ApplyDiscountCommandHandler.cs
    │   │   │   │   └── ApplyDiscountCommandValidator.cs
    │   │   │   ├── CancelOrder/
    │   │   │   │   ├── CancelOrderCommand.cs
    │   │   │   │   └── CancelOrderCommandHandler.cs
    │   │   │   ├── CompleteOrder/
    │   │   │   │   ├── CompleteOrderCommand.cs
    │   │   │   │   ├── CompleteOrderCommandHandler.cs
    │   │   │   │   └── CompleteOrderCommandValidator.cs
    │   │   │   ├── CreateSalesOrder/
    │   │   │   │   ├── CreateSalesOrderCommand.cs
    │   │   │   │   ├── CreateSalesOrderCommandHandler.cs
    │   │   │   │   └── CreateSalesOrderCommandValidator.cs
    │   │   │   ├── GenerateInvoice/
    │   │   │   │   ├── GenerateInvoiceCommand.cs
    │   │   │   │   └── GenerateInvoiceCommandHandler.cs
    │   │   │   ├── RemoveItemFromOrder/
    │   │   │   │   ├── RemoveItemFromOrderCommand.cs
    │   │   │   │   └── RemoveItemFromOrderCommandHandler.cs
    │   │   │   └── UpdateOrderItemQuantity/
    │   │   │       ├── UpdateOrderItemQuantityCommand.cs
    │   │   │       ├── UpdateOrderItemQuantityCommandHandler.cs
    │   │   │       └── UpdateOrderItemQuantityCommandValidator.cs
    │   │   ├── DTOs/
    │   │   │   ├── InvoiceDto.cs
    │   │   │   ├── InvoiceItemDto.cs
    │   │   │   ├── SalesOrderDto.cs
    │   │   │   └── SalesOrderItemDto.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IInvoiceNumberGenerator.cs
    │   │   │   ├── IInvoiceRepository.cs
    │   │   │   ├── IOrderNumberGenerator.cs
    │   │   │   ├── IPdfExportService.cs
    │   │   │   ├── ISalesOrderRepository.cs
    │   │   │   ├── ISaleUnitOfWork.cs
    │   │   │   └── IStoreInfoService.cs
    │   │   ├── Mappers/
    │   │   │   ├── InvoiceMapper.cs
    │   │   │   └── SalesOrderMapper.cs
    │   │   └── Queries/
    │   │       ├── ExportInvoicePdf/
    │   │       │   ├── ExportInvoicePdfQuery.cs
    │   │       │   └── ExportInvoicePdfQueryHandler.cs
    │   │       ├── ExportInvoicesZip/
    │   │       │   ├── ExportInvoicesZipQuery.cs
    │   │       │   └── ExportInvoicesZipQueryHandler.cs
    │   │       ├── GetInvoice/
    │   │       │   ├── GetInvoiceQuery.cs
    │   │       │   └── GetInvoiceQueryHandler.cs
    │   │       ├── GetInvoices/
    │   │       │   ├── GetInvoicesQuery.cs
    │   │       │   └── GetInvoicesQueryHandler.cs
    │   │       ├── GetSalesOrder/
    │   │       │   ├── GetSalesOrderQuery.cs
    │   │       │   └── GetSalesOrderQueryHandler.cs
    │   │       └── GetSalesOrders/
    │   │           ├── GetSalesOrdersQuery.cs
    │   │           └── GetSalesOrdersQueryHandler.cs
    │   ├── Sale.Domain/
    │   │   ├── Sale.Domain.csproj
    │   │   ├── Domain/
    │   │   │   ├── Invoice.cs
    │   │   │   ├── InvoiceItem.cs
    │   │   │   ├── InvoiceNumberSequence.cs
    │   │   │   ├── OrderNumberSequence.cs
    │   │   │   ├── SalesOrder.cs
    │   │   │   └── SalesOrderItem.cs
    │   │   ├── Enum/
    │   │   │   ├── OrderStatus.cs
    │   │   │   └── PaymentMethod.cs
    │   │   ├── Events/
    │   │   │   ├── InvoiceGeneratedEvent.cs
    │   │   │   ├── SalesOrderCancelledEvent.cs
    │   │   │   ├── SalesOrderCompletedEvent.cs
    │   │   │   └── SalesOrderCreatedEvent.cs
    │   │   ├── Exceptions/
    │   │   │   ├── DuplicateInvoiceException.cs
    │   │   │   ├── InsufficientStockException.cs
    │   │   │   ├── InvalidOrderStateException.cs
    │   │   │   └── SalesOrderNotFoundException.cs
    │   │   └── ValueObjects/
    │   │       └── StoreInfo.cs
    │   ├── Sale.Infrastructure/
    │   │   ├── DependencyInjection.cs
    │   │   ├── Sale.Infrastructure.csproj
    │   │   ├── Data/
    │   │   │   ├── DesignTimeDbContextFactory.cs
    │   │   │   ├── SaleDbContext.cs
    │   │   │   └── Configurations/
    │   │   │       ├── InvoiceConfiguration.cs
    │   │   │       ├── InvoiceItemConfiguration.cs
    │   │   │       ├── InvoiceNumberSequenceConfiguration.cs
    │   │   │       ├── OrderNumberSequenceConfiguration.cs
    │   │   │       ├── SalesOrderConfiguration.cs
    │   │   │       └── SalesOrderItemConfiguration.cs
    │   │   ├── Migrations/
    │   │   │   ├── 20260227165019_Initial_Sale.cs
    │   │   │   ├── 20260227165019_Initial_Sale.Designer.cs
    │   │   │   ├── 20260414113519_Add_OrderNumberSequence.cs
    │   │   │   ├── 20260414113519_Add_OrderNumberSequence.Designer.cs
    │   │   │   └── SaleDbContextModelSnapshot.cs
    │   │   ├── Pdf/
    │   │   │   └── InvoicePdfDocument.cs
    │   │   ├── Repositories/
    │   │   │   ├── InvoiceRepository.cs
    │   │   │   └── SalesOrderRepository.cs
    │   │   └── Services/
    │   │       ├── InvoiceNumberGenerator.cs
    │   │       ├── OrderNumberGenerator.cs
    │   │       ├── PdfExportService.cs
    │   │       ├── SaleUnitOfWork.cs
    │   │       └── StoreInfoService.cs
    │   ├── Shared.Application/
    │   │   ├── Shared.Application.csproj
    │   │   ├── Common/
    │   │   │   ├── ExportFileResult.cs
    │   │   │   ├── PagedResult.cs
    │   │   │   └── Result.cs
    │   │   └── Interfaces/
    │   │       ├── IAuditLogger.cs
    │   │       ├── INotificationService.cs
    │   │       ├── ITaxConfigService.cs
    │   │       └── IUnitOfWork.cs
    │   ├── Shared.Contracts/
    │   │   ├── Shared.Contracts.csproj
    │   │   └── Events/
    │   │       └── IIntegrationEvent.cs
    │   ├── Shared.Domain/
    │   │   ├── Shared.Domain.csproj
    │   │   ├── Common/
    │   │   │   ├── BaseEntity.cs
    │   │   │   ├── DomainEvent.cs
    │   │   │   └── ValueObject.cs
    │   │   ├── Enums/
    │   │   │   ├── NotificationRole.cs
    │   │   │   └── NotificationType.cs
    │   │   └── Interfaces/
    │   │       └── IRepository.cs
    │   └── Tests/
    │       ├── BiBoGC.Tests.Integration/
    │       │   ├── BiBoGC.Tests.Integration.csproj
    │       │   ├── Fixtures/
    │       │   │   └── PostgresFixture.cs
    │       │   └── Repositories/
    │       │       └── TaxConfigRepositoryTests.cs
    │       └── BiBoGC.Tests.Unit/
    │           ├── BiBoGC.Tests.Unit.csproj
    │           ├── Application/
    │           │   ├── AddBatchCommandHandlerTests.cs
    │           │   ├── AddItemToOrderCommandHandlerTests.cs
    │           │   ├── CancelOrderCommandHandlerTests.cs
    │           │   ├── CompleteOrderCommandHandlerTests.cs
    │           │   ├── CreateExpenseCommandHandlerTests.cs
    │           │   ├── CreateNotificationCommandHandlerTests.cs
    │           │   ├── CreateProductCommandHandlerTests.cs
    │           │   ├── CreateSalesOrderCommandHandlerTests.cs
    │           │   ├── CreateStockTransactionCommandHandlerTests.cs
    │           │   ├── ExportTaxDeclarationHandlerTests.cs
    │           │   ├── GenerateInvoiceCommandHandlerTests.cs
    │           │   ├── GetAnnualRevenueReportHandlerTests.cs
    │           │   ├── GetDailySalesReportHandlerTests.cs
    │           │   ├── GetMonthlyFinancialReportHandlerTests.cs
    │           │   ├── GetTaxConfigQueryHandlerTests.cs
    │           │   ├── LoginCommandHandlerTests.cs
    │           │   ├── MarkAsReadCommandHandlerTests.cs
    │           │   ├── RefreshTokenCommandHandlerTests.cs
    │           │   └── UpdateTaxConfigCommandHandlerTests.cs
    │           ├── Domain/
    │           │   ├── ExpenseTests.cs
    │           │   ├── NotificationTests.cs
    │           │   ├── ProductBatchTests.cs
    │           │   ├── ProductTests.cs
    │           │   ├── RefreshTokenTests.cs
    │           │   ├── SalesOrderTests.cs
    │           │   └── TaxConfigurationTests.cs
    │           └── Helpers/
    │               └── TaxCalculationTests.cs
    ├── DocumentationTemplate/
    │   ├── Khai-thue-ho-kinh-doanh-template.docx
    │   └── S2a-HKD.xlsx
    ├── FrontEnd/
    │   ├── FRONTEND_ARCHITECTURE.md
    │   ├── UI_UX_DESIGN_PLAN.md
    │   └── bibogc/
    │       ├── README.md
    │       ├── analysis_options.yaml
    │       ├── devtools_options.yaml
    │       ├── pubspec.lock
    │       ├── pubspec.yaml
    │       ├── .metadata
    │       ├── android/
    │       │   ├── build.gradle.kts
    │       │   ├── gradle.properties
    │       │   ├── settings.gradle.kts
    │       │   ├── app/
    │       │   │   ├── build.gradle.kts
    │       │   │   └── src/
    │       │   │       ├── main/
    │       │   │       │   ├── AndroidManifest.xml
    │       │   │       │   ├── kotlin/
    │       │   │       │   │   └── com/
    │       │   │       │   │       └── finandfun/
    │       │   │       │   │           └── bibogc/
    │       │   │       │   │               └── MainActivity.kt
    │       │   │       │   └── res/
    │       │   │       │       ├── drawable/
    │       │   │       │       │   └── launch_background.xml
    │       │   │       │       ├── drawable-v21/
    │       │   │       │       │   └── launch_background.xml
    │       │   │       │       ├── values/
    │       │   │       │       │   └── styles.xml
    │       │   │       │       └── values-night/
    │       │   │       │           └── styles.xml
    │       │   │       └── profile/
    │       │   │           └── AndroidManifest.xml
    │       │   └── gradle/
    │       │       └── wrapper/
    │       │           └── gradle-wrapper.properties
    │       ├── ios/
    │       │   ├── Flutter/
    │       │   │   ├── AppFrameworkInfo.plist
    │       │   │   ├── Debug.xcconfig
    │       │   │   └── Release.xcconfig
    │       │   ├── Runner/
    │       │   │   ├── AppDelegate.swift
    │       │   │   ├── Info.plist
    │       │   │   ├── Runner-Bridging-Header.h
    │       │   │   ├── Assets.xcassets/
    │       │   │   │   ├── AppIcon.appiconset/
    │       │   │   │   │   └── Contents.json
    │       │   │   │   └── LaunchImage.imageset/
    │       │   │   │       ├── README.md
    │       │   │   │       └── Contents.json
    │       │   │   └── Base.lproj/
    │       │   │       ├── LaunchScreen.storyboard
    │       │   │       └── Main.storyboard
    │       │   └── RunnerTests/
    │       │       └── RunnerTests.swift
    │       ├── lib/
    │       │   ├── app.dart
    │       │   ├── main.dart
    │       │   ├── core/
    │       │   │   ├── bloc/
    │       │   │   │   └── theme_cubit.dart
    │       │   │   ├── common/
    │       │   │   │   └── paged_result.dart
    │       │   │   ├── config/
    │       │   │   │   ├── app_routes.dart
    │       │   │   │   ├── env_config.dart
    │       │   │   │   ├── router.dart
    │       │   │   │   └── theme.dart
    │       │   │   ├── constants/
    │       │   │   │   ├── api_constants.dart
    │       │   │   │   ├── app_colors.dart
    │       │   │   │   ├── app_radii.dart
    │       │   │   │   └── app_spacing.dart
    │       │   │   ├── di/
    │       │   │   │   ├── injection.config.dart
    │       │   │   │   ├── injection.dart
    │       │   │   │   └── register_module.dart
    │       │   │   ├── error/
    │       │   │   │   └── failures.dart
    │       │   │   ├── events/
    │       │   │   │   └── home_refresh_bus.dart
    │       │   │   ├── network/
    │       │   │   │   ├── dio_client.dart
    │       │   │   │   ├── network_mode.dart
    │       │   │   │   └── network_service.dart
    │       │   │   ├── security/
    │       │   │   │   └── biometric_auth_service.dart
    │       │   │   ├── utils/
    │       │   │   │   ├── currency_utils.dart
    │       │   │   │   ├── dialog_utils.dart
    │       │   │   │   └── thousands_separator_formatter.dart
    │       │   │   └── widgets/
    │       │   │       ├── app_button.dart
    │       │   │       ├── barcode_scanner_sheet.dart
    │       │   │       └── denomination_grid.dart
    │       │   └── features/
    │       │       ├── auth/
    │       │       │   ├── data/
    │       │       │   │   ├── datasources/
    │       │       │   │   │   └── auth_remote_datasource.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── auth_repository_impl.dart
    │       │       │   ├── domain/
    │       │       │   │   └── repositories/
    │       │       │   │       └── auth_repository.dart
    │       │       │   └── presentation/
    │       │       │       ├── bloc/
    │       │       │       │   ├── auth_bloc.dart
    │       │       │       │   ├── auth_event.dart
    │       │       │       │   └── auth_state.dart
    │       │       │       └── pages/
    │       │       │           ├── login_page.dart
    │       │       │           └── welcome_page.dart
    │       │       ├── home/
    │       │       │   └── presentation/
    │       │       │       ├── pages/
    │       │       │       │   └── home_page.dart
    │       │       │       └── widgets/
    │       │       │           ├── home_app_bar.dart
    │       │       │           ├── quick_actions_grid.dart
    │       │       │           ├── recent_activity_list.dart
    │       │       │           └── summary_card.dart
    │       │       ├── inventory/
    │       │       │   ├── data/
    │       │       │   │   ├── datasources/
    │       │       │   │   │   └── inventory_remote_data_source.dart
    │       │       │   │   ├── models/
    │       │       │   │   │   ├── stock_batch_model.dart
    │       │       │   │   │   └── stock_batch_model.g.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── inventory_repository_impl.dart
    │       │       │   ├── domain/
    │       │       │   │   ├── entities/
    │       │       │   │   │   └── stock_batch.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── inventory_repository.dart
    │       │       │   └── presentation/
    │       │       │       ├── bloc/
    │       │       │       │   ├── stock_import_bloc.dart
    │       │       │       │   ├── stock_import_event.dart
    │       │       │       │   └── stock_import_state.dart
    │       │       │       └── pages/
    │       │       │           └── stock_import_page.dart
    │       │       ├── invoice/
    │       │       │   ├── data/
    │       │       │   │   ├── datasources/
    │       │       │   │   │   └── invoice_remote_data_source.dart
    │       │       │   │   ├── models/
    │       │       │   │   │   ├── invoice_model.dart
    │       │       │   │   │   └── invoice_model.g.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── invoice_repository_impl.dart
    │       │       │   ├── domain/
    │       │       │   │   ├── entities/
    │       │       │   │   │   └── invoice.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── invoice_repository.dart
    │       │       │   └── presentation/
    │       │       │       ├── bloc/
    │       │       │       │   ├── invoice_bloc.dart
    │       │       │       │   ├── invoice_event.dart
    │       │       │       │   └── invoice_state.dart
    │       │       │       ├── pages/
    │       │       │       │   ├── invoice_detail_page.dart
    │       │       │       │   └── invoices_page.dart
    │       │       │       └── widgets/
    │       │       │           └── invoice_card.dart
    │       │       ├── notification/
    │       │       │   ├── data/
    │       │       │   │   ├── datasources/
    │       │       │   │   │   └── notification_remote_datasource.dart
    │       │       │   │   ├── models/
    │       │       │   │   │   └── notification_model.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── notification_repository_impl.dart
    │       │       │   ├── domain/
    │       │       │   │   ├── entities/
    │       │       │   │   │   └── notification_entity.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── notification_repository.dart
    │       │       │   └── presentation/
    │       │       │       ├── bloc/
    │       │       │       │   ├── notification_bloc.dart
    │       │       │       │   ├── notification_event.dart
    │       │       │       │   └── notification_state.dart
    │       │       │       └── widgets/
    │       │       │           └── notification_bell.dart
    │       │       ├── product/
    │       │       │   ├── data/
    │       │       │   │   ├── datasources/
    │       │       │   │   │   └── product_remote_data_source.dart
    │       │       │   │   ├── models/
    │       │       │   │   │   ├── product_model.dart
    │       │       │   │   │   └── product_model.g.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── product_repository_impl.dart
    │       │       │   ├── domain/
    │       │       │   │   ├── entities/
    │       │       │   │   │   └── product.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── product_repository.dart
    │       │       │   └── presentation/
    │       │       │       ├── bloc/
    │       │       │       │   ├── product_bloc.dart
    │       │       │       │   ├── product_event.dart
    │       │       │       │   └── product_state.dart
    │       │       │       ├── pages/
    │       │       │       │   ├── product_detail_page.dart
    │       │       │       │   └── products_page.dart
    │       │       │       └── widgets/
    │       │       │           ├── product_card.dart
    │       │       │           └── product_variant_chip.dart
    │       │       ├── sales_order/
    │       │       │   ├── data/
    │       │       │   │   ├── datasources/
    │       │       │   │   │   └── sales_order_remote_data_source.dart
    │       │       │   │   ├── models/
    │       │       │   │   │   ├── sales_order_model.dart
    │       │       │   │   │   └── sales_order_model.g.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── sales_order_repository_impl.dart
    │       │       │   ├── domain/
    │       │       │   │   ├── entities/
    │       │       │   │   │   └── sales_order.dart
    │       │       │   │   └── repositories/
    │       │       │   │       └── sales_order_repository.dart
    │       │       │   └── presentation/
    │       │       │       ├── bloc/
    │       │       │       │   ├── sales_order_bloc.dart
    │       │       │       │   ├── sales_order_event.dart
    │       │       │       │   └── sales_order_state.dart
    │       │       │       ├── pages/
    │       │       │       │   ├── sales_order_detail_page.dart
    │       │       │       │   └── sales_orders_page.dart
    │       │       │       └── widgets/
    │       │       │           ├── add_item_bottom_sheet.dart
    │       │       │           ├── complete_order_dialog.dart
    │       │       │           ├── create_order_bottom_sheet.dart
    │       │       │           └── sales_order_card.dart
    │       │       └── suppliers/
    │       │           ├── data/
    │       │           │   ├── datasources/
    │       │           │   │   └── supplier_remote_data_source.dart
    │       │           │   ├── models/
    │       │           │   │   ├── supplier_model.dart
    │       │           │   │   └── supplier_model.g.dart
    │       │           │   └── repositories/
    │       │           │       └── supplier_repository_impl.dart
    │       │           ├── domain/
    │       │           │   ├── entities/
    │       │           │   │   └── supplier.dart
    │       │           │   └── repositories/
    │       │           │       └── supplier_repository.dart
    │       │           └── presentation/
    │       │               ├── bloc/
    │       │               │   ├── supplier_bloc.dart
    │       │               │   ├── supplier_event.dart
    │       │               │   └── supplier_state.dart
    │       │               ├── pages/
    │       │               │   ├── supplier_detail_page.dart
    │       │               │   └── suppliers_page.dart
    │       │               └── widgets/
    │       │                   ├── supplier_card.dart
    │       │                   └── supplier_form_dialog.dart
    │       ├── test/
    │       │   └── widget_test.dart
    │       └── web/
    │           ├── index.html
    │           └── manifest.json
    └── .github/
        └── workflows/
            ├── Build-Apk.yml
            ├── Build-Test.yml
            └── deploy.yml

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
