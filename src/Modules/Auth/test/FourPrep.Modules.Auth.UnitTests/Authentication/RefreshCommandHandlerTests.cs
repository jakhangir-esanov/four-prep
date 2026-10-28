namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Authentication.Refresh;

public class RefreshCommandHandlerTests
{
    private const string SessionId = "keycloak-session";
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime NewExpiry = new(2026, 9, 18, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_MovesTheSessionRecordToTheNewAccessToken()
    {
        var sessions = new FakeUserSessionRepository();
        var session = ExistingSession(sessions);
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(Identity(), sessions, unitOfWork).Handle(new RefreshCommand("old-refresh"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("new-access", result.Data.AccessToken);
        Assert.Single(sessions.Inserted);
        Assert.Equal(AccessTokenHash.Of("new-access"), session.AccessTokenHash);
        Assert.Equal(NewExpiry, session.ExpiresOnUtc);
        Assert.Equal(new FakeClock().UtcNow, session.RefreshedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_DoesNotStartARecordThatWouldPassForAFreshSignIn()
    {
        var sessions = new FakeUserSessionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(Identity(), sessions, unitOfWork).Handle(new RefreshCommand("old-refresh"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(sessions.Inserted);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesARevokedSessionAndEndsItInKeycloak()
    {
        var identity = Identity();
        var sessions = new FakeUserSessionRepository();
        var session = ExistingSession(sessions);
        session.Revoke(new FakeClock().UtcNow);
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(identity, sessions, unitOfWork).Handle(new RefreshCommand("old-refresh"), default);

        Assert.Equal(AuthenticationErrors.InvalidRefreshToken, result.Error);
        Assert.Equal("new-refresh", Assert.Single(identity.LoggedOutRefreshTokens));
        Assert.Equal("old-hash", session.AccessTokenHash);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_PassesOnAKeycloakRefusalWithoutTouchingSessions()
    {
        var sessions = new FakeUserSessionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(new FakeIdentityProviderClient(), sessions, unitOfWork)
            .Handle(new RefreshCommand("expired"), default);

        Assert.Equal(AuthenticationErrors.InvalidRefreshToken, result.Error);
        Assert.Empty(sessions.Inserted);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static FakeIdentityProviderClient Identity() =>
        new()
        {
            RefreshResult = Result.Success(new IdentityTokenResult(
                "new-access", "new-refresh", null, "Bearer", 1800, 2592000, UserId, NewExpiry, SessionId))
        };

    private static UserSession ExistingSession(FakeUserSessionRepository sessions)
    {
        var clock = new FakeClock();
        var session = UserSession.Create(UserId, SessionId, "old-hash", clock.UtcNow, clock.UtcNow.AddMinutes(-30));
        sessions.Insert(session);
        return session;
    }

    private static RefreshCommandHandler Handler(
        FakeIdentityProviderClient identity,
        FakeUserSessionRepository sessions,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeClock(), sessions, identity);
}
