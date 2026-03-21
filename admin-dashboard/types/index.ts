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
