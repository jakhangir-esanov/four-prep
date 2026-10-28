namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Users.SetUserPassword;

public class SetUserPasswordCommandHandlerTests
{
    private readonly FakeIdentityProviderClient _identity = new();
    private readonly FakeUserSessionRepository _sessions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Handle_SetsThePasswordAndEndsTheUsersSessions()
    {
        var user = _identity.Add("aziz", isActive: true);
        var clock = new FakeClock();
        var session = UserSession.Create(user.Id, "sid", "hash", clock.UtcNow, clock.UtcNow);
        _sessions.Insert(session);

        var result = await Handler().Handle(new SetUserPasswordCommand(user.Id, "NewPassw0rd"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("NewPassw0rd", _identity.Passwords[user.Id]);
        Assert.Equal(user.Id, Assert.Single(_identity.LoggedOutUserIds));
        Assert.True(session.IsRevoked);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsAnUnknownAccountWithoutEndingSessions()
    {
        var result = await Handler().Handle(new SetUserPasswordCommand(Guid.NewGuid(), "NewPassw0rd"), default);

        Assert.Equal(UserErrors.NotFound, result.Error);
        Assert.Empty(_identity.LoggedOutUserIds);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private SetUserPasswordCommandHandler Handler() =>
        new(_unitOfWork, new FakeClock(), _sessions, _identity);
}
