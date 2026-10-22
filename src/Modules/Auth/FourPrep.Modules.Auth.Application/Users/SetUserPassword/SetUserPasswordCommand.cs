namespace FourPrep.Modules.Auth.Application.Users.SetUserPassword;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record SetUserPasswordCommand(Guid UserId, string Password) : ICommand;

internal sealed class SetUserPasswordCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<SetUserPasswordCommand>
{
    public Task<Result> Handle(SetUserPasswordCommand request, CancellationToken cancellationToken) =>
        identityProviderClient.SetUserPasswordAsync(request.UserId, request.Password, cancellationToken);
}
