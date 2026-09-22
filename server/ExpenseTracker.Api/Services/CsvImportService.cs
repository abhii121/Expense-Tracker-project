using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class CsvImportService(AppDbContext db)
{
    private sealed class CsvRow
    {
        public string? Date { get; set; }
        public string? Amount { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Note { get; set; }
    }

    public async Task<CsvImportResult> ImportAsync(int userId, Stream csvStream)
    {
        var categories = await db.Categories
            .Where(c => c.UserId == userId)
            .ToDictionaryAsync(c => c.Name.Trim().ToLowerInvariant(), c => c);

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        List<CsvRow> rows;
        try
        {
            rows = csv.GetRecords<CsvRow>().ToList();
        }
        catch (Exception ex)
        {
            return new CsvImportResult(0, 0, [$"Could not parse CSV: {ex.Message}"]);
        }

        var errors = new List<string>();
        var toInsert = new List<Transaction>();
        var rowNumber = 1;

        foreach (var row in rows)
        {
            rowNumber++;

            if (!DateOnly.TryParse(row.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                errors.Add($"Row {rowNumber}: invalid or missing date '{row.Date}'.");
                continue;
            }

            if (!decimal.TryParse(row.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
            {
                errors.Add($"Row {rowNumber}: invalid amount '{row.Amount}'.");
                continue;
            }

            if (!Enum.TryParse<TransactionType>(row.Type, true, out var type))
            {
                errors.Add($"Row {rowNumber}: type must be 'Income' or 'Expense', got '{row.Type}'.");
                continue;
            }

            var categoryKey = (row.Category ?? "Other").Trim().ToLowerInvariant();
            if (!categories.TryGetValue(categoryKey, out var category))
            {
                category = new Category
                {
                    Name = string.IsNullOrWhiteSpace(row.Category) ? "Other" : row.Category!.Trim(),
                    Icon = "tag",
                    Color = "#898781",
                    Type = type,
                    UserId = userId
                };
                db.Categories.Add(category);
                categories[categoryKey] = category;
            }

            toInsert.Add(new Transaction
            {
                Date = date,
                Amount = amount,
                Type = type,
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note!.Trim(),
                UserId = userId,
                Category = category
            });
        }

        if (toInsert.Count > 0)
        {
            db.Transactions.AddRange(toInsert);
            await db.SaveChangesAsync();
        }

        return new CsvImportResult(toInsert.Count, errors.Count, errors);
    }
}
