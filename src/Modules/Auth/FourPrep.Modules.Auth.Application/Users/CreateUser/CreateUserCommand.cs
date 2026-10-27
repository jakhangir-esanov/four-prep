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
    string Role,
    string? Country = null,
    string? TimeZone = null,
    string? Locale = null) : ICommand<Guid>;

internal sealed class CreateUserCommandHandler(
    IIdentityProviderClient identityProviderClient,
    IStudentService studentService) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var attributes = UserAttributes.Normalized(
            request.PhoneNumber,
            request.Country,
            request.TimeZone,
            request.Locale);

        var createResult = await identityProviderClient.CreateUserAsync(
            request.Username,
            request.Email,
            attributes,
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
            attributes.Country,
            request.Role,
            cancellationToken);

        return roleGranted.IsFailure
            ? Result.Failure<Guid>(roleGranted.Error)
            : Result.Success(Guid.Parse(createResult.Data));
    }
}
