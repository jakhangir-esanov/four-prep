namespace FourPrep.Modules.Auth.Application.Account.UpdateMyAccount;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

// The new values reach the access token on the next refresh.
public sealed record UpdateMyAccountCommand(
    string? PhoneNumber,
    string Country,
    string? TimeZone,
    string? Locale) : ICommand;

internal sealed class UpdateMyAccountCommandHandler(
    ICurrentUser currentUser,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<UpdateMyAccountCommand>
{
    public async Task<Result> Handle(UpdateMyAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.GetUserAsync(currentUser.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        return await identityProviderClient.UpdateUserAsync(
            user.Data.Id.ToString(),
            user.Data.UserName,
            user.Data.Email ?? string.Empty,
            UserAttributes.Normalized(request.PhoneNumber, request.Country, request.TimeZone, request.Locale),
            user.Data.EmailVerified,
            user.Data.IsActive,
            cancellationToken);
    }
}
