namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Application.Authentication.Logout;

public class LogoutCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_EndsTheKeycloakSessionAndRevokesItsRecordOnly()
    {
        var identity = new FakeIdentityProviderClient();
        identity.SessionIdsByToken["refresh"] = "sid-phone";
        var sessions = new FakeUserSessionRepository();
        var phone = Session(sessions, "sid-phone");
        var laptop = Session(sessions, "sid-laptop");
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(identity, sessions, unitOfWork).Handle(new LogoutCommand("refresh"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("refresh", Assert.Single(identity.LoggedOutRefreshTokens));
        Assert.True(phone.IsRevoked);
        Assert.False(laptop.IsRevoked);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_LeavesTheRecordWhenKeycloakRefusesTheToken()
    {
        var identity = new FakeIdentityProviderClient { LogoutFailure = AuthenticationErrors.LogoutFailed };
        identity.SessionIdsByToken["refresh"] = "sid-phone";
        var sessions = new FakeUserSessionRepository();
        var phone = Session(sessions, "sid-phone");
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(identity, sessions, unitOfWork).Handle(new LogoutCommand("refresh"), default);

        Assert.Equal(AuthenticationErrors.LogoutFailed, result.Error);
        Assert.False(phone.IsRevoked);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static UserSession Session(FakeUserSessionRepository sessions, string sessionId)
    {
        var clock = new FakeClock();
        var session = UserSession.Create(UserId, sessionId, "hash", clock.UtcNow.AddMinutes(30), clock.UtcNow);
        sessions.Insert(session);
        return session;
    }

    private static LogoutCommandHandler Handler(
        FakeIdentityProviderClient identity,
        FakeUserSessionRepository sessions,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeClock(), sessions, identity);
}
