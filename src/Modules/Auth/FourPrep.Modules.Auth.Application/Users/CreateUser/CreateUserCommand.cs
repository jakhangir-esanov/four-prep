namespace FourPrep.Modules.Auth.Application.Users.CreateUser;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Students;

public sealed record CreateUserCommand(
    string Username,
    string Email,
    string? PhoneNumber,
    string Password,
    string Role) : ICommand<Guid>;

internal sealed class CreateUserCommandHandler(
    IIdentityProviderClient identityProviderClient,
    IStudentService studentService) : ICommandHandler<CreateUserCommand, Guid>
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

        var roleGranted = await AccountRoles.GrantAsync(
            identityProviderClient,
            studentService,
            createResult.Data,
            request.Username,
            request.Role,
            cancellationToken);

        return roleGranted.IsFailure
            ? Result.Failure<Guid>(roleGranted.Error)
            : Result.Success(Guid.Parse(createResult.Data));
    }
}
