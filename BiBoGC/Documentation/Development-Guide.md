# Development Guide

## Prerequisites

### Required Software

| Software       | Version                   | Download                                                |
| -------------- | ------------------------- | ------------------------------------------------------- |
| .NET SDK       | 10.0+                     | https://dotnet.microsoft.com/download                   |
| Docker Desktop | Latest                    | https://www.docker.com/products/docker-desktop          |
| PostgreSQL     | 16+                       | https://www.postgresql.org/download/ (hoặc dùng Docker) |
| IDE            | VS 2022 / VS Code / Rider | -                                                       |

### Recommended VS Code Extensions

- C# Dev Kit
- PostgreSQL (by Chris Kolkman)
- REST Client
- Docker

---

## Getting Started

### 1. Clone Repository

```bash
git clone https://github.com/[your-repo]/BiBo-s-GC.git
cd BiBo-s-GC/BiBoGC
```

### 2. Run with .NET Aspire (Recommended)

```bash
# Aspire sẽ tự động khởi động PostgreSQL container
dotnet run --project BiBoGC.AppHost
```

### 3. Access Points

| Service          | URL                           |
| ---------------- | ----------------------------- |
| API              | http://localhost:5020         |
| Swagger          | http://localhost:5020/swagger |
| Aspire Dashboard | https://localhost:17062       |

### 4. Alternative: Manual Setup

```bash
# Start PostgreSQL (Docker)
docker run -d --name bibogc-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=bibogc \
  -p 5432:5432 \
  postgres:16

# Update connection string in appsettings.Development.json
# Run API
dotnet run --project BiBoGC
```

---

## Project Structure

```
BiBoGC/
├── BiBoGC/                          # 🌐 API Project
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   ├── CategoriesController.cs
│   │   ├── SuppliersController.cs
│   │   └── StockTransactionsController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
│
├── InventoryManagement.Domain/      # 📦 Domain Layer
│   ├── Entities/
│   │   ├── Product.cs
│   │   ├── ProductBatch.cs
│   │   ├── Category.cs
│   │   ├── Supplier.cs
│   │   └── StockTransaction.cs
│   ├── ValueObjects/
│   │   ├── Sku.cs
│   │   ├── Money.cs
│   │   └── Quantity.cs
│   └── Enums/
│       ├── ProductStatuses.cs
│       └── StockTransactionType.cs
│
├── InventoryManagement.Application/ # ⚙️ Application Layer
│   ├── Commands/
│   │   ├── CreateProduct/
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   └── CreateProductCommandValidator.cs
│   │   └── ...
│   ├── Queries/
│   │   ├── GetProducts/
│   │   │   ├── GetProductsQuery.cs
│   │   │   ├── GetProductsQueryHandler.cs
│   │   │   └── GetProductsQueryValidator.cs
│   │   └── ...
│   ├── DTOs/
│   │   ├── ProductDto.cs
│   │   └── ...
│   └── Interfaces/
│       ├── IProductRepository.cs
│       └── ...
│
└── InventoryManagement.Infrastructure/ # 🔧 Infrastructure Layer
    ├── Data/
    │   ├── InventoryDbContext.cs
    │   ├── DataSeeder.cs
    │   └── Configurations/
    │       └── ProductConfiguration.cs
    └── Repositories/
        ├── ProductRepository.cs
        └── ...
```

---

## Development Workflow

### Adding New Feature

#### 1. Create Domain Entity (if needed)

```csharp
// InventoryManagement.Domain/Entities/NewEntity.cs
public class NewEntity : BaseEntity
{
    public string Name { get; private set; }

    private NewEntity() { } // EF Core

    public NewEntity(string name)
    {
        Name = name;
    }
}
```

#### 2. Create Repository Interface

```csharp
// InventoryManagement.Application/Interfaces/INewEntityRepository.cs
public interface INewEntityRepository
{
    Task<NewEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<NewEntity> AddAsync(NewEntity entity, CancellationToken ct);
}
```

#### 3. Create Command/Query + Handler + Validator

```csharp
// Command
public record CreateNewEntityCommand : IRequest<Result<NewEntityDto>>
{
    public string Name { get; init; } = string.Empty;
}

// Handler
public class CreateNewEntityCommandHandler
    : IRequestHandler<CreateNewEntityCommand, Result<NewEntityDto>>
{
    private readonly INewEntityRepository _repository;

    public async Task<Result<NewEntityDto>> Handle(
        CreateNewEntityCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new NewEntity(request.Name);
        await _repository.AddAsync(entity, cancellationToken);
        return Result<NewEntityDto>.Success(MapToDto(entity));
    }
}

// Validator
public class CreateNewEntityCommandValidator
    : AbstractValidator<CreateNewEntityCommand>
{
    public CreateNewEntityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống")
            .MaximumLength(200).WithMessage("Tên tối đa 200 ký tự");
    }
}
```

#### 4. Implement Repository

```csharp
// InventoryManagement.Infrastructure/Repositories/NewEntityRepository.cs
public class NewEntityRepository : INewEntityRepository
{
    private readonly InventoryDbContext _context;

    public async Task<NewEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.NewEntities
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
    }
}
```

#### 5. Register in DI

```csharp
// InventoryManagement.Infrastructure/DependencyInjection.cs
services.AddScoped<INewEntityRepository, NewEntityRepository>();
```

#### 6. Create Controller Endpoint

```csharp
// BiBoGC/Controllers/NewEntitiesController.cs
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateNewEntityCommand command)
{
    var result = await _mediator.Send(command);
    if (!result.IsSuccess)
        return BadRequest(result.Errors);
    return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
}
```

---

## Database Migrations

### Create Migration

```bash
cd BiBoGC

# Add migration
dotnet ef migrations add AddNewEntity \
  -p InventoryManagement.Infrastructure \
  -s BiBoGC

# Apply migration
dotnet ef database update \
  -p InventoryManagement.Infrastructure \
  -s BiBoGC
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName \
  -p InventoryManagement.Infrastructure \
  -s BiBoGC
```

---

## Coding Conventions

### Naming

| Type          | Convention  | Example             |
| ------------- | ----------- | ------------------- |
| Class         | PascalCase  | `ProductRepository` |
| Method        | PascalCase  | `GetByIdAsync`      |
| Property      | PascalCase  | `ProductName`       |
| Private field | \_camelCase | `_context`          |
| Parameter     | camelCase   | `productId`         |
| Constant      | UPPER_CASE  | `MAX_PAGE_SIZE`     |

### File Organization

```
Commands/
├── CreateProduct/
│   ├── CreateProductCommand.cs      # Command record
│   ├── CreateProductCommandHandler.cs # Handler
│   └── CreateProductCommandValidator.cs # Validator
```

### API Response Conventions

- Success: Return entity/DTO directly
- Created: Return 201 with Location header
- Not Found: Return 404 with ProblemDetails
- Validation Error: Return 400 with ValidationProblemDetails
- Delete: Return 204 No Content

---

## Debugging

### View SQL Queries

```csharp
// In Program.cs or DbContext
optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
// Or
optionsBuilder.EnableSensitiveDataLogging();
```

### Common Issues

#### 1. EF Core Include Error

```
Error: The expression 's.Transactions' is invalid inside an 'Include' operation
```

**Solution:** Tách filtered include thành separate queries hoặc không dùng complex expressions trong Include.

#### 2. Concurrency Token Error

```
Error: The property 'RowVersion' on entity type 'X' is configured as a concurrency token
```

**Solution:** PostgreSQL dùng `xmin` system column thay vì RowVersion.

#### 3. Null Reference in Navigation Property

```
Error: Object reference not set to an instance of an object
```

**Solution:** Ensure `.Include()` is called for required navigation properties.

---

## Testing (Coming Soon)

### Unit Tests

```csharp
// Tests/Application.UnitTests/
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-001",
            Price = 10000
        }, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
```

### Integration Tests

```csharp
// Tests/Api.IntegrationTests/
public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    [Fact]
    public async Task GetProducts_ReturnsOkResult()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## Git Workflow

### Branch Naming

```
feature/add-sales-orders
bugfix/fix-stock-calculation
hotfix/critical-bug
```

### Commit Message Format

```
feat: add sales order creation endpoint
fix: correct stock quantity calculation
docs: update API documentation
refactor: extract validation logic
test: add unit tests for ProductHandler
```

---

_Document Version: 1.0 | Last Updated: January 2026_
