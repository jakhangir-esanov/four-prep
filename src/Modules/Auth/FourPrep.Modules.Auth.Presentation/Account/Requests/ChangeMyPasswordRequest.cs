namespace FourPrep.Modules.Auth.Presentation.Account.Requests;

internal sealed record ChangeMyPasswordRequest
{
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
}
