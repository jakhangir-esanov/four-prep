namespace FourPrep.Modules.Auth.UnitTests.Account;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Application.Account.DeleteMyAccount;

public class DeleteMyAccountCommandHandlerTests
{
    private const string CurrentSessionId = "sid-current";

    private readonly FakeIdentityProviderClient _identity = new();
    private readonly FakeStudentService _students = new();
    private readonly FakeUserSessionRepository _sessions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly UserResponse _user;

    public DeleteMyAccountCommandHandlerTests()
    {
        _user = _identity.Add("aziz", isActive: true);
    }

    [Fact]
    public async Task Handle_ErasesTheAccountItsStudentDataAndSessionsWhenThePasswordIsRight()
    {
        _identity.Passwords[_user.Id] = "Passw0rd!23";
        AddSession(_user.Id, CurrentSessionId, minutesAgo: 120);

        var result = await Handler(_students).Handle(new DeleteMyAccountCommand("Passw0rd!23"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(_identity.Users);
        Assert.Equal(_user.Id, Assert.Single(_students.ErasedUserIds));
        Assert.Empty(_sessions.Inserted);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesAWrongPasswordAndKeepsEverything()
    {
        _identity.Passwords[_user.Id] = "Passw0rd!23";

        var result = await Handler(_students).Handle(new DeleteMyAccountCommand("guess"), default);

        Assert.Equal(UserErrors.WrongPassword, result.Error);
        Assert.Contains(_user.Id, _identity.Users.Keys);
        Assert.Empty(_students.ErasedUserIds);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesAMissingPasswordForAnAccountThatHasOne()
    {
        _identity.Passwords[_user.Id] = "Passw0rd!23";

        var result = await Handler(_students).Handle(new DeleteMyAccountCommand(null), default);

        Assert.Equal(UserErrors.WrongPassword, result.Error);
        Assert.Contains(_user.Id, _identity.Users.Keys);
    }

    [Fact]
    public async Task Handle_ErasesAGoogleOnlyAccountRightAfterItSignedIn()
    {
        AddSession(_user.Id, CurrentSessionId, minutesAgo: 5);

        var result = await Handler(_students).Handle(new DeleteMyAccountCommand(null), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(_identity.Users);
        Assert.Equal(_user.Id, Assert.Single(_students.ErasedUserIds));
    }

    [Fact]
    public async Task Handle_AsksAGoogleOnlyAccountToSignInAgainWhenTheSessionIsOld()
    {
        AddSession(_user.Id, CurrentSessionId, minutesAgo: 11);

        var result = await Handler(_students).Handle(new DeleteMyAccountCommand(null), default);

        Assert.Equal(UserErrors.ReauthenticationRequired, result.Error);
        Assert.Contains(_user.Id, _identity.Users.Keys);
        Assert.Empty(_students.ErasedUserIds);
    }

    [Fact]
    public async Task Handle_DoesNotAcceptAFreshSessionOfAnotherUser()
    {
        AddSession(Guid.NewGuid(), CurrentSessionId, minutesAgo: 1);

        var result = await Handler(_students).Handle(new DeleteMyAccountCommand(null), default);

        Assert.Equal(UserErrors.ReauthenticationRequired, result.Error);
        Assert.Contains(_user.Id, _identity.Users.Keys);
    }

    [Fact]
    public async Task Handle_KeepsTheAccountWhenTheStudentDataCannotBeErased()
    {
        _identity.Passwords[_user.Id] = "Passw0rd!23";
        var students = new FakeStudentService { EraseFailure = IdentityProviderErrors.OperationFailed };

        var result = await Handler(students).Handle(new DeleteMyAccountCommand("Passw0rd!23"), default);

        Assert.Equal(IdentityProviderErrors.OperationFailed, result.Error);
        Assert.Contains(_user.Id, _identity.Users.Keys);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private void AddSession(Guid userId, string sessionId, int minutesAgo)
    {
        var createdOnUtc = new FakeClock().UtcNow.AddMinutes(-minutesAgo);
        _sessions.Insert(UserSession.Create(userId, sessionId, "hash", createdOnUtc.AddMinutes(30), createdOnUtc));
    }

    private DeleteMyAccountCommandHandler Handler(FakeStudentService students) =>
        new(_unitOfWork, new FakeCurrentUser(_user.Id, CurrentSessionId), new FakeClock(), students, _sessions, _identity);
}
