namespace FourPrep.Modules.Auth.Application.Authentication.Login;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record LoginCommand(
    string Username,
    string Password) : ICommand<AuthTokenResponse>;

internal sealed class LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<LoginCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
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
