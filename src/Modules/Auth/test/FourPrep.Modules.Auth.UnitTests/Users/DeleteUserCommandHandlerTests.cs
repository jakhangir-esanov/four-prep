namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Users.DeleteUser;

public class DeleteUserCommandHandlerTests
{
    private static readonly Guid AdminId = Guid.NewGuid();

    private readonly FakeIdentityProviderClient _identity = new();
    private readonly FakeStudentService _students = new();
    private readonly FakeUserSessionRepository _sessions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Handle_DeletesTheAccountItsStudentDataAndSessions()
    {
        var user = _identity.Add("aziz", isActive: true);
        var clock = new FakeClock();
        _sessions.Insert(UserSession.Create(user.Id, "sid", "hash", clock.UtcNow, clock.UtcNow));

        var result = await Handler().Handle(new DeleteUserCommand(user.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(_identity.Users);
        Assert.Equal(user.Id, Assert.Single(_students.ErasedUserIds));
        Assert.Empty(_sessions.Inserted);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_TreatsAnAlreadyMissingAccountAsDeleted()
    {
        var result = await Handler().Handle(new DeleteUserCommand(Guid.NewGuid()), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_RefusesToDeleteTheAdminThemself()
    {
        var result = await Handler().Handle(new DeleteUserCommand(AdminId), default);

        Assert.Equal(UserErrors.CannotDeleteSelf, result.Error);
        Assert.Empty(_students.ErasedUserIds);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private DeleteUserCommandHandler Handler() =>
        new(_unitOfWork, new FakeCurrentUser(AdminId), _students, _sessions, _identity);
}
