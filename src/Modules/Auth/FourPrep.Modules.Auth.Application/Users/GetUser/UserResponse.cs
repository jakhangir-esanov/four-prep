namespace FourPrep.Modules.Auth.Application.Users.GetUser;

public sealed record UserResponse(
    Guid Id,
    string UserName,
    string? Email,
    bool EmailVerified,
    string? PhoneNumber,
    bool IsActive,
    DateTimeOffset? RegisteredOnUtc,
    string? Country = null,
    string? TimeZone = null,
    string? Locale = null);
