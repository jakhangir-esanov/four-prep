namespace FourPrep.Modules.Auth.Application.Users.GetUser;

public sealed record UserFilter(
    string? Search = null,
    bool? IsActive = null,
    string? Role = null)
{
    public static readonly UserFilter None = new();
}
