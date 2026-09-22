using System.Security.Claims;

namespace ExpenseTracker.Api.Services;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null || !int.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user identity.");
        }

        return userId;
    }
}
