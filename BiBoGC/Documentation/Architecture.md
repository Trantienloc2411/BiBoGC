# System Architecture

## Overview

BiBo's GC được xây dựng theo **Clean Architecture** kết hợp với **CQRS pattern** (Command Query Responsibility Segregation).

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│                   (BiBoGC - Web API)                        │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐   │
│  │ Controllers │ │ Middleware  │ │ API Models          │   │
│  └─────────────┘ └─────────────┘ └─────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│           (InventoryManagement.Application)                  │
│  ┌──────────┐ ┌─────────┐ ┌──────────┐ ┌─────────────────┐ │
│  │ Commands │ │ Queries │ │ Handlers │ │ Validators      │ │
│  └──────────┘ └─────────┘ └──────────┘ └─────────────────┘ │
│  ┌──────────┐ ┌──────────────────────┐                     │
│  │   DTOs   │ │ Repository Interfaces│                     │
│  └──────────┘ └──────────────────────┘                     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│             (InventoryManagement.Domain)                     │
│  ┌──────────┐ ┌──────────────┐ ┌─────────────────────────┐ │
│  │ Entities │ │ Value Objects│ │ Domain Events           │ │
│  └──────────┘ └──────────────┘ └─────────────────────────┘ │
│  ┌──────────┐ ┌──────────────┐                             │
│  │  Enums   │ │  Exceptions  │                             │
│  └──────────┘ └──────────────┘                             │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│          (InventoryManagement.Infrastructure)                │
│  ┌─────────────┐ ┌──────────────────┐ ┌─────────────────┐  │
│  │ DbContext   │ │ Configurations   │ │ Repositories    │  │
│  └─────────────┘ └──────────────────┘ └─────────────────┘  │
│  ┌─────────────┐ ┌──────────────────┐                      │
│  │ Migrations  │ │ Data Seeder      │                      │
│  └─────────────┘ └──────────────────┘                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Database                               │
│                     (PostgreSQL)                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)

Tách biệt operations thành Commands (write) và Queries (read):

```
Commands (Write)                    Queries (Read)
├── CreateProductCommand            ├── GetProductQuery
├── UpdateProductCommand            ├── GetProductsQuery
├── DeleteProductCommand            ├── GetBatchQuery
├── AddBatchCommand                 └── GetBatchesQuery
└── CreateStockTransactionCommand
```

**Lợi ích:**

- Separation of concerns
- Dễ dàng scale read/write independently
- Simplified testing

### 2. Repository Pattern

```csharp
// Interface (Application Layer)
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<(IEnumerable<Product>, int)> GetAllAsync(...);
    Task<Product> AddAsync(Product product, CancellationToken ct);
    Task UpdateAsync(Product product, CancellationToken ct);
    Task DeleteAsync(Product product, CancellationToken ct);
}

// Implementation (Infrastructure Layer)
public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;
    // Implementation details...
}
```

### 3. MediatR Pipeline

```
Request → Validation Behavior → Handler → Response
              │
              ▼
        FluentValidation
```

```csharp
// Validation Behavior
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    // Validates request before handler execution
}
```

### 4. Domain-Driven Design (DDD) Concepts

#### Entities

```csharp
public class Product : BaseEntity
{
    public string Name { get; private set; }
    public Sku Sku { get; private set; }
    public Money Price { get; private set; }

    // Domain logic
    public void UpdatePrice(Money newPrice) { ... }
    public void Discontinue() { ... }
}
```

#### Value Objects

```csharp
public record Sku : ValueObject
{
    public string Value { get; }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty");
        Value = value.ToUpperInvariant();
    }
}

public record Money : ValueObject
{
    public decimal Value { get; }
    public string Currency { get; } = "VND";
}
```

#### Domain Events

```csharp
public record ProductCreatedEvent(Guid ProductId) : DomainEvent;
public record ProductPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice) : DomainEvent;
public record BatchAddedEvent(Guid BatchId, Guid ProductId, int Quantity) : DomainEvent;
```

---

## Data Flow

### Command Flow (Write Operation)

```
1. Client sends POST/PUT/DELETE request
2. Controller receives request
3. Controller creates Command object
4. MediatR dispatches to Handler
5. ValidationBehavior validates Command
6. Handler executes business logic
7. Repository persists to database
8. Response returned to client
```

### Query Flow (Read Operation)

```
1. Client sends GET request
2. Controller receives request
3. Controller creates Query object
4. MediatR dispatches to Handler
5. Handler calls Repository
6. Repository queries database (with AsNoTracking)
7. Handler maps to DTO
8. Response returned to client
```

---

## Key Components

### Dependency Injection Setup

```csharp
// Application Layer
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
});
services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Infrastructure Layer
services.AddDbContext<InventoryDbContext>(...);
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ISupplierRepository, SupplierRepository>();
services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
services.AddScoped<IProductBatchRepository, ProductBatchRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
```

### Exception Handling

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            // Return 400 with validation errors
        }
        catch (NotFoundException ex)
        {
            // Return 404
        }
        catch (Exception ex)
        {
            // Return 500 with generic error
        }
    }
}
```

---

## Deployment Architecture

### Development (Local)

```
┌─────────────────────────────────────────┐
│           .NET Aspire Host              │
│  ┌─────────────┐  ┌─────────────────┐  │
│  │  BiBoGC API │  │  PostgreSQL     │  │
│  │  :5020      │  │  (Container)    │  │
│  └─────────────┘  └─────────────────┘  │
│         │                  │            │
│         └────────┬─────────┘            │
│                  │                      │
│         ┌────────▼────────┐            │
│         │ Aspire Dashboard│            │
│         │ :17062          │            │
│         └─────────────────┘            │
└─────────────────────────────────────────┘
```

### Production (Planned)

```
┌──────────────────────────────────────────────────────┐
│                    Local Network                      │
│  ┌─────────────┐                                     │
│  │ Mini PC     │                                     │
│  │ (Server)    │                                     │
│  │  ┌────────┐ │   ┌──────────┐  ┌──────────┐      │
│  │  │ Docker │◄├───┤ Client 1 │  │ Client 2 │      │
│  │  │ Stack  │ │   │ (POS)    │  │ (Mobile) │      │
│  │  └────────┘ │   └──────────┘  └──────────┘      │
│  └─────────────┘                                     │
└──────────────────────────────────────────────────────┘
```

---

## Security Considerations

### Current (Phase 1)

- Input validation with FluentValidation
- Soft delete (data preservation)
- Exception handling middleware

### Future Phases

- JWT Authentication
- Role-based authorization (Owner, Staff)
- API rate limiting
- Audit logging

---

_Document Version: 1.0 | Last Updated: January 2026_
