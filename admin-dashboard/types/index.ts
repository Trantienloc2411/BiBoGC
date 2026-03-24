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
  category: number
  amount: number
  description: string
  expenseDate: string
  paymentMethod: number
  receiptNumber?: string
}

// ─── Enums (match backend integer values) ────────────────────────────────────

export const ExpenseCategory = {
  Rent:       1,
  Utilities:  2,
  Supplies:   3,
  Salary:     4,
  Marketing:  5,
  Others:     99,
} as const

export const ExpenseCategoryLabel: Record<number, string> = {
  1:  'Thuê mặt bằng',
  2:  'Điện / Nước',
  3:  'Vật tư / Dụng cụ',
  4:  'Lương nhân viên',
  5:  'Marketing',
  99: 'Khác',
}

export const ExpensePaymentMethod = {
  Cash:         1,
  BankTransfer: 2,
  QRPayment:    3,
} as const

export const ExpensePaymentMethodLabel: Record<number, string> = {
  1: 'Tiền mặt',
  2: 'Chuyển khoản',
  3: 'QR Code',
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

export interface ProductDto {
  id: string
  name: string
  description: string | null
  sku: string
  status: ProductStatus
  hasVariants: boolean
  baseUnits: string
  salePrice: number
  costPrice: number | null
  categoryId: string | null
  categoryName: string | null
  supplierId: string | null
  supplierName: string | null
  totalStock: number
  lowStockThreshold: number
  isLowStock: boolean
  availableStock: number
  expiredBatchesCount: number
  expiringSoonBatchesCount: number
  recentBatches: ProductBatchDtoV2[]
  variants: ProductVariantDtoV2[]
  createdAt: string
  updatedAt: string | null
}

export interface CreateProductRequestV2 {
  name: string
  description?: string
  sku: string
  hasVariants: boolean
  baseUnits: string
  salePrice: number
  costPrice?: number
  categoryId?: string
  supplierId?: string
  lowStockThreshold: number
  initialStock?: number
}

export interface UpdateProductRequest {
  name: string
  description?: string
  salePrice: number
  costPrice?: number
  categoryId?: string
  supplierId?: string
  lowStockThreshold: number
  status: number
}

export interface ProductBatchDtoV2 {
  id: string
  productId: string
  productName: string | null
  batchNumber: string
  quantity: number
  manufactureDate: string | null
  expiryDate: string | null
  isExpired: boolean
  isExpiringSoon: boolean
  daysUntilExpiry: number | null
  createdAt: string
  updatedAt: string | null
}

export interface AddBatchRequest {
  batchNumber: string
  quantity: number
  manufactureDate?: string
  expiryDate?: string
}

export interface UpdateBatchRequest {
  quantity: number
  manufactureDate?: string
  expiryDate?: string
}

export interface ProductVariantDtoV2 {
  id: string
  productId: string
  variantName: string
  sku: string
  unit: string
  quantityBaseUnit: number
  salePrice: number
  costPrice: number | null
  barcode: string | null
  createdAt: string
  updatedAt: string | null
}

export interface CreateVariantRequestV2 {
  variantName: string
  unit: string
  quantityBaseUnit: number
  salePrice: number
  costPrice?: number
  barcode?: string
}

export interface UpdateVariantRequest {
  variantName: string
  salePrice: number
  costPrice?: number
  barcode?: string
  quantityBaseUnit: number
}

export interface CategoryDtoV2 {
  id: string
  name: string
  description: string | null
  parentCategoryId: string | null
  parentCategoryName: string | null
  subCategories: CategoryDtoV2[]
  productCount: number
}

export interface CreateCategoryRequestV2 {
  name: string
  description?: string
  parentCategoryId?: string
}

export interface UpdateCategoryRequest {
  name: string
  description?: string
  parentCategoryId?: string | null
}

export interface SupplierDtoV2 {
  id: string
  name: string
  contactPerson: string | null
  phone: string | null
  email: string | null
  address: string | null
  taxCode: string | null
  notes: string | null
  createdAt: string
  updatedAt: string | null
}

export interface CreateSupplierRequestV2 {
  name: string
  contactPerson?: string
  phone?: string
  email?: string
  address?: string
  taxCode?: string
  notes?: string
}

export interface StockTransactionDtoV2 {
  id: string
  productId: string
  productName: string
  productSku: string
  productBatchId: string | null
  batchNumber: string | null
  supplierId: string | null
  supplierName: string | null
  transactionType: string
  quantity: number
  unitPrice: number
  totalValue: number
  transactionDate: string
  notes: string | null
  createdAt: string
}

export interface AdjustStockRequestV2 {
  productId: string
  batchId?: string
  quantity: number
  reason: string
  unitPrice: number
}
