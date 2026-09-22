using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.DTOs;

public record CategoryDto(int Id, string Name, string Icon, string Color, TransactionType Type);

public record CategoryRequest(
    [Required, StringLength(50, MinimumLength = 1)] string Name,
    string Icon,
    string Color,
    TransactionType Type
);
