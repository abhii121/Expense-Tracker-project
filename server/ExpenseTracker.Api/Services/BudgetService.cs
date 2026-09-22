using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class BudgetService(AppDbContext db)
{
    public async Task<List<BudgetDto>> GetForMonthAsync(int userId, int year, int month)
    {
        var budgets = await db.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .ToListAsync();

        var spentByCategory = await db.Transactions
            .Where(t => t.UserId == userId
                        && t.Date.Year == year
                        && t.Date.Month == month
                        && t.Type == Models.TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Total);

        return budgets.Select(b => new BudgetDto(
            b.Id, b.MonthlyLimit, b.Year, b.Month, b.CategoryId,
            b.Category?.Name ?? "", b.Category?.Color ?? "#898781",
            spentByCategory.GetValueOrDefault(b.CategoryId, 0m)
        )).ToList();
    }

    public async Task<BudgetDto> UpsertAsync(int userId, BudgetRequest request)
    {
        var categoryOwned = await db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
        if (!categoryOwned)
        {
            throw new InvalidOperationException("Category not found.");
        }

        var budget = await db.Budgets.SingleOrDefaultAsync(b =>
            b.UserId == userId && b.CategoryId == request.CategoryId && b.Year == request.Year && b.Month == request.Month);

        if (budget is null)
        {
            budget = new Models.Budget
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                Year = request.Year,
                Month = request.Month
            };
            db.Budgets.Add(budget);
        }

        budget.MonthlyLimit = request.MonthlyLimit;
        await db.SaveChangesAsync();

        var category = await db.Categories.FindAsync(request.CategoryId);
        var spent = await db.Transactions
            .Where(t => t.UserId == userId && t.CategoryId == request.CategoryId
                        && t.Date.Year == request.Year && t.Date.Month == request.Month
                        && t.Type == Models.TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        return new BudgetDto(budget.Id, budget.MonthlyLimit, budget.Year, budget.Month,
            budget.CategoryId, category?.Name ?? "", category?.Color ?? "#898781", spent);
    }

    public async Task<bool> DeleteAsync(int userId, int budgetId)
    {
        var budget = await db.Budgets.SingleOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);
        if (budget is null) return false;

        db.Budgets.Remove(budget);
        await db.SaveChangesAsync();
        return true;
    }
}
