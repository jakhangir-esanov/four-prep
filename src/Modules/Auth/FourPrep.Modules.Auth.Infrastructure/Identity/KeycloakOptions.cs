namespace FourPrep.Modules.Auth.Infrastructure.Identity;

internal sealed class KeycloakOptions
{
    public string BaseUrl { get; init; } = null!;
    public string Realm { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientSecret { get; init; } = null!;
    public string? EmailRedirectUrl { get; init; }
}
