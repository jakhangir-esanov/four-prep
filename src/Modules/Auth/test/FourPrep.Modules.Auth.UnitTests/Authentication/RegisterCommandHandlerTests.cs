namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Authentication.Register;

public class RegisterCommandHandlerTests
{
    private static RegisterCommand Command(string role) =>
        new("Aziz.Karimov", "aziz@example.com", "+1 (415) 555-0100", "Passw0rd!23", "us", "America/Los_Angeles", "EN", role);

    [Fact]
    public async Task Handle_CreatesAStudentWithAProfileAndSignsThemIn()
    {
        var identity = new FakeIdentityProviderClient();
        var students = new FakeStudentService();
        var (handler, sessions, unitOfWork) = Build(identity, students);

        var result = await handler.Handle(Command(Roles.Student), default);

        Assert.True(result.IsSuccess);
        var user = Assert.Single(identity.Users.Values);
        Assert.Equal((user.Id, Roles.Student), Assert.Single(identity.AssignedRoles));
        Assert.Equal((user.Id, "aziz.karimov", "US"), Assert.Single(students.Created));
        Assert.Equal("access-token", result.Data.AccessToken);
        Assert.Single(sessions.Inserted);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_StoresTheInternationalProfileInNormalFormAndSendsAVerificationEmail()
    {
        var identity = new FakeIdentityProviderClient();
        var (handler, _, _) = Build(identity, new FakeStudentService());

        await handler.Handle(Command(Roles.Student), default);

        var user = Assert.Single(identity.Users.Values);
        Assert.Equal("+14155550100", user.PhoneNumber);
        Assert.Equal("US", user.Country);
        Assert.Equal("America/Los_Angeles", user.TimeZone);
        Assert.Equal("en", user.Locale);
        Assert.Equal(user.Id, Assert.Single(identity.VerificationEmails));
    }

    [Fact]
    public async Task Handle_CreatesAParentWithoutAStudentProfile()
    {
        var identity = new FakeIdentityProviderClient();
        var students = new FakeStudentService();
        var (handler, _, _) = Build(identity, students);

        var result = await handler.Handle(Command(Roles.Parent), default);

        Assert.True(result.IsSuccess);
        var user = Assert.Single(identity.Users.Values);
        Assert.Equal((user.Id, Roles.Parent), Assert.Single(identity.AssignedRoles));
        Assert.Empty(students.Created);
    }

    [Fact]
    public async Task Handle_ReportsATakenUsernameWithoutAssigningARole()
    {
        var identity = new FakeIdentityProviderClient { CreateFailure = UserErrors.UsernameTaken };
        var (handler, sessions, _) = Build(identity, new FakeStudentService());

        var result = await handler.Handle(Command(Roles.Student), default);

        Assert.Equal(UserErrors.UsernameTaken, result.Error);
        Assert.Empty(identity.AssignedRoles);
        Assert.Empty(sessions.Inserted);
    }

    [Fact]
    public async Task Handle_DeletesTheAccountWhenTheRoleIsRefused()
    {
        var identity = new FakeIdentityProviderClient { AssignRoleFailure = IdentityProviderErrors.RoleNotFound };
        var students = new FakeStudentService();
        var (handler, sessions, _) = Build(identity, students);

        var result = await handler.Handle(Command(Roles.Student), default);

        Assert.Equal(IdentityProviderErrors.RoleNotFound, result.Error);
        Assert.Empty(identity.Users);
        Assert.Empty(students.Created);
        Assert.Empty(sessions.Inserted);
    }

    [Fact]
    public async Task Handle_DeletesTheAccountWhenTheStudentProfileFails()
    {
        var identity = new FakeIdentityProviderClient();
        var students = new FakeStudentService { CreateFailure = IdentityProviderErrors.OperationFailed };
        var (handler, sessions, _) = Build(identity, students);

        var result = await handler.Handle(Command(Roles.Student), default);

        Assert.Equal(IdentityProviderErrors.OperationFailed, result.Error);
        Assert.Empty(identity.Users);
        Assert.Empty(sessions.Inserted);
    }

    private static (RegisterCommandHandler, FakeUserSessionRepository, FakeUnitOfWork) Build(
        FakeIdentityProviderClient identity,
        FakeStudentService students)
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
        var handler = new RegisterCommandHandler(identity, students, sessions, unitOfWork, new FakeClock());

        return (handler, sessions, unitOfWork);
    }
}
