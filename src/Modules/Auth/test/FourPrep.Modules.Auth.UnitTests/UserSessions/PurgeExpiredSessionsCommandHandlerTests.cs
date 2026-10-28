namespace FourPrep.Modules.Auth.UnitTests.UserSessions;

using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.UserSessions.PurgeExpiredSessions;

public class PurgeExpiredSessionsCommandHandlerTests
{
    [Fact]
    public async Task Handle_RemovesOnlySessionsIdleLongerThanKeycloakAllows()
    {
        var sessions = new FakeUserSessionRepository();
        var now = new FakeClock().UtcNow;
        var dead = Add(sessions, expiresOnUtc: now.AddDays(-31));
        var recent = Add(sessions, expiresOnUtc: now.AddDays(-29));

        var result = await new PurgeExpiredSessionsCommandHandler(new FakeClock(), sessions)
            .Handle(new PurgeExpiredSessionsCommand(), default);

        Assert.Equal(1, result.Data);
        Assert.DoesNotContain(dead, sessions.Inserted);
        Assert.Contains(recent, sessions.Inserted);
    }

    private static UserSession Add(FakeUserSessionRepository sessions, DateTime expiresOnUtc)
    {
        var session = UserSession.Create(Guid.NewGuid(), "sid", "hash", expiresOnUtc, expiresOnUtc.AddMinutes(-30));
        sessions.Insert(session);
        return session;
    }
}
