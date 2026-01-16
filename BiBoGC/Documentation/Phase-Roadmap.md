# Phase Roadmap

## Overview

BiBo's GC được phát triển theo 3 phases chính:

| Phase   | Focus                | Status       | Timeline |
| ------- | -------------------- | ------------ | -------- |
| Phase 1 | Inventory Management | 80% Complete | Current  |
| Phase 2 | Sales Management     | Planned      | Next     |
| Phase 3 | Advanced Features    | Planned      | Future   |

---

## Phase 1: Inventory Management (Current)

### Goals

- Xây dựng core inventory system
- Quản lý sản phẩm, danh mục, nhà cung cấp
- Theo dõi lô hàng và hạn sử dụng
- Ghi nhận giao dịch kho

### Features

#### ✅ Completed

| Feature               | Description                      | API Endpoints                 |
| --------------------- | -------------------------------- | ----------------------------- |
| Product CRUD          | Quản lý sản phẩm                 | /api/products/\*              |
| Category CRUD         | Quản lý danh mục (nested)        | /api/categories/\*            |
| Supplier CRUD         | Quản lý nhà cung cấp             | /api/suppliers/\*             |
| Batch Management      | Theo dõi lô hàng, HSD            | /api/products/{id}/batches/\* |
| Stock Transactions    | Ghi nhận nhập/xuất/điều chỉnh    | /api/stocktransactions/\*     |
| Data Seeding          | Sample data cho cửa hàng tạp hoá | -                             |
| OpenAPI Documentation | Postman-importable spec          | openapi-bibogc.json           |

#### 🔄 In Progress

| Feature             | Description                                | Status             |
| ------------------- | ------------------------------------------ | ------------------ |
| Transaction Filters | Filter by supplierId, productId, dateRange | Handler cần update |

#### ⏳ Pending

| Feature         | Description                    | Priority |
| --------------- | ------------------------------ | -------- |
| Low Stock Alert | Cảnh báo sản phẩm sắp hết      | Medium   |
| Expiry Alert    | Cảnh báo hàng sắp hết hạn      | High     |
| Stock Summary   | Tổng hợp tồn kho theo category | Medium   |
| Bulk Import     | Import sản phẩm từ Excel/CSV   | Low      |

---

## Phase 2: Sales Management

### Goals

- Xử lý bán hàng và xuất hoá đơn
- Báo cáo doanh thu theo ngày/tháng
- Quản lý chi phí vận hành

### Planned Features

#### Sales Orders

| Feature           | Description                    | Priority |
| ----------------- | ------------------------------ | -------- |
| Create Sale Order | Tạo đơn hàng bán               | High     |
| Order Line Items  | Chi tiết sản phẩm trong đơn    | High     |
| Quick Sale        | Bán nhanh không cần đơn hàng   | High     |
| Order Status      | Draft → Completed → Cancelled  | Medium   |
| Customer Info     | Lưu thông tin khách (optional) | Low      |

#### Invoice

| Feature          | Description             | Priority |
| ---------------- | ----------------------- | -------- |
| Generate Invoice | Tạo hoá đơn từ đơn hàng | High     |
| Invoice Number   | Tự động tạo số hoá đơn  | High     |
| Print Invoice    | In hoá đơn (thermal/A4) | Medium   |
| Invoice History  | Lịch sử hoá đơn         | Medium   |

#### Reports

| Feature                 | Description             | Priority |
| ----------------------- | ----------------------- | -------- |
| Daily Sales Report      | Báo cáo doanh thu ngày  | High     |
| Monthly Sales Report    | Báo cáo doanh thu tháng | High     |
| Top Selling Products    | Sản phẩm bán chạy       | Medium   |
| Low Performing Products | Sản phẩm ít bán         | Medium   |
| Revenue by Category     | Doanh thu theo danh mục | Medium   |

#### Expense Tracking

| Feature                 | Description                              | Priority |
| ----------------------- | ---------------------------------------- | -------- |
| Record Expense          | Ghi nhận chi phí                         | High     |
| Expense Categories      | Phân loại chi phí (điện, nước, thuê,...) | High     |
| Daily Expense Summary   | Tổng chi phí ngày                        | Medium   |
| Monthly Expense Report  | Báo cáo chi phí tháng                    | Medium   |
| Profit/Loss Calculation | Tính lãi/lỗ                              | High     |

### Database Schema (Planned)

```sql
-- Sales Orders
CREATE TABLE sales_orders (
    id UUID PRIMARY KEY,
    order_number VARCHAR(20) UNIQUE,
    order_date TIMESTAMP,
    customer_name VARCHAR(200),
    customer_phone VARCHAR(20),
    subtotal DECIMAL(18,2),
    discount DECIMAL(18,2) DEFAULT 0,
    tax DECIMAL(18,2) DEFAULT 0,
    total_amount DECIMAL(18,2),
    status INTEGER, -- 1=Draft, 2=Completed, 3=Cancelled
    notes TEXT,
    created_at TIMESTAMP,
    ...
);

-- Order Items
CREATE TABLE sale_order_items (
    id UUID PRIMARY KEY,
    order_id UUID REFERENCES sales_orders(id),
    product_id UUID REFERENCES products(id),
    product_batch_id UUID REFERENCES product_batches(id),
    product_name VARCHAR(200),
    sku VARCHAR(20),
    quantity INTEGER,
    unit_price DECIMAL(18,2),
    discount DECIMAL(18,2) DEFAULT 0,
    line_total DECIMAL(18,2),
    ...
);

-- Expenses
CREATE TABLE expenses (
    id UUID PRIMARY KEY,
    expense_date DATE,
    category VARCHAR(100),
    amount DECIMAL(18,2),
    description VARCHAR(500),
    receipt_number VARCHAR(50),
    paid_to VARCHAR(200),
    payment_method VARCHAR(50),
    created_at TIMESTAMP,
    ...
);
```

---

## Phase 3: Advanced Features

### Goals

- Tích hợp barcode scanning
- Xuất hoá đơn PDF chuyên nghiệp
- Tính toán thuế và báo cáo thuế
- Phân tích doanh thu hằng năm

### Planned Features

#### Barcode Integration

| Feature                | Description                  | Priority |
| ---------------------- | ---------------------------- | -------- |
| Scan to Add            | Quét barcode để thêm vào đơn | High     |
| Assign Barcode         | Gán barcode cho sản phẩm     | High     |
| Generate Barcode       | Tạo barcode cho SP chưa có   | Medium   |
| Barcode Label Printing | In nhãn barcode              | Medium   |

#### PDF Export

| Feature         | Description           | Priority |
| --------------- | --------------------- | -------- |
| Invoice PDF     | Xuất hoá đơn dạng PDF | High     |
| Report PDF      | Xuất báo cáo PDF      | Medium   |
| Custom Template | Mẫu hoá đơn tuỳ chỉnh | Low      |
| Batch Export    | Xuất nhiều hoá đơn    | Low      |

#### Tax Handling

| Feature           | Description            | Priority |
| ----------------- | ---------------------- | -------- |
| VAT Configuration | Cấu hình thuế VAT      | High     |
| Tax Calculation   | Tự động tính thuế      | High     |
| Tax Report        | Báo cáo thuế tháng/quý | High     |
| Tax Categories    | Phân loại thuế theo SP | Medium   |

#### Annual Analytics

| Feature           | Description                  | Priority |
| ----------------- | ---------------------------- | -------- |
| Yearly Revenue    | Doanh thu cả năm             | High     |
| YoY Comparison    | So sánh năm này vs năm trước | Medium   |
| Revenue Forecast  | Dự đoán doanh thu            | Low      |
| Seasonal Analysis | Phân tích theo mùa           | Low      |

#### System Improvements

| Feature           | Description             | Priority |
| ----------------- | ----------------------- | -------- |
| Authentication    | JWT login/register      | High     |
| Role-based Access | Owner, Staff, Viewer    | High     |
| Audit Log         | Lịch sử thao tác        | Medium   |
| Data Backup       | Backup/restore database | High     |
| Multi-store       | Hỗ trợ nhiều cửa hàng   | Low      |

---

## Technical Debt & Bug Fixes

### Phase 1 Debt

| Item                | Description                                      | Priority |
| ------------------- | ------------------------------------------------ | -------- |
| Transaction Filters | Handler không dùng supplierId, productId filters | High     |
| CS8618 Warnings     | Non-nullable property warnings                   | Low      |
| Unit Tests          | Chưa có automated tests                          | Medium   |

### Planned Refactoring

| Item                  | Description             | Phase |
| --------------------- | ----------------------- | ----- |
| Generic Repository    | Base repository pattern | 2     |
| Specification Pattern | Complex query handling  | 2     |
| Event Sourcing        | Audit trail via events  | 3     |

---

## Timeline Estimate

```
2026
├── Q1: Phase 1 Completion
│   ├── Fix remaining bugs
│   ├── Add alerts (low stock, expiry)
│   └── Unit tests
│
├── Q2: Phase 2 - Sales
│   ├── Sales orders & invoices
│   ├── Daily/Monthly reports
│   └── Expense tracking
│
├── Q3: Phase 2 - Polish
│   ├── UI development (separate project)
│   ├── Integration testing
│   └── Performance optimization
│
└── Q4: Phase 3 - Advanced
    ├── Barcode integration
    ├── PDF export
    └── Tax handling

2027
├── Q1: Phase 3 Completion
│   ├── Annual analytics
│   └── Authentication & authorization
│
└── Q2: Production Release
    ├── Final testing
    ├── Documentation
    └── Deployment
```

---

## Success Metrics

### Phase 1

- [ ] All CRUD operations working
- [ ] No critical bugs
- [ ] API documentation complete
- [ ] Test coverage > 50%

### Phase 2

- [ ] Complete sales workflow
- [ ] Accurate financial reports
- [ ] Response time < 500ms

### Phase 3

- [ ] Barcode scan < 1s
- [ ] PDF generation < 3s
- [ ] 99.9% uptime

---

_Document Version: 1.0 | Last Updated: January 2026_
