using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class TransactionService(AppDbContext db)
{
    public async Task<PagedResult<TransactionDto>> GetAllAsync(int userId, TransactionQuery query)
    {
        var q = db.Transactions.Where(t => t.UserId == userId);

        if (query.From is not null) q = q.Where(t => t.Date >= query.From);
        if (query.To is not null) q = q.Where(t => t.Date <= query.To);
        if (query.CategoryId is not null) q = q.Where(t => t.CategoryId == query.CategoryId);
        if (query.Type is not null) q = q.Where(t => t.Type == query.Type);

        var totalCount = await q.CountAsync();

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await q
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToDto(t))
            .ToListAsync();

        return new PagedResult<TransactionDto>(items, totalCount, page, pageSize);
    }

    public async Task<TransactionDto> CreateAsync(int userId, TransactionRequest request)
    {
        await EnsureCategoryOwned(userId, request.CategoryId);

        var transaction = new Transaction
        {
            Amount = request.Amount,
            Type = request.Type,
            Note = request.Note?.Trim(),
            Date = request.Date,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        await db.Entry(transaction).Reference(t => t.Category).LoadAsync();
        return ToDto(transaction);
    }

    public async Task<TransactionDto?> UpdateAsync(int userId, int transactionId, TransactionRequest request)
    {
        var transaction = await db.Transactions
            .Include(t => t.Category)
            .SingleOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

        if (transaction is null) return null;

        await EnsureCategoryOwned(userId, request.CategoryId);

        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Note = request.Note?.Trim();
        transaction.Date = request.Date;
        transaction.CategoryId = request.CategoryId;

        await db.SaveChangesAsync();
        await db.Entry(transaction).Reference(t => t.Category).LoadAsync();
        return ToDto(transaction);
    }

    public async Task<bool> DeleteAsync(int userId, int transactionId)
    {
        var transaction = await db.Transactions.SingleOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);
        if (transaction is null) return false;

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task EnsureCategoryOwned(int userId, int categoryId)
    {
        var owned = await db.Categories.AnyAsync(c => c.Id == categoryId && c.UserId == userId);
        if (!owned)
        {
            throw new InvalidOperationException("Category not found.");
        }
    }

    private static TransactionDto ToDto(Transaction t) => new(
        t.Id, t.Amount, t.Type, t.Note, t.Date,
        t.CategoryId, t.Category?.Name ?? "", t.Category?.Color ?? "#898781", t.Category?.Icon ?? "tag"
    );
}
