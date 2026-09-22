namespace ExpenseTracker.Api.DTOs;

public record DashboardSummary(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    IReadOnlyList<CategoryBreakdown> ExpenseByCategory,
    IReadOnlyList<MonthlyTrendPoint> MonthlyTrend,
    IReadOnlyList<TransactionDto> RecentTransactions
);

public record CategoryBreakdown(int CategoryId, string CategoryName, string Color, decimal Total);

public record MonthlyTrendPoint(int Year, int Month, decimal Income, decimal Expense);
