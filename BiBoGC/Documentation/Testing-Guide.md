# Testing Guide

## Overview

Tài liệu này hướng dẫn cách test hệ thống BiBo's GC, bao gồm manual testing với Postman và automated testing (planned).

---

## Test Environment Setup

### 1. Khởi động hệ thống

```bash
cd BiBoGC
dotnet run --project BiBoGC.AppHost
```

### 2. Import Postman Collection

1. Mở Postman
2. Click **Import**
3. Chọn file `openapi-bibogc.json`
4. Collection sẽ được tạo với tất cả endpoints

### 3. Base URL

```
http://localhost:5020
```

---

## Test Data (Seed Data)

Khi khởi động lần đầu, hệ thống tự động seed data mẫu:

### Categories (17 records)

| ID  | Name      | Parent    |
| --- | --------- | --------- |
| -   | Đồ uống   | (root)    |
| -   | Nước ngọt | Đồ uống   |
| -   | Sữa       | Đồ uống   |
| -   | Thực phẩm | (root)    |
| -   | Mì gói    | Thực phẩm |
| ... | ...       | ...       |

### Suppliers (12 records)

| Name                            | Contact    |
| ------------------------------- | ---------- |
| Công ty TNHH Coca-Cola Việt Nam | 0283456789 |
| Tập đoàn Vinamilk               | 0287654321 |
| Công ty CP Acecook Việt Nam     | 0283334455 |
| ...                             | ...        |

### Products (21 records)

| Name                    | SKU        | Price  | Category  |
| ----------------------- | ---------- | ------ | --------- |
| Coca Cola lon 330ml     | COCA-330   | 12,000 | Nước ngọt |
| Pepsi lon 330ml         | PEPSI-330  | 11,000 | Nước ngọt |
| Mì Hảo Hảo tôm chua cay | HAOHAO-TCC | 4,000  | Mì gói    |
| ...                     | ...        | ...    | ...       |

---

## Manual Test Cases

### TC-CAT-001: Get All Categories

**Endpoint:** `GET /api/categories`

| Step | Action                        | Expected Result             |
| ---- | ----------------------------- | --------------------------- |
| 1    | Send GET request              | Status 200                  |
| 2    | Check response                | Array of categories         |
| 3    | Verify parent-child structure | Categories có subcategories |

**Sample Request:**

```http
GET http://localhost:5020/api/categories
```

---

### TC-CAT-002: Create Category

**Endpoint:** `POST /api/categories`

| Step | Action                        | Expected Result              |
| ---- | ----------------------------- | ---------------------------- |
| 1    | Send POST with valid data     | Status 201                   |
| 2    | Send POST with empty name     | Status 400, validation error |
| 3    | Send POST with duplicate name | Status 400, error message    |

**Valid Request:**

```json
{
  "name": "Test Category",
  "description": "Test description",
  "parentCategoryId": null
}
```

**Invalid Request (empty name):**

```json
{
  "name": "",
  "description": "Test"
}
```

Expected: `400 Bad Request` with message "Tên danh mục không được để trống"

---

### TC-PRD-001: Get Products with Pagination

**Endpoint:** `GET /api/products`

| Step | Action                       | Expected Result              |
| ---- | ---------------------------- | ---------------------------- |
| 1    | GET without params           | Page 1, 10 items             |
| 2    | GET ?pageNumber=2&pageSize=5 | Page 2, 5 items              |
| 3    | GET ?searchTerm=coca         | Products matching "coca"     |
| 4    | GET ?pageNumber=0            | Status 400, validation error |
| 5    | GET ?pageSize=200            | Status 400, max is 100       |

**Sample Requests:**

```http
# Default
GET http://localhost:5020/api/products

# Custom pagination
GET http://localhost:5020/api/products?pageNumber=2&pageSize=5

# Search
GET http://localhost:5020/api/products?searchTerm=coca
```

---

### TC-PRD-002: Create Product

**Endpoint:** `POST /api/products`

| Test Case              | Data             | Expected             |
| ---------------------- | ---------------- | -------------------- |
| Valid product          | name, sku, price | 201 Created          |
| Missing name           | sku, price only  | 400 - name required  |
| Missing SKU            | name, price only | 400 - sku required   |
| Duplicate SKU          | existing SKU     | 400 - SKU exists     |
| Negative price         | price: -1000     | 400 - price >= 0     |
| SKU with special chars | "SKU@#$"         | 400 - invalid format |

**Valid Request:**

```json
{
  "name": "Test Product",
  "sku": "TEST-001",
  "price": 15000,
  "description": "Test description",
  "requiresBatchTracking": true
}
```

---

### TC-PRD-003: Get Product by ID

**Endpoint:** `GET /api/products/{id}`

| Test Case           | ID          | Expected                 |
| ------------------- | ----------- | ------------------------ |
| Existing product    | valid GUID  | 200 with product details |
| Non-existing        | random GUID | 404 Not Found            |
| Invalid GUID format | "invalid"   | 400 Bad Request          |

---

### TC-BTH-001: Add Batch to Product

**Endpoint:** `POST /api/products/{productId}/batches`

| Test Case                   | Data            | Expected                   |
| --------------------------- | --------------- | -------------------------- |
| Valid batch                 | all fields      | 201 Created                |
| Future manufacturing date   | date > today    | 400 - invalid date         |
| Expiry before manufacturing | exp < mfg       | 400 - expiry must be after |
| Duplicate batch number      | existing number | 400 - batch exists         |
| Quantity = 0                | quantity: 0     | 400 - must be > 0          |

**Valid Request:**

```json
{
  "batchNumber": "BATCH-TEST-001",
  "quantity": 100,
  "manufacturingDate": "2026-01-01",
  "expirationDate": "2027-01-01",
  "costPrice": 10000
}
```

---

### TC-SUP-001: Supplier CRUD

**Endpoints:** `/api/suppliers/*`

| Operation  | Endpoint                            | Expected              |
| ---------- | ----------------------------------- | --------------------- |
| List all   | GET /api/suppliers                  | 200, paginated list   |
| Get by ID  | GET /api/suppliers/{id}             | 200 with transactions |
| Create     | POST /api/suppliers                 | 201 Created           |
| Update     | PUT /api/suppliers/{id}             | 200 Updated           |
| Delete     | DELETE /api/suppliers/{id}          | 204 No Content        |
| Activate   | POST /api/suppliers/{id}/activate   | 200                   |
| Deactivate | POST /api/suppliers/{id}/deactivate | 200                   |

---

### TC-TXN-001: Create Stock Transaction

**Endpoint:** `POST /api/stocktransactions`

| Test Case                 | Transaction Type | Required Fields                 | Expected                |
| ------------------------- | ---------------- | ------------------------------- | ----------------------- |
| Purchase                  | 1                | productId, supplierId, quantity | 201                     |
| Purchase without supplier | 1                | productId, quantity             | 400 - supplier required |
| Sale                      | 2                | productId, quantity             | 201                     |
| Adjustment In             | 3                | productId, quantity             | 201                     |
| Adjustment Out            | 4                | productId, quantity             | 201                     |
| Invalid type              | 99               | -                               | 400 - invalid type      |

**Purchase Request:**

```json
{
  "productId": "653eb0a5-fc22-41a8-9f2a-ee201b8524cf",
  "productBatchId": "075cc84e-9f38-400a-a74b-b79a83ee99c0",
  "supplierId": "cb143108-f662-4c89-b12d-e0b31fa48692",
  "transactionType": 1,
  "quantity": 100,
  "unitPrice": 10000,
  "notes": "Test purchase"
}
```

---

### TC-TXN-002: Filter Transactions by Supplier

**Endpoint:** `GET /api/stocktransactions?supplierId={id}&transactionType=1`

| Step | Action                | Expected                           |
| ---- | --------------------- | ---------------------------------- |
| 1    | Get supplier ID       | Note the ID                        |
| 2    | Filter by supplierId  | Only that supplier's transactions  |
| 3    | Add transactionType=1 | Only Purchase transactions         |
| 4    | Verify results        | All results have matching supplier |

---

## Validation Test Summary

### Required Field Validation

| Entity      | Field     | Rule             | Error Message                      |
| ----------- | --------- | ---------------- | ---------------------------------- |
| Product     | name      | NotEmpty         | "Tên sản phẩm không được để trống" |
| Product     | sku       | NotEmpty, Unique | "SKU không được để trống"          |
| Product     | price     | >= 0             | "Giá phải >= 0"                    |
| Supplier    | name      | NotEmpty         | "Tên NCC không được để trống"      |
| Batch       | quantity  | > 0              | "Số lượng phải > 0"                |
| Transaction | productId | NotEmpty, Exists | "Sản phẩm không tồn tại"           |

### Pagination Validation

| Parameter  | Rule  | Error Message            |
| ---------- | ----- | ------------------------ |
| pageNumber | >= 1  | "Số trang phải >= 1"     |
| pageSize   | 1-100 | "Kích thước trang 1-100" |

---

## Test Checklist Template

```markdown
## Test Run: [Date]

Tester: [Name]
Environment: Development
Version: [Version]

### Categories

- [ ] TC-CAT-001: Get All Categories
- [ ] TC-CAT-002: Create Category
- [ ] TC-CAT-003: Update Category
- [ ] TC-CAT-004: Delete Category

### Products

- [ ] TC-PRD-001: Get Products with Pagination
- [ ] TC-PRD-002: Create Product
- [ ] TC-PRD-003: Get Product by ID
- [ ] TC-PRD-004: Update Product
- [ ] TC-PRD-005: Delete Product

### Batches

- [ ] TC-BTH-001: Add Batch
- [ ] TC-BTH-002: Get Batches
- [ ] TC-BTH-003: Update Batch
- [ ] TC-BTH-004: Delete Batch

### Suppliers

- [ ] TC-SUP-001: CRUD Operations
- [ ] TC-SUP-002: Activate/Deactivate

### Stock Transactions

- [ ] TC-TXN-001: Create Transaction
- [ ] TC-TXN-002: Filter by Supplier
- [ ] TC-TXN-003: Filter by Date Range

### Notes:

[Any issues found]

### Bugs Found:

1. [Bug description]
```

---

## Known Issues

| Issue                                 | Status      | Workaround     |
| ------------------------------------- | ----------- | -------------- |
| Stock transaction filters not working | In Progress | Use searchTerm |
| -                                     | -           | -              |

---

_Document Version: 1.0 | Last Updated: January 2026_
