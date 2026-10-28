namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FluentAssertions;
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

        result.IsSuccess.Should().BeTrue();
        identity.LoggedOutUserIds.Should().Equal(UserId);
        session.IsRevoked.Should().BeTrue();
        session.RevokedOnUtc.Should().Be(new FakeClock().UtcNow);
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_KeepsTheRecordsWhenKeycloakRefusesTheLogout()
    {
        var identity = new FakeIdentityProviderClient { LogoutSessionsFailure = IdentityProviderErrors.OperationFailed };
        var sessions = new FakeUserSessionRepository();
        var session = Session(sessions);
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(identity, sessions, unitOfWork).Handle(new LogoutAllCommand(), default);

        result.Error.Should().Be(IdentityProviderErrors.OperationFailed);
        session.IsRevoked.Should().BeFalse();
        unitOfWork.SaveCount.Should().Be(0);
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
