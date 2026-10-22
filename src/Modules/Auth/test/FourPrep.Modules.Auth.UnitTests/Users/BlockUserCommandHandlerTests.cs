namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Application.Users.BlockUser;

public class BlockUserCommandHandlerTests
{
    private static readonly Guid AdminId = Guid.NewGuid();

    [Fact]
    public async Task Handle_DisablesTheAccountAndEndsItsSessions()
    {
        var (handler, identity, sessions, unitOfWork) = Build();
        var user = identity.Add("ali", isActive: true);

        var result = await handler.Handle(new BlockUserCommand(user.Id), default);

        Assert.True(result.IsSuccess);
        Assert.False(identity.Users[user.Id].IsActive);
        Assert.Contains(user.Id, identity.LoggedOutUserIds);
        Assert.Contains(user.Id, sessions.RevokedUserIds);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesToBlockTheAdministratorThemself()
    {
        var (handler, _, _, unitOfWork) = Build();

        var result = await handler.Handle(new BlockUserCommand(AdminId), default);

        Assert.Equal(UserErrors.CannotBlockSelf, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsAnUnknownAccount()
    {
        var (handler, _, _, unitOfWork) = Build();

        var result = await handler.Handle(new BlockUserCommand(Guid.NewGuid()), default);

        Assert.Equal(UserErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesAnAccountThatIsAlreadyDisabled()
    {
        var (handler, identity, _, unitOfWork) = Build();
        var user = identity.Add("ali", isActive: false);

        var result = await handler.Handle(new BlockUserCommand(user.Id), default);

        Assert.Equal(UserErrors.AlreadyBlocked, result.Error);
        Assert.Empty(identity.LoggedOutUserIds);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static (
        BlockUserCommandHandler Handler,
        FakeIdentityProviderClient Identity,
        FakeUserSessionRepository Sessions,
        FakeUnitOfWork UnitOfWork) Build()
    {
        var identity = new FakeIdentityProviderClient();
        var sessions = new FakeUserSessionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new BlockUserCommandHandler(
            unitOfWork, new FakeCurrentUser(AdminId), new FakeClock(), sessions, identity);

        return (handler, identity, sessions, unitOfWork);
    }
}
