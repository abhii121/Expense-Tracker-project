using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.DTOs;

public record BudgetDto(
    int Id,
    decimal MonthlyLimit,
    int Year,
    int Month,
    int CategoryId,
    string CategoryName,
    string CategoryColor,
    decimal Spent
);

public record BudgetRequest(
    [Required] decimal MonthlyLimit,
    [Required, Range(1, 9999)] int Year,
    [Required, Range(1, 12)] int Month,
    [Required] int CategoryId
);
