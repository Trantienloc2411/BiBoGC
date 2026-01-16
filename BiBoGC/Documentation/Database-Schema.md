# Database Schema

## Overview

Database: **PostgreSQL 16**  
ORM: **Entity Framework Core 10**  
Naming Convention: **snake_case** (PostgreSQL)

---

## Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐
│   Categories    │       │    Suppliers    │
├─────────────────┤       ├─────────────────┤
│ id (PK)         │       │ id (PK)         │
│ name            │       │ name            │
│ description     │       │ contact_person  │
│ parent_id (FK)  │───┐   │ phone_number    │
│ is_active       │   │   │ address         │
│ display_order   │   │   │ is_active       │
│ created_at      │   │   │ created_at      │
│ updated_at      │◄──┘   │ updated_at      │
│ is_deleted      │       │ is_deleted      │
└────────┬────────┘       └────────┬────────┘
         │                         │
         │ 1:N                     │ 1:N
         │                         │
         ▼                         │
┌─────────────────┐                │
│    Products     │                │
├─────────────────┤                │
│ id (PK)         │                │
│ name            │                │
│ sku (unique)    │                │
│ price           │                │
│ description     │                │
│ status          │                │
│ requires_batch  │                │
│ category_id(FK) │                │
│ created_at      │                │
│ updated_at      │                │
│ is_deleted      │                │
└────────┬────────┘                │
         │                         │
         │ 1:N                     │
         │                         │
         ▼                         │
┌─────────────────┐                │
│ ProductBatches  │                │
├─────────────────┤                │
│ id (PK)         │                │
│ product_id (FK) │                │
│ batch_number    │                │
│ quantity        │                │
│ manufacturing_  │                │
│   date          │                │
│ expiration_date │                │
│ cost_price      │                │
│ created_at      │                │
│ updated_at      │                │
│ is_deleted      │                │
└────────┬────────┘                │
         │                         │
         │ 1:N                     │
         │                         │
         ▼                         ▼
┌──────────────────────────────────────────┐
│           StockTransactions              │
├──────────────────────────────────────────┤
│ id (PK)                                  │
│ product_id (FK)                          │
│ product_batch_id (FK, nullable)          │
│ supplier_id (FK, nullable)               │
│ transaction_type                         │
│ quantity                                 │
│ unit_price                               │
│ transaction_date                         │
│ notes                                    │
│ created_at                               │
│ updated_at                               │
│ is_deleted                               │
└──────────────────────────────────────────┘
```

---

## Tables Detail

### Categories

```sql
CREATE TABLE categories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL,
    description     VARCHAR(500),
    parent_id       UUID REFERENCES categories(id),
    is_active       BOOLEAN DEFAULT true,
    display_order   INTEGER DEFAULT 0,
    created_at      TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at      TIMESTAMP WITH TIME ZONE,
    is_deleted      BOOLEAN DEFAULT false
);

-- Indexes
CREATE INDEX ix_categories_parent_id ON categories(parent_id);
CREATE INDEX ix_categories_is_deleted ON categories(is_deleted);
```

### Products

```sql
CREATE TABLE products (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(200) NOT NULL,
    sku                 VARCHAR(20) NOT NULL UNIQUE,
    price               DECIMAL(18,2) NOT NULL,
    description         VARCHAR(1000),
    status              INTEGER NOT NULL DEFAULT 1, -- 1=Active, 2=Discontinued, 3=OutOfStock
    requires_batch_tracking BOOLEAN DEFAULT false,
    category_id         UUID REFERENCES categories(id),
    created_at          TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at          TIMESTAMP WITH TIME ZONE,
    is_deleted          BOOLEAN DEFAULT false
);

-- Indexes
CREATE UNIQUE INDEX ix_products_sku ON products(sku) WHERE is_deleted = false;
CREATE INDEX ix_products_category_id ON products(category_id);
CREATE INDEX ix_products_status ON products(status);
CREATE INDEX ix_products_is_deleted ON products(is_deleted);
```

### ProductBatches

```sql
CREATE TABLE product_batches (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id          UUID NOT NULL REFERENCES products(id),
    batch_number        VARCHAR(50) NOT NULL,
    quantity            INTEGER NOT NULL DEFAULT 0,
    manufacturing_date  TIMESTAMP WITH TIME ZONE NOT NULL,
    expiration_date     TIMESTAMP WITH TIME ZONE NOT NULL,
    cost_price          DECIMAL(18,2) DEFAULT 0,
    created_at          TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at          TIMESTAMP WITH TIME ZONE,
    is_deleted          BOOLEAN DEFAULT false
);

-- Indexes
CREATE UNIQUE INDEX ix_batches_product_batch ON product_batches(product_id, batch_number)
    WHERE is_deleted = false;
CREATE INDEX ix_batches_product_id ON product_batches(product_id);
CREATE INDEX ix_batches_expiration ON product_batches(expiration_date);
CREATE INDEX ix_batches_is_deleted ON product_batches(is_deleted);
```

### Suppliers

```sql
CREATE TABLE suppliers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(200) NOT NULL,
    contact_person  VARCHAR(100),
    phone_number    VARCHAR(20),
    address         VARCHAR(500),
    is_active       BOOLEAN DEFAULT true,
    created_at      TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at      TIMESTAMP WITH TIME ZONE,
    is_deleted      BOOLEAN DEFAULT false
);

-- Indexes
CREATE INDEX ix_suppliers_is_active ON suppliers(is_active);
CREATE INDEX ix_suppliers_is_deleted ON suppliers(is_deleted);
```

### StockTransactions

```sql
CREATE TABLE stock_transactions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id          UUID NOT NULL REFERENCES products(id),
    product_batch_id    UUID REFERENCES product_batches(id),
    supplier_id         UUID REFERENCES suppliers(id),
    transaction_type    INTEGER NOT NULL,
    quantity            INTEGER NOT NULL,
    unit_price          DECIMAL(18,2) DEFAULT 0,
    transaction_date    TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    notes               VARCHAR(500),
    created_at          TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at          TIMESTAMP WITH TIME ZONE,
    is_deleted          BOOLEAN DEFAULT false
);

-- Indexes
CREATE INDEX ix_transactions_product_id ON stock_transactions(product_id);
CREATE INDEX ix_transactions_supplier_id ON stock_transactions(supplier_id);
CREATE INDEX ix_transactions_batch_id ON stock_transactions(product_batch_id);
CREATE INDEX ix_transactions_type ON stock_transactions(transaction_type);
CREATE INDEX ix_transactions_date ON stock_transactions(transaction_date DESC);
CREATE INDEX ix_transactions_is_deleted ON stock_transactions(is_deleted);
```

---

## Enums

### ProductStatus

| Value | Name         | Description      |
| ----- | ------------ | ---------------- |
| 1     | Active       | Đang kinh doanh  |
| 2     | Discontinued | Ngừng kinh doanh |
| 3     | OutOfStock   | Hết hàng         |

### StockTransactionType

| Value | Name           | Description      | Stock Impact |
| ----- | -------------- | ---------------- | ------------ |
| 1     | Purchase       | Nhập hàng từ NCC | + (Increase) |
| 2     | Sale           | Bán hàng         | - (Decrease) |
| 3     | AdjustmentIn   | Điều chỉnh tăng  | + (Increase) |
| 4     | AdjustmentOut  | Điều chỉnh giảm  | - (Decrease) |
| 5     | Damage         | Hàng hư hỏng     | - (Decrease) |
| 6     | Expiry         | Hàng hết hạn     | - (Decrease) |
| 7     | Return         | Khách trả hàng   | + (Increase) |
| 8     | SupplierReturn | Trả hàng NCC     | - (Decrease) |

---

## Soft Delete Pattern

Tất cả các entity đều implement soft delete:

- Field `is_deleted` (boolean, default: false)
- Khi xóa: `is_deleted = true`, `updated_at = NOW()`
- Tất cả queries đều filter `WHERE is_deleted = false`

```csharp
// BaseEntity
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

---

## EF Core Configurations

### Product Configuration

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Value)
                .HasColumnName("price")
                .HasPrecision(18, 2);
        });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId);

        builder.HasMany(p => p.Batches)
            .WithOne(b => b.Product)
            .HasForeignKey(b => b.ProductId);
    }
}
```

---

## Migration Commands

```bash
# Add new migration
dotnet ef migrations add MigrationName -p InventoryManagement.Infrastructure -s BiBoGC

# Update database
dotnet ef database update -p InventoryManagement.Infrastructure -s BiBoGC

# Generate SQL script
dotnet ef migrations script -p InventoryManagement.Infrastructure -s BiBoGC -o script.sql
```

---

## Future Tables (Phase 2 & 3)

### SalesOrders (Phase 2)

```sql
-- Planned for Phase 2
CREATE TABLE sales_orders (
    id              UUID PRIMARY KEY,
    order_number    VARCHAR(20) NOT NULL UNIQUE,
    order_date      TIMESTAMP WITH TIME ZONE,
    customer_name   VARCHAR(200),
    total_amount    DECIMAL(18,2),
    discount        DECIMAL(18,2) DEFAULT 0,
    tax             DECIMAL(18,2) DEFAULT 0,
    status          INTEGER, -- Draft, Completed, Cancelled
    ...
);
```

### Expenses (Phase 2)

```sql
-- Planned for Phase 2
CREATE TABLE expenses (
    id              UUID PRIMARY KEY,
    expense_date    DATE NOT NULL,
    category        VARCHAR(100),
    amount          DECIMAL(18,2) NOT NULL,
    description     VARCHAR(500),
    ...
);
```

---

_Document Version: 1.0 | Last Updated: January 2026_
