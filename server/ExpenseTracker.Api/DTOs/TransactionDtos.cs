using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.DTOs;

public record TransactionDto(
    int Id,
    decimal Amount,
    TransactionType Type,
    string? Note,
    DateOnly Date,
    int CategoryId,
    string CategoryName,
    string CategoryColor,
    string CategoryIcon
);

public record TransactionRequest(
    [Required] decimal Amount,
    [Required] TransactionType Type,
    string? Note,
    [Required] DateOnly Date,
    [Required] int CategoryId
);

public record TransactionQuery(
    DateOnly? From,
    DateOnly? To,
    int? CategoryId,
    TransactionType? Type,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public record CsvImportResult(int Imported, int Skipped, IReadOnlyList<string> Errors);
