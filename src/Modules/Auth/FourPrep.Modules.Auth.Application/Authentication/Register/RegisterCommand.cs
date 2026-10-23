namespace FourPrep.Modules.Auth.Application.Authentication.Register;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Students;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string? PhoneNumber,
    string Password,
    string Role) : ICommand<AuthTokenResponse>;

internal sealed class RegisterCommandHandler(
    IIdentityProviderClient identityProviderClient,
    IStudentService studentService,
    IUserSessionRepository sessionRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RegisterCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
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
            return Result.Failure<AuthTokenResponse>(createResult.Error);
        }

        var roleGranted = await AccountRoles.GrantAsync(
            identityProviderClient,
            studentService,
            createResult.Data,
            request.Username,
            request.Role,
            cancellationToken);

        if (roleGranted.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(roleGranted.Error);
        }

        var loginResult = await identityProviderClient.LoginAsync(request.Username, request.Password, cancellationToken);
        if (loginResult.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(loginResult.Error);
        }

        byte[] bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(loginResult.Data.AccessToken));

        var session = UserSession.Create(
            loginResult.Data.UserId,
            Convert.ToHexString(bytes),
            loginResult.Data.ExpiresAt,
            dateTimeProvider.UtcNow);

        sessionRepository.Insert(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            loginResult.Data.AccessToken,
            loginResult.Data.RefreshToken,
            loginResult.Data.IdToken,
            loginResult.Data.TokenType,
            loginResult.Data.ExpiresIn,
            loginResult.Data.RefreshExpiresIn);
    }
}
