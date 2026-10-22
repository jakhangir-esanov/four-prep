namespace FourPrep.Modules.Auth.Application.Users.CreateUser;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record CreateUserCommand(
    string Username,
    string Email,
    string? PhoneNumber,
    string Password,
    string Role) : ICommand<Guid>;

internal sealed class CreateUserCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var createResult = await identityProviderClient.CreateUserAsync(
            request.Username,
            request.Email,
            request.PhoneNumber,
            emailVerified: false,
            isActive: true,
            password: request.Password,
            cancellationToken: cancellationToken);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var userId = Guid.Parse(createResult.Data);

        var roleAssigned = await identityProviderClient.AssignRealmRoleAsync(userId, request.Role, cancellationToken);
        if (roleAssigned.IsFailure)
        {
            await identityProviderClient.DeleteUserAsync(createResult.Data, cancellationToken);
            return Result.Failure<Guid>(roleAssigned.Error);
        }

        return Result.Success(userId);
    }
}
