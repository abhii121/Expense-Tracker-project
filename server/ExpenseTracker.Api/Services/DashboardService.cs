using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class DashboardService(AppDbContext db)
{
    public async Task<DashboardSummary> GetSummaryAsync(int userId, int year, int month)
    {
        var monthTransactions = await db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date.Year == year && t.Date.Month == month)
            .ToListAsync();

        var totalIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var expenseByCategory = monthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new CategoryBreakdown(g.Key!.Id, g.Key.Name, g.Key.Color, g.Sum(t => t.Amount)))
            .OrderByDescending(c => c.Total)
            .ToList();

        var sixMonthsAgo = new DateOnly(year, month, 1).AddMonths(-5);
        var trendRaw = await db.Transactions
            .Where(t => t.UserId == userId && t.Date >= sixMonthsAgo)
            .ToListAsync();

        var monthlyTrend = Enumerable.Range(0, 6)
            .Select(offset => sixMonthsAgo.AddMonths(offset))
            .Select(monthStart =>
            {
                var monthItems = trendRaw.Where(t => t.Date.Year == monthStart.Year && t.Date.Month == monthStart.Month);
                return new MonthlyTrendPoint(
                    monthStart.Year,
                    monthStart.Month,
                    monthItems.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    monthItems.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                );
            })
            .ToList();

        var recent = await db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(8)
            .Select(t => new TransactionDto(
                t.Id, t.Amount, t.Type, t.Note, t.Date,
                t.CategoryId, t.Category!.Name, t.Category.Color, t.Category.Icon))
            .ToListAsync();

        return new DashboardSummary(
            totalIncome, totalExpense, totalIncome - totalExpense,
            expenseByCategory, monthlyTrend, recent
        );
    }
}
