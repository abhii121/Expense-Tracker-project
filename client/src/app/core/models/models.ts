export type TransactionType = 'Income' | 'Expense';

export interface User {
  id: number;
  name: string;
  email: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface Category {
  id: number;
  name: string;
  icon: string;
  color: string;
  type: TransactionType;
}

export interface CategoryRequest {
  name: string;
  icon: string;
  color: string;
  type: TransactionType;
}

export interface Transaction {
  id: number;
  amount: number;
  type: TransactionType;
  note: string | null;
  date: string;
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  categoryIcon: string;
}

export interface TransactionRequest {
  amount: number;
  type: TransactionType;
  note: string | null;
  date: string;
  categoryId: number;
}

export interface TransactionQuery {
  from?: string;
  to?: string;
  categoryId?: number;
  type?: TransactionType;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CsvImportResult {
  imported: number;
  skipped: number;
  errors: string[];
}

export interface Budget {
  id: number;
  monthlyLimit: number;
  year: number;
  month: number;
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  spent: number;
}

export interface BudgetRequest {
  monthlyLimit: number;
  year: number;
  month: number;
  categoryId: number;
}

export interface CategoryBreakdown {
  categoryId: number;
  categoryName: string;
  color: string;
  total: number;
}

export interface MonthlyTrendPoint {
  year: number;
  month: number;
  income: number;
  expense: number;
}

export interface DashboardSummary {
  totalIncome: number;
  totalExpense: number;
  balance: number;
  expenseByCategory: CategoryBreakdown[];
  monthlyTrend: MonthlyTrendPoint[];
  recentTransactions: Transaction[];
}
