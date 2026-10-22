namespace FourPrep.Modules.Auth.Application.Authentication.Logout;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

internal sealed class LogoutCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await identityProviderClient.LogoutAsync(request.RefreshToken, cancellationToken);
    }
}
