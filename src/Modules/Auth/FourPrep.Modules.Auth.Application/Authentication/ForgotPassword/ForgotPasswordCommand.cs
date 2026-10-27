namespace FourPrep.Modules.Auth.Application.Authentication.ForgotPassword;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record ForgotPasswordCommand(string Email) : ICommand;

internal sealed class ForgotPasswordCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.FindUserByEmailAsync(request.Email.Trim(), cancellationToken);

        if (user.IsSuccess && user.Data.IsActive)
        {
            await identityProviderClient.SendPasswordResetEmailAsync(user.Data.Id, cancellationToken);
        }

        return Result.Success();
    }
}
