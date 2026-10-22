namespace FourPrep.Modules.Auth.Application.Users.DeleteUser;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;

internal sealed class DeleteUserCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var deleted = await identityProviderClient.DeleteUserAsync(request.UserId.ToString(), cancellationToken);

        return deleted.IsFailure && deleted.Error != IdentityProviderErrors.UserNotFound
            ? deleted
            : Result.Success();
    }
}
