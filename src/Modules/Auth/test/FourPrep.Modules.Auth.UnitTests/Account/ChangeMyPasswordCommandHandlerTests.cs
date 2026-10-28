namespace FourPrep.Modules.Auth.UnitTests.Account;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Application.Account.ChangeMyPassword;

public class ChangeMyPasswordCommandHandlerTests
{
    private readonly FakeIdentityProviderClient _identity = new();
    private readonly FakeUserSessionRepository _sessions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly UserResponse _user;

    public ChangeMyPasswordCommandHandlerTests()
    {
        _user = _identity.Add("aziz", isActive: true);
        _identity.Passwords[_user.Id] = "OldPassw0rd";
    }

    [Fact]
    public async Task Handle_ChangesThePasswordEndsOtherSessionsAndSignsInAgain()
    {
        var otherDevice = OtherDeviceSession();

        var result = await Handler().Handle(new ChangeMyPasswordCommand("OldPassw0rd", "NewPassw0rd"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-NewPassw0rd", result.Data.AccessToken);
        Assert.Equal("NewPassw0rd", _identity.Passwords[_user.Id]);
        Assert.Equal(_user.Id, Assert.Single(_identity.LoggedOutUserIds));
        Assert.True(otherDevice.IsRevoked);
        Assert.Contains(_sessions.Inserted, session => session.IdentitySessionId == "sid-NewPassw0rd" && !session.IsRevoked);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_EndsTheSessionOpenedToCheckTheCurrentPassword()
    {
        await Handler().Handle(new ChangeMyPasswordCommand("OldPassw0rd", "NewPassw0rd"), default);

        Assert.Equal("refresh-OldPassw0rd", Assert.Single(_identity.LoggedOutRefreshTokens));
    }

    [Fact]
    public async Task Handle_RefusesAWrongCurrentPasswordAndChangesNothing()
    {
        var otherDevice = OtherDeviceSession();

        var result = await Handler().Handle(new ChangeMyPasswordCommand("Guess1234", "NewPassw0rd"), default);

        Assert.Equal(UserErrors.WrongPassword, result.Error);
        Assert.Equal("OldPassw0rd", _identity.Passwords[_user.Id]);
        Assert.Empty(_identity.LoggedOutUserIds);
        Assert.False(otherDevice.IsRevoked);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_TellsAGoogleOnlyAccountToCreateAPasswordFirst()
    {
        _identity.Passwords.Remove(_user.Id);

        var result = await Handler().Handle(new ChangeMyPasswordCommand("anything", "NewPassw0rd"), default);

        Assert.Equal(UserErrors.PasswordNotSet, result.Error);
        Assert.Empty(_identity.LoggedOutUserIds);
    }

    [Fact]
    public async Task Handle_PassesOnAPasswordKeycloakRejectsWithoutEndingSessions()
    {
        var identity = new FakeIdentityProviderClient { SetPasswordFailure = UserErrors.PasswordNotAllowed };
        var user = identity.Add("aziz", isActive: true);
        identity.Passwords[user.Id] = "OldPassw0rd";
        var handler = new ChangeMyPasswordCommandHandler(
            _unitOfWork, new FakeCurrentUser(user.Id), new FakeClock(), _sessions, identity);

        var result = await handler.Handle(new ChangeMyPasswordCommand("OldPassw0rd", "aziz@example.com"), default);

        Assert.Equal(UserErrors.PasswordNotAllowed, result.Error);
        Assert.Empty(identity.LoggedOutUserIds);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesTheUsernameAsTheNewPassword()
    {
        var result = await Handler().Handle(new ChangeMyPasswordCommand("OldPassw0rd", "AZIZ"), default);

        Assert.Equal(UserErrors.PasswordNotAllowed, result.Error);
        Assert.Equal("OldPassw0rd", _identity.Passwords[_user.Id]);
    }

    [Fact]
    public async Task Handle_ReportsAFailedLogoutInsteadOfLeavingOldSessionsAlive()
    {
        var identity = new FakeIdentityProviderClient { LogoutSessionsFailure = IdentityProviderErrors.OperationFailed };
        var user = identity.Add("aziz", isActive: true);
        identity.Passwords[user.Id] = "OldPassw0rd";
        var otherDevice = UserSession.Create(user.Id, "sid-laptop", "hash", new FakeClock().UtcNow, new FakeClock().UtcNow);
        _sessions.Insert(otherDevice);
        var handler = new ChangeMyPasswordCommandHandler(
            _unitOfWork, new FakeCurrentUser(user.Id), new FakeClock(), _sessions, identity);

        var result = await handler.Handle(new ChangeMyPasswordCommand("OldPassw0rd", "NewPassw0rd"), default);

        Assert.Equal(IdentityProviderErrors.OperationFailed, result.Error);
        Assert.False(otherDevice.IsRevoked);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private UserSession OtherDeviceSession()
    {
        var clock = new FakeClock();
        var session = UserSession.Create(_user.Id, "sid-laptop", "hash", clock.UtcNow.AddMinutes(30), clock.UtcNow);
        _sessions.Insert(session);
        return session;
    }

    private ChangeMyPasswordCommandHandler Handler() =>
        new(_unitOfWork, new FakeCurrentUser(_user.Id), new FakeClock(), _sessions, _identity);
}
