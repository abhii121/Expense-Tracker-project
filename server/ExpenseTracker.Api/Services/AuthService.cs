using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class AuthService(AppDbContext db, JwtTokenService tokenService)
{
    // Colors follow the validated categorical palette (fixed hue order, CVD-safe adjacent pairs).
    private static readonly (string Name, string Icon, string Color, TransactionType Type)[] DefaultCategories =
    [
        ("Salary", "briefcase", "#2a78d6", TransactionType.Income),
        ("Freelance", "laptop", "#eb6834", TransactionType.Income),
        ("Food & Dining", "utensils", "#1baf7a", TransactionType.Expense),
        ("Transport", "car", "#eda100", TransactionType.Expense),
        ("Bills & Utilities", "receipt", "#e87ba4", TransactionType.Expense),
        ("Entertainment", "film", "#008300", TransactionType.Expense),
        ("Shopping", "shopping-bag", "#4a3aa7", TransactionType.Expense),
        ("Health", "heart-pulse", "#e34948", TransactionType.Expense),
        ("Other", "tag", "#898781", TransactionType.Expense)
    ];

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        foreach (var (name, icon, color, type) in DefaultCategories)
        {
            db.Categories.Add(new Category { Name = name, Icon = icon, Color = color, Type = type, UserId = user.Id });
        }
        await db.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expiresAt) = tokenService.GenerateToken(user);
        return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.Name, user.Email));
    }
}
