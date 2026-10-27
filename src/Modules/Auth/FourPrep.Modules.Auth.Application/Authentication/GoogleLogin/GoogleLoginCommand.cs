namespace FourPrep.Modules.Auth.Application.Authentication.GoogleLogin;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Students;

public sealed record GoogleLoginCommand(
    string IdToken,
    string? Role,
    string? Country,
    string? TimeZone,
    string? Locale) : ICommand<AuthTokenResponse>;

internal sealed class GoogleLoginCommandHandler(
    IIdentityProviderClient identityProviderClient,
    IStudentService studentService,
    IUserSessionRepository sessionRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<GoogleLoginCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var loginResult = await identityProviderClient.LoginWithGoogleAsync(request.IdToken, cancellationToken);
        if (loginResult.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(loginResult.Error);
        }

        var tokens = await EnsureSignedUpAsync(loginResult.Data, request, cancellationToken);
        if (tokens.IsFailure)
        {
            await identityProviderClient.LogoutAsync(loginResult.Data.RefreshToken, cancellationToken);
            return Result.Failure<AuthTokenResponse>(tokens.Error);
        }

        byte[] bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(tokens.Data.AccessToken));

        var session = UserSession.Create(
            tokens.Data.UserId,
            Convert.ToHexString(bytes),
            tokens.Data.ExpiresAt,
            dateTimeProvider.UtcNow);

        sessionRepository.Insert(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            tokens.Data.AccessToken,
            tokens.Data.RefreshToken,
            tokens.Data.IdToken,
            tokens.Data.TokenType,
            tokens.Data.ExpiresIn,
            tokens.Data.RefreshExpiresIn);
    }

    private async Task<Result<IdentityTokenResult>> EnsureSignedUpAsync(
        IdentityTokenResult tokens,
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        var roles = await identityProviderClient.GetUserRealmRolesAsync(tokens.UserId, cancellationToken);
        if (roles.IsFailure)
        {
            return Result.Failure<IdentityTokenResult>(roles.Error);
        }

        if (roles.Data.Any(role => !IsUserRole(role)))
        {
            return Result.Failure<IdentityTokenResult>(AuthenticationErrors.GoogleSignInNotAllowed);
        }

        if (roles.Data.Count > 0)
        {
            return tokens;
        }

        var signedUp = await SignUpAsync(tokens.UserId, request, cancellationToken);
        if (signedUp.IsFailure)
        {
            return Result.Failure<IdentityTokenResult>(signedUp.Error);
        }

        return await identityProviderClient.RefreshAsync(tokens.RefreshToken, cancellationToken);
    }

    private static bool IsUserRole(string role) => role is Roles.Student or Roles.Parent;

    private async Task<Result> SignUpAsync(Guid userId, GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        if (request.Role is null || request.Country is null)
        {
            return Result.Failure(AuthenticationErrors.SignUpDetailsRequired);
        }

        var user = await identityProviderClient.GetUserAsync(userId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        var attributes = UserAttributes.Normalized(null, request.Country, request.TimeZone, request.Locale);

        var updated = await identityProviderClient.UpdateUserAsync(
            userId.ToString(),
            user.Data.UserName,
            user.Data.Email ?? string.Empty,
            attributes,
            user.Data.EmailVerified,
            user.Data.IsActive,
            cancellationToken);

        if (updated.IsFailure)
        {
            return updated;
        }

        return await AccountRoles.GrantAsync(
            identityProviderClient,
            studentService,
            userId.ToString(),
            user.Data.UserName,
            attributes.Country,
            request.Role,
            cancellationToken);
    }
}
