# API Reference

## Base URL

- **Development**: `http://localhost:5020`
- **OpenAPI Spec**: `openapi-bibogc.json` (Postman importable)

---

## Authentication

> **Phase 1**: No authentication required  
> **Future**: JWT Bearer token

---

## Response Format

### Success Response

```json
{
  "id": "guid",
  "name": "Product Name",
  ...
}
```

### Paginated Response

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true
}
```

### Error Response

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Lỗi validation",
  "status": 400,
  "detail": "Tên sản phẩm không được để trống",
  "errors": {
    "Name": ["Tên sản phẩm không được để trống"]
  }
}
```

---

## API Endpoints

### Categories

| Method | Endpoint               | Description                |
| ------ | ---------------------- | -------------------------- |
| GET    | `/api/categories`      | Lấy danh sách danh mục     |
| GET    | `/api/categories/{id}` | Lấy chi tiết danh mục      |
| POST   | `/api/categories`      | Tạo danh mục mới           |
| PUT    | `/api/categories/{id}` | Cập nhật danh mục          |
| DELETE | `/api/categories/{id}` | Xóa danh mục (soft delete) |

#### GET /api/categories

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| includeInactive | boolean | false | Bao gồm danh mục không hoạt động |
| parentCategoryId | uuid | null | Lọc theo danh mục cha |

#### POST /api/categories

```json
{
  "name": "Đồ uống",
  "description": "Các loại nước uống",
  "parentCategoryId": null
}
```

---

### Products

| Method | Endpoint             | Description                         |
| ------ | -------------------- | ----------------------------------- |
| GET    | `/api/products`      | Lấy danh sách sản phẩm (phân trang) |
| GET    | `/api/products/{id}` | Lấy chi tiết sản phẩm               |
| POST   | `/api/products`      | Tạo sản phẩm mới                    |
| PUT    | `/api/products/{id}` | Cập nhật sản phẩm                   |
| DELETE | `/api/products/{id}` | Xóa sản phẩm (soft delete)          |

#### GET /api/products

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Số trang (1-based) |
| pageSize | int | 10 | Số lượng mỗi trang (1-100) |
| searchTerm | string | null | Tìm theo tên, SKU, mô tả |

#### POST /api/products

```json
{
  "name": "Coca Cola lon 330ml",
  "sku": "COCA-330",
  "price": 12000,
  "description": "Nước ngọt có gas",
  "requiresBatchTracking": true
}
```

**Validation Rules:**

- `name`: Bắt buộc, max 200 ký tự
- `sku`: Bắt buộc, max 20 ký tự, unique, chỉ chữ/số/-/\_
- `price`: Bắt buộc, >= 0

---

### Product Batches

| Method | Endpoint                                      | Description           |
| ------ | --------------------------------------------- | --------------------- |
| GET    | `/api/products/{productId}/batches`           | Lấy danh sách lô hàng |
| GET    | `/api/products/{productId}/batches/{batchId}` | Lấy chi tiết lô hàng  |
| POST   | `/api/products/{productId}/batches`           | Thêm lô hàng mới      |
| PUT    | `/api/products/{productId}/batches/{batchId}` | Cập nhật lô hàng      |
| DELETE | `/api/products/{productId}/batches/{batchId}` | Xóa lô hàng           |

#### GET /api/products/{productId}/batches

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Số trang |
| pageSize | int | 10 | Số lượng mỗi trang |
| includeExpired | boolean | false | Bao gồm lô đã hết hạn |
| sortBy | string | ExpirationDate | Field sắp xếp |
| sortDescending | boolean | false | Sắp xếp giảm dần |

**Sort Options:** ExpirationDate, ManufacturingDate, Quantity, BatchNumber, CreatedAt

#### POST /api/products/{productId}/batches

```json
{
  "batchNumber": "BATCH-2026-001",
  "quantity": 100,
  "manufacturingDate": "2026-01-01",
  "expirationDate": "2027-01-01",
  "costPrice": 10000
}
```

**Validation Rules:**

- `batchNumber`: Bắt buộc, max 50 ký tự, unique trong product
- `quantity`: Bắt buộc, > 0
- `manufacturingDate`: Bắt buộc, không trong tương lai
- `expirationDate`: Bắt buộc, phải sau manufacturingDate

---

### Suppliers

| Method | Endpoint                         | Description                    |
| ------ | -------------------------------- | ------------------------------ |
| GET    | `/api/suppliers`                 | Lấy danh sách NCC (phân trang) |
| GET    | `/api/suppliers/{id}`            | Lấy chi tiết NCC               |
| POST   | `/api/suppliers`                 | Tạo NCC mới                    |
| PUT    | `/api/suppliers/{id}`            | Cập nhật NCC                   |
| DELETE | `/api/suppliers/{id}`            | Xóa NCC (soft delete)          |
| POST   | `/api/suppliers/{id}/activate`   | Kích hoạt NCC                  |
| POST   | `/api/suppliers/{id}/deactivate` | Vô hiệu hóa NCC                |

#### GET /api/suppliers

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Số trang |
| pageSize | int | 10 | Số lượng mỗi trang |
| searchTerm | string | null | Tìm theo tên, SĐT, địa chỉ |
| isActive | boolean | true | Lọc theo trạng thái |
| sortBy | string | Name | Field sắp xếp (Name, CreatedAt) |
| sortDescending | boolean | false | Sắp xếp giảm dần |

#### POST /api/suppliers

```json
{
  "name": "Công ty TNHH Coca-Cola Việt Nam",
  "contactPerson": "Nguyễn Văn An",
  "phoneNumber": "0283456789",
  "address": "KCN Biên Hòa, Đồng Nai"
}
```

---

### Stock Transactions

| Method | Endpoint                            | Description             |
| ------ | ----------------------------------- | ----------------------- |
| GET    | `/api/stocktransactions`            | Lấy danh sách giao dịch |
| GET    | `/api/stocktransactions/{id}`       | Lấy chi tiết giao dịch  |
| POST   | `/api/stocktransactions`            | Tạo giao dịch mới       |
| POST   | `/api/stocktransactions/purchase`   | Shortcut: Nhập hàng     |
| POST   | `/api/stocktransactions/sale`       | Shortcut: Bán hàng      |
| POST   | `/api/stocktransactions/adjustment` | Shortcut: Điều chỉnh    |

#### GET /api/stocktransactions

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Số trang |
| pageSize | int | 10 | Số lượng mỗi trang |
| searchTerm | string | null | Tìm kiếm |
| productId | uuid | null | Lọc theo sản phẩm |
| supplierId | uuid | null | Lọc theo NCC |
| transactionType | int | null | Lọc theo loại (1-8) |
| fromDate | datetime | null | Từ ngày |
| toDate | datetime | null | Đến ngày |
| sortBy | string | TransactionDate | Field sắp xếp |
| sortDescending | boolean | true | Mới nhất trước |

#### Transaction Types

| Value | Name           | Description      |
| ----- | -------------- | ---------------- |
| 1     | Purchase       | Nhập hàng từ NCC |
| 2     | Sale           | Bán hàng         |
| 3     | AdjustmentIn   | Điều chỉnh tăng  |
| 4     | AdjustmentOut  | Điều chỉnh giảm  |
| 5     | Damage         | Hàng hư hỏng     |
| 6     | Expiry         | Hàng hết hạn     |
| 7     | Return         | Khách trả hàng   |
| 8     | SupplierReturn | Trả hàng NCC     |

#### POST /api/stocktransactions

```json
{
  "productId": "653eb0a5-fc22-41a8-9f2a-ee201b8524cf",
  "productBatchId": "075cc84e-9f38-400a-a74b-b79a83ee99c0",
  "supplierId": "cb143108-f662-4c89-b12d-e0b31fa48692",
  "transactionType": 1,
  "quantity": 100,
  "unitPrice": 10000,
  "notes": "Nhập hàng đợt 1"
}
```

---

## HTTP Status Codes

| Code | Description                    |
| ---- | ------------------------------ |
| 200  | Success                        |
| 201  | Created                        |
| 204  | No Content (Delete success)    |
| 400  | Bad Request (Validation error) |
| 404  | Not Found                      |
| 500  | Internal Server Error          |

---

## Examples

### Lấy đơn nhập hàng từ NCC cụ thể

```http
GET /api/stocktransactions?supplierId=cb143108-f662-4c89-b12d-e0b31fa48692&transactionType=1
```

### Lấy sản phẩm sắp hết hạn

```http
GET /api/products/{productId}/batches?includeExpired=false&sortBy=ExpirationDate
```

### Tìm kiếm sản phẩm

```http
GET /api/products?searchTerm=coca&pageSize=20
```

---

_Document Version: 1.0 | Last Updated: January 2026_
