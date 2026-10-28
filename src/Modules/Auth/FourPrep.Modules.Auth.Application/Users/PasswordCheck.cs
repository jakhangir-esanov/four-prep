namespace FourPrep.Modules.Auth.Application.Users;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

internal static class PasswordCheck
{
    public static async Task<Result> VerifyAsync(
        IIdentityProviderClient identityProviderClient,
        string userName,
        string? password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(password))
        {
            return Result.Failure(UserErrors.WrongPassword);
        }

        var login = await identityProviderClient.LoginAsync(userName, password, cancellationToken);
        if (login.IsFailure)
        {
            return Result.Failure(UserErrors.WrongPassword);
        }

        await identityProviderClient.LogoutAsync(login.Data.RefreshToken, cancellationToken);

        return Result.Success();
    }
}
