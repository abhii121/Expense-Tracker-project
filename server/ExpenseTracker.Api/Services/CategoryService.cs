using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class CategoryService(AppDbContext db)
{
    public async Task<List<CategoryDto>> GetAllAsync(int userId)
    {
        return await db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Icon, c.Color, c.Type))
            .ToListAsync();
    }

    public async Task<CategoryDto> CreateAsync(int userId, CategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name.Trim(),
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "tag" : request.Icon,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#2a78d6" : request.Color,
            Type = request.Type,
            UserId = userId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Icon, category.Color, category.Type);
    }

    public async Task<CategoryDto?> UpdateAsync(int userId, int categoryId, CategoryRequest request)
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
        if (category is null) return null;

        category.Name = request.Name.Trim();
        category.Icon = string.IsNullOrWhiteSpace(request.Icon) ? category.Icon : request.Icon;
        category.Color = string.IsNullOrWhiteSpace(request.Color) ? category.Color : request.Color;
        category.Type = request.Type;

        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name, category.Icon, category.Color, category.Type);
    }

    public async Task<bool> DeleteAsync(int userId, int categoryId)
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
        if (category is null) return false;

        var hasTransactions = await db.Transactions.AnyAsync(t => t.CategoryId == categoryId);
        if (hasTransactions)
        {
            throw new InvalidOperationException("Cannot delete a category that has transactions. Reassign or delete those first.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }
}
