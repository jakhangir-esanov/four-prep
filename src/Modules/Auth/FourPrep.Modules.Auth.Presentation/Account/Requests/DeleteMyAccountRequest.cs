namespace FourPrep.Modules.Auth.Presentation.Account.Requests;

internal sealed record DeleteMyAccountRequest
{
    public string? Password { get; init; }
}
