namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FluentAssertions;
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

        result.IsSuccess.Should().BeTrue();
        result.Data.AccessToken.Should().Be("new-access");
        sessions.Inserted.Should().ContainSingle();
        session.AccessTokenHash.Should().Be(AccessTokenHash.Of("new-access"));
        session.ExpiresOnUtc.Should().Be(NewExpiry);
        session.RefreshedOnUtc.Should().Be(new FakeClock().UtcNow);
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_StartsARecordWhenNoneMatchesTheKeycloakSession()
    {
        var sessions = new FakeUserSessionRepository();

        await Handler(Identity(), sessions, new FakeUnitOfWork()).Handle(new RefreshCommand("old-refresh"), default);

        var session = sessions.Inserted.Should().ContainSingle().Subject;
        session.UserId.Should().Be(UserId);
        session.IdentitySessionId.Should().Be(SessionId);
        session.AccessTokenHash.Should().Be(AccessTokenHash.Of("new-access"));
        session.RefreshedOnUtc.Should().BeNull();
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

        result.Error.Should().Be(AuthenticationErrors.InvalidRefreshToken);
        identity.LoggedOutRefreshTokens.Should().Equal("new-refresh");
        session.AccessTokenHash.Should().Be("old-hash");
        unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PassesOnAKeycloakRefusalWithoutTouchingSessions()
    {
        var sessions = new FakeUserSessionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(new FakeIdentityProviderClient(), sessions, unitOfWork)
            .Handle(new RefreshCommand("expired"), default);

        result.Error.Should().Be(AuthenticationErrors.InvalidRefreshToken);
        sessions.Inserted.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(0);
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
