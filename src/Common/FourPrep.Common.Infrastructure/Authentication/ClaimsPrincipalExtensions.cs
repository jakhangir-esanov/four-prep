namespace FourPrep.Common.Infrastructure.Authentication;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId =
            principal?.FindFirst(CustomClaims.Sub)?.Value ??
            principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User identifier is unavailable");
    }

    public static Guid? GetUserIdOrNull(this ClaimsPrincipal? principal)
    {
        string? userId =
            principal?.FindFirst(CustomClaims.Sub)?.Value ??
            principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userId, out Guid parsedUserId) ? parsedUserId : null;
    }

    public static string GetUserName(this ClaimsPrincipal? principal) =>
        principal?.FindFirst(CustomClaims.PreferredUsername)?.Value ??
        throw new ApplicationException("User name is unavailable");
}
