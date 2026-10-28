namespace FourPrep.Modules.Auth.Application.Authentication.Refresh;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record RefreshCommand(string RefreshToken) : ICommand<AuthTokenResponse>;

internal sealed class RefreshCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<RefreshCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var refreshResult = await identityProviderClient.RefreshAsync(request.RefreshToken, cancellationToken);
        if (refreshResult.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(refreshResult.Error);
        }

        var tokens = refreshResult.Data;
        var session = await FindSessionAsync(tokens, cancellationToken);

        if (session is { IsRevoked: true })
        {
            await identityProviderClient.LogoutAsync(tokens.RefreshToken, cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthenticationErrors.InvalidRefreshToken);
        }

        if (session is not null)
        {
            session.Refresh(AccessTokenHash.Of(tokens.AccessToken), tokens.ExpiresAt, dateTimeProvider.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return AuthTokenResponse.From(tokens);
    }

    private async Task<UserSession?> FindSessionAsync(IdentityTokenResult tokens, CancellationToken cancellationToken) =>
        string.IsNullOrEmpty(tokens.SessionId)
            ? null
            : await sessionRepository.GetByIdentitySessionIdAsync(tokens.SessionId, cancellationToken);
}
