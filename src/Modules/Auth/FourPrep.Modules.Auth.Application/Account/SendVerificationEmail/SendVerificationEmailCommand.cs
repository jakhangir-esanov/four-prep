namespace FourPrep.Modules.Auth.Application.Account.SendVerificationEmail;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record SendVerificationEmailCommand : ICommand;

internal sealed class SendVerificationEmailCommandHandler(
    ICurrentUser currentUser,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<SendVerificationEmailCommand>
{
    public async Task<Result> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.GetUserAsync(currentUser.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        if (user.Data.EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        return await identityProviderClient.SendVerificationEmailAsync(currentUser.UserId, cancellationToken);
    }
}
