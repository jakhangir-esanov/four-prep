namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Authentication.LogoutAll;

public class LogoutAllCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_EndsTheKeycloakSessionsAndRevokesTheRecords()
    {
        var identity = new FakeIdentityProviderClient();
        var sessions = new FakeUserSessionRepository();
        var session = Session(sessions);
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(identity, sessions, unitOfWork).Handle(new LogoutAllCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserId, Assert.Single(identity.LoggedOutUserIds));
        Assert.True(session.IsRevoked);
        Assert.Equal(new FakeClock().UtcNow, session.RevokedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_KeepsTheRecordsWhenKeycloakRefusesTheLogout()
    {
        var identity = new FakeIdentityProviderClient { LogoutSessionsFailure = IdentityProviderErrors.OperationFailed };
        var sessions = new FakeUserSessionRepository();
        var session = Session(sessions);
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(identity, sessions, unitOfWork).Handle(new LogoutAllCommand(), default);

        Assert.Equal(IdentityProviderErrors.OperationFailed, result.Error);
        Assert.False(session.IsRevoked);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static UserSession Session(FakeUserSessionRepository sessions)
    {
        var clock = new FakeClock();
        var session = UserSession.Create(UserId, "sid-1", "hash", clock.UtcNow.AddMinutes(30), clock.UtcNow);
        sessions.Insert(session);
        return session;
    }

    private static LogoutAllCommandHandler Handler(
        FakeIdentityProviderClient identity,
        FakeUserSessionRepository sessions,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(UserId), new FakeClock(), sessions, identity);
}
