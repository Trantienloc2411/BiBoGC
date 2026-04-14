// ─── Auth ────────────────────────────────────────────────────────────────────

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpirationAt: string
  role: string
  userName: string
}

// ─── API envelope (Sales / Finance responses) ─────────────────────────────────

export interface ApiResponse<T> {
  success: boolean
  data: T
  message: string | null
  errors?: string[]
}

// ─── Finance / Reports ────────────────────────────────────────────────────────

export interface TopProductDto {
  productName: string
  variantName: string
  quantitySold: number
  revenue: number
}

export interface HourlySalesDto {
  hour: number
  revenue: number
  transactionCount: number
}

export interface DailySalesReportDto {
  date: string
  totalRevenue: number
  transactionCount: number
  previousDayRevenue: number
  revenueChangePercent: number
  topSellingProducts: TopProductDto[]
  salesByHour: HourlySalesDto[]
}

export interface DailyBreakdownDto {
  day: number
  revenue: number
  transactionCount: number
}

export interface MonthlySalesReportDto {
  year: number
  month: number
  totalRevenue: number
  transactionCount: number
  previousMonthRevenue: number
  previousMonthChangePercent: number
  sameMonthLastYearRevenue: number
  yoYChangePercent: number
  dailyBreakdown: DailyBreakdownDto[]
  topSellingProducts: TopProductDto[]
}

export interface MonthlyFinancialReportDto {
  year: number
  month: number
  totalRevenue: number
  totalCogs: number
  totalExpenses: number
  grossProfit: number
  netProfit: number
  profitMarginPercent: number
}

// ─── Annual Report ───────────────────────────────────────────────────────────

export interface MonthlyBreakdownDto {
  month: number
  monthName: string
  revenue: number
  transactionCount: number
  moMGrowthPercent: number | null
}

export interface AnnualSalesReportDto {
  year: number
  totalRevenue: number
  totalTransactions: number
  monthlyBreakdowns: MonthlyBreakdownDto[]
}

// ─── Tax Configuration ──────────────────────────────────────────────────────

export interface TaxConfigDto {
  name: string
  rate: number
  isEnabled: boolean
  effectiveFrom: string
}

export interface TaxConfigsDto {
  vat: TaxConfigDto
  pit: TaxConfigDto
}

export interface UpdateTaxConfigRequest {
  taxType: 'VAT' | 'PIT'
  rate: number
  isEnabled: boolean
}

// ─── Expenses ─────────────────────────────────────────────────────────────────

export interface ExpenseDto {
  id: string
  category: string
  amount: number
  description: string
  expenseDate: string
  receiptNumber?: string
  paymentMethod: string
}

export interface ExpenseCategoryBreakdownDto {
  category: string
  total: number
  count: number
}

export interface DailyExpenseSummaryDto {
  date: string
  totalAmount: number
  breakdownByCategory: ExpenseCategoryBreakdownDto[]
  expenses: ExpenseDto[]
}

export interface CreateExpenseRequest {
  /** Rent=0, Utilities=1, Salary=2, Supplies=3, Marketing=4, Other=5 */
  category: number
  amount: number
  description: string
  expenseDate: string
  /** Cash=0, Card=1, Transfer=2 */
  paymentMethod: number
  receiptNumber?: string
}

// ─── Enums (match backend integer values per Postman) ────────────────────────

export const ExpenseCategory = {
  Rent:       0,
  Utilities:  1,
  Salary:     2,
  Supplies:   3,
  Marketing:  4,
  Other:      5,
} as const

export const ExpenseCategoryLabel: Record<number, string> = {
  0: 'Thuê mặt bằng',
  1: 'Điện / Nước',
  2: 'Lương nhân viên',
  3: 'Vật tư / Dụng cụ',
  4: 'Marketing',
  5: 'Khác',
}

export const ExpensePaymentMethod = {
  Cash:     0,
  Card:     1,
  Transfer: 2,
} as const

export const ExpensePaymentMethodLabel: Record<number, string> = {
  0: 'Tiền mặt',
  1: 'Thẻ',
  2: 'Chuyển khoản',
}

// ─── Sales Orders ─────────────────────────────────────────────────────────────

export interface SalesOrderSummaryDto {
  id: string
  orderNumber: string
  status: string
  totalAmount: number
  paidAmount: number
  createdAt: string
  completedAt: string | null
  hasInvoice: boolean
  invoiceNumber: string | null
}

export interface SalesOrderItemDto {
  productName: string
  variantName: string | null
  quantity: number
  unitPrice: number
  lineTotal: number
}

export interface SalesOrderDetailDto extends SalesOrderSummaryDto {
  subTotal: number
  taxAmount: number
  discountAmount: number
  items: SalesOrderItemDto[]
  invoiceId: string | null
}

// ─── Invoices ─────────────────────────────────────────────────────────────────

export interface InvoiceItemDto {
  id: string
  productName: string
  variantName: string | null
  sku: string
  unit: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export interface InvoiceDto {
  id: string
  invoiceNumber: string
  invoiceDate: string
  salesOrderId: string
  orderNumber: string
  storeName: string
  storeAddress: string
  storePhone: string
  storeTaxCode: string | null
  customerName: string | null
  customerPhone: string | null
  subTotal: number
  discountAmount: number
  taxAmount: number
  grandTotal: number
  amountPaid: number
  changeAmount: number
  paymentMethod: string
  items: InvoiceItemDto[]
  createdAt: string
}

// ─── Notifications ────────────────────────────────────────────────────────────

export interface NotificationDto {
  id: string
  title: string
  message: string
  type: 'info' | 'warning' | 'error'
  isRead: boolean
  createdAt: string
}

export interface NotificationPagedResult {
  items: NotificationDto[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// ─── Audit Logs ───────────────────────────────────────────────────────────────

export interface AuditLogDto {
  id: string
  userId: string | null
  username: string | null
  action: string
  isSuccess: boolean
  description: string | null
  timestamp: string
  ipAddress: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// ─── Inventory — Products ─────────────────────────────────────────────────────

export interface ProductSummaryDto {
  id: string
  name: string
  sku: string
  description: string | null
  baseUnit: string
  salePrice: number
  costPrice: number
  categoryId: string | null
  categoryName: string | null
  supplierId: string | null
  supplierName: string | null
  status: string
  totalStock: number
  availableStock: number
  lowStockThreshold: number
  isLowStock: boolean
}

export interface ProductDetailDto extends ProductSummaryDto {
  batches: ProductBatchDto[]
  variants: ProductVariantDto[]
}

export interface ProductBatchDto {
  id: string
  batchNumber: string
  quantity: number
  manufacturingDate: string | null
  expiryDate: string | null
  costPrice: number
  isExpired: boolean
  isExpiringSoon: boolean
}

export interface ProductVariantDto {
  id: string
  name: string
  sku: string
  barcode: string | null
  unit: string
  salePrice: number
  costPrice: number
  stock: number
}

export interface CreateProductRequest {
  name: string
  sku: string
  description?: string
  baseUnit: string
  salePrice: number
  costPrice: number
  categoryId?: string
  supplierId?: string
  lowStockThreshold: number
}

export interface CreateBatchRequest {
  batchNumber: string
  quantity: number
  manufacturingDate?: string
  expiryDate?: string
  costPrice: number
}

export interface CreateVariantRequest {
  name: string
  sku: string
  barcode?: string
  unit: string
  salePrice: number
  costPrice: number
}

// ─── Inventory — Categories ───────────────────────────────────────────────────

export interface CategoryDto {
  id: string
  name: string
  description: string | null
  parentId: string | null
  parentName: string | null
  children?: CategoryDto[]
}

export interface CreateCategoryRequest {
  name: string
  description?: string
  parentId?: string
}

// ─── Inventory — Suppliers ────────────────────────────────────────────────────

export interface SupplierDto {
  id: string
  name: string
  contactPerson: string | null
  phone: string | null
  email: string | null
  address: string | null
  taxCode: string | null
}

export interface CreateSupplierRequest {
  name: string
  contactPerson?: string
  phone?: string
  email?: string
  address?: string
  taxCode?: string
}

// ─── Inventory — Stock Transactions ───────────────────────────────────────────

export interface StockTransactionDto {
  id: string
  productId: string
  productName: string
  batchNumber: string | null
  type: string
  quantity: number
  unitPrice: number
  totalValue: number
  reason: string | null
  createdAt: string
  createdBy: string | null
}

export interface AdjustStockRequest {
  productId: string
  batchId?: string
  quantity: number
  reason: string
  unitPrice: number
}

// ─── Inventory — Alerts ───────────────────────────────────────────────────────

export interface LowStockProductDto {
  id: string
  name: string
  sku: string
  availableStock: number
  lowStockThreshold: number
  categoryName: string | null
}

export interface ExpiredBatchDto {
  productId: string
  productName: string
  batchNumber: string
  quantity: number
  expiryDate: string
}

export interface ExpiringSoonBatchDto extends ExpiredBatchDto {
  daysUntilExpiry: number
}

// ─── Pagination ───────────────────────────────────────────────────────────────

export interface PaginatedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

// ─── Inventory v2 — Products ──────────────────────────────────────────────────

export type ProductStatus = 'Active' | 'Inactive' | 'Discontinued' | 'OutOfStock'

export type StockTransactionType =
  | 'Purchase' | 'Sale' | 'AdjustmentIn' | 'AdjustmentOut'
  | 'Damage' | 'Expiry' | 'Return' | 'SupplierReturn'

/**
 * ProductDto — matches Postman response:
 * id, name, sku, price, description, status, requiresBatchTracking,
 * totalStock, availableStock, expiredStock, expiringSoonStock,
 * categoryId, categoryName, createdAt, updatedAt, batches[]
 */
export interface ProductDto {
  id: string
  name: string
  description: string | null
  sku: string
  /** String enum: Active | Inactive | Discontinued | OutOfStock */
  status: ProductStatus
  requiresBatchTracking: boolean
  /** Single price field per Postman */
  price: number
  categoryId: string | null
  categoryName: string | null
  totalStock: number
  availableStock: number
  expiredStock: number
  expiringSoonStock: number
  /** isLowStock derived from totalStock vs threshold (not in Postman response directly) */
  isLowStock?: boolean
  lowStockThreshold: number | null
  supplierId?: string | null
  supplierName?: string | null
  recentBatches: ProductBatchDtoV2[]
  createdAt: string
  updatedAt: string | null
}

/**
 * CreateProductRequest — matches Postman POST /api/products body:
 * { name, sku, price, description, requiresBatchTracking }
 */
export interface CreateProductRequestV2 {
  name: string
  sku: string
  /** Single price field per Postman */
  price: number
  description?: string
  requiresBatchTracking: boolean
  categoryId?: string
  baseUnits: number
}

/**
 * UpdateProductRequest — matches Postman PUT /api/products/:id body:
 * { price } — only price shown in Postman
 */
export interface UpdateProductRequest {
  price?: number
  name?: string
  description?: string
  status?: number
}

/**
 * ProductBatchDtoV2 — matches Postman batch response:
 * id, productId, batchNumber, quantity, manufacturingDate, expirationDate,
 * costPrice, isExpired, daysUntilExpiration, isExpiringSoon
 */
export interface ProductBatchDtoV2 {
  id: string
  productId: string
  productName?: string | null
  batchNumber: string
  quantity: number
  /** ISO datetime string */
  manufacturingDate: string | null
  /** ISO datetime string — backend field name */
  expirationDate: string | null
  costPrice: number
  isExpired: boolean
  /** Days until expiration (backend field name) */
  daysUntilExpiration: number | null
  isExpiringSoon: boolean
}

/**
 * AddBatchRequest — matches Postman POST /api/products/:productId/batches body:
 * { batchNumber, quantity, manufacturingDate, expirationDate, costPrice }
 */
export interface AddBatchRequest {
  batchNumber: string
  quantity: number
  manufacturingDate?: string
  /** backend field name */
  expirationDate?: string
  costPrice?: number
}

/**
 * UpdateBatchRequest — matches Postman PUT /api/products/:productId/batches/:batchId body:
 * { quantity, manufacturingDate, expirationDate }
 */
export interface UpdateBatchRequest {
  quantity: number
  manufacturingDate?: string
  /** backend field name */
  expirationDate?: string
}

/**
 * ProductVariantDtoV2 — matches Postman variant create response:
 * id, productId, productName, variantName, unit (int), unitName, quantityBaseUnit,
 * salePrice, costPrice, displayOrder, barcode, createdAt, updatedAt
 */
export interface ProductVariantDtoV2 {
  id: string
  productId: string
  productName?: string | null
  variantName: string
  /** SKU unique string */
  sku?: string
  /** Integer unit code */
  unit: number
  unitName?: string | null
  quantityBaseUnit: number
  salePrice: number
  costPrice: number | null
  displayOrder?: number
  barcode: string | null
  createdAt: string
  updatedAt: string | null
}

/**
 * CreateVariantRequestV2 — matches Postman POST /api/products/:productId/variants body:
 * { productId, variantName, barcode, displayOrder, unit (int), salePrice, costPrice, quantityBaseUnit }
 */
export interface CreateVariantRequestV2 {
  productId: string
  variantName: string
  barcode?: string
  displayOrder?: number
  /** Integer unit code per Postman enum */
  unit: number
  salePrice: number
  costPrice: number
  quantityBaseUnit: number
}

/**
 * UpdateVariantRequest — matches Postman PUT /api/products/:productId/variants/:variantId body:
 * { productVariantId, productId, variantName, displayOrder, quantityBaseUnit,
 *   costPrice, salePrice, barcode, unit (int) }
 */
export interface UpdateVariantRequest {
  productVariantId: string
  productId: string
  variantName: string
  displayOrder?: number
  quantityBaseUnit: number
  costPrice: number
  salePrice: number
  barcode?: string
  /** Integer unit code per Postman enum */
  unit: number
}

/** Integer unit codes per Postman documentation */
export const VariantUnitCode = {
  Pcs:   1,
  Hop:   2,
  Chai:  3,
  Lon:   4,
  Goi:   5,
  Bich:  6,
  Loc:   7,
  Thung: 8,
  Cuon:  9,
  Vi:    10,
  Cay:   11,
  Thanh: 12,
  Tui:   13,
  Bo:    14,
  Doi:   15,
  Can:   16,
  Kg:    21,
  Lang:  22,
  Lit:   31,
  Qua:   40,
  Trai:  41,
} as const

export const VariantUnitLabel: Record<number, string> = {
  1:  'Cái',
  2:  'Hộp',
  3:  'Chai',
  4:  'Lon',
  5:  'Gói',
  6:  'Bịch',
  7:  'Lốc',
  8:  'Thùng',
  9:  'Cuộn',
  10: 'Vỉ',
  11: 'Cây',
  12: 'Thanh',
  13: 'Túi',
  14: 'Bộ',
  15: 'Đôi',
  16: 'Cân',
  21: 'Kg',
  22: 'Lạng',
  31: 'Lít',
  40: 'Quả',
  41: 'Trái',
}

/**
 * CategoryTreeNode — response from GET /api/categories/tree and GET /api/categories/{id}/tree
 * Optimised tree endpoint: no N+1, includes productCount per node.
 */
export interface CategoryTreeNode {
  id: string
  name: string
  productCount: number
  subCategories: CategoryTreeNode[]
}

/**
 * CategoryDtoV2 — matches Postman GET /api/categories response:
 * id, name, description, parentCategoryId, isActive, displayOrder, subCategories[]
 */
export interface CategoryDtoV2 {
  id: string
  name: string
  description: string | null
  parentCategoryId: string | null
  isActive: boolean
  displayOrder: number
  subCategories: CategoryDtoV2[]
}

/**
 * CreateCategoryRequestV2 — matches Postman POST /api/categories body:
 * { name, description, parentCategoryId }
 */
export interface CreateCategoryRequestV2 {
  name: string
  description?: string
  parentCategoryId?: string | null
}

/**
 * UpdateCategoryRequest — matches Postman PUT /api/categories/:id body:
 * { name, description, isActive, displayOrder }
 */
export interface UpdateCategoryRequest {
  name: string
  description?: string
  isActive?: boolean
  displayOrder?: number
}

/**
 * SupplierDtoV2 — matches Postman GET /api/suppliers response:
 * id, name, contactName, contactPhone, address, isActive, createAt, transactions[]
 * List response also includes: id, name, contactName, contactPhone, address, isActive
 */
export interface SupplierDtoV2 {
  id: string
  name: string
  contactName: string | null
  contactPhone: string | null
  address: string | null
  isActive: boolean
  createAt?: string
  notes?: string | null
}

/**
 * CreateSupplierRequestV2 — matches Postman POST /api/suppliers body:
 * { name, contactPerson, phoneNumber, address }
 */
export interface CreateSupplierRequestV2 {
  name: string
  contactPerson?: string
  phoneNumber?: string
  address?: string
}

/**
 * UpdateSupplierRequest — matches Postman PUT /api/suppliers/:id body:
 * { name, contactName, contactPhone, address, isActive }
 */
export interface UpdateSupplierRequest {
  name: string
  contactName?: string
  contactPhone?: string
  address?: string
  isActive?: boolean
}

/**
 * StockTransactionDtoV2 — matches Postman response:
 * id, productId, productName, productBatchId, batchNumber, supplierId, supplierName,
 * sku, transactionType, quantity, unitPrice, totalAmount, transactionDate, notes, referenceNumber
 */
export interface StockTransactionDtoV2 {
  id: string
  productId: string
  productName: string
  productBatchId: string | null
  batchNumber: string | null
  supplierId: string | null
  supplierName: string | null
  sku: string
  transactionType: string
  quantity: number
  unitPrice: number
  /** Backend field name is totalAmount */
  totalAmount: number
  transactionDate: string
  notes: string | null
  referenceNumber: string | null
}

/**
 * AdjustStockRequestV2 — matches Postman POST /api/stocktransactions/adjustment body:
 * { productId, productBatchId, isIncrease, quantity, unitPrice, notes }
 */
export interface AdjustStockRequestV2 {
  productId: string
  productBatchId?: string
  /** true = increase stock, false = decrease */
  isIncrease: boolean
  quantity: number
  unitPrice: number
  notes?: string
}
