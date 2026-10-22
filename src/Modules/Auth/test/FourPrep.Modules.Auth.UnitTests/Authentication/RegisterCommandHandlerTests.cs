namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Authentication.Register;

public class RegisterCommandHandlerTests
{
    private static readonly RegisterCommand Command =
        new("aziz.karimov", "aziz@example.com", "+998901234567", "Passw0rd!23");

    [Fact]
    public async Task Handle_CreatesAStudentAndSignsThemIn()
    {
        var identity = new FakeIdentityProviderClient();
        var (handler, sessions, unitOfWork) = Build(identity);

        var result = await handler.Handle(Command, default);

        Assert.True(result.IsSuccess);
        var user = Assert.Single(identity.Users.Values);
        Assert.Equal((user.Id, Roles.Student), Assert.Single(identity.AssignedRoles));
        Assert.Equal("access-token", result.Data.AccessToken);
        Assert.Single(sessions.Inserted);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsATakenUsernameWithoutAssigningARole()
    {
        var identity = new FakeIdentityProviderClient { CreateFailure = UserErrors.UsernameTaken };
        var (handler, sessions, _) = Build(identity);

        var result = await handler.Handle(Command, default);

        Assert.Equal(UserErrors.UsernameTaken, result.Error);
        Assert.Empty(identity.AssignedRoles);
        Assert.Empty(sessions.Inserted);
    }

    [Fact]
    public async Task Handle_DeletesTheAccountWhenTheStudentRoleIsRefused()
    {
        var identity = new FakeIdentityProviderClient { AssignRoleFailure = IdentityProviderErrors.RoleNotFound };
        var (handler, sessions, _) = Build(identity);

        var result = await handler.Handle(Command, default);

        Assert.Equal(IdentityProviderErrors.RoleNotFound, result.Error);
        Assert.Empty(identity.Users);
        Assert.Empty(sessions.Inserted);
    }

    private static (RegisterCommandHandler, FakeUserSessionRepository, FakeUnitOfWork) Build(
        FakeIdentityProviderClient identity)
    {
        identity.LoginResult = Result.Success(new IdentityTokenResult(
            "access-token",
            "refresh-token",
            null,
            "Bearer",
            1800,
            2592000,
            Guid.NewGuid(),
            new DateTime(2026, 9, 18, 12, 30, 0, DateTimeKind.Utc)));

        var sessions = new FakeUserSessionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterCommandHandler(identity, sessions, unitOfWork, new FakeClock());

        return (handler, sessions, unitOfWork);
    }
}
