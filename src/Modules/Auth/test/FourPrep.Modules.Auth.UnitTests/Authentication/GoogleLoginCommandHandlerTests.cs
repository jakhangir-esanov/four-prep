namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Authentication.GoogleLogin;

public class GoogleLoginCommandHandlerTests
{
    private const string GoogleUserName = "google-108234567890";

    [Fact]
    public async Task Handle_SignsInAReturningUserWithoutTouchingTheirAccount()
    {
        var identity = new FakeIdentityProviderClient();
        var user = AddGoogleUser(identity);
        identity.RealmRoles[user.Id] = [Roles.Student];
        var students = new FakeStudentService();
        var (handler, sessions, unitOfWork) = Build(identity, students);

        var result = await handler.Handle(new GoogleLoginCommand("id-token", null, null, null, null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("google-access-token", result.Data.AccessToken);
        Assert.Empty(identity.AssignedRoles);
        Assert.Empty(students.Created);
        Assert.Equal(user.Id, Assert.Single(sessions.Inserted).UserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_AsksANewUserForSignUpDetailsAndEndsTheirKeycloakSession()
    {
        var identity = new FakeIdentityProviderClient();
        var user = AddGoogleUser(identity);
        var (handler, sessions, _) = Build(identity, new FakeStudentService());

        var result = await handler.Handle(new GoogleLoginCommand("id-token", Roles.Student, null, null, null), default);

        Assert.Equal(AuthenticationErrors.SignUpDetailsRequired, result.Error);
        Assert.Equal("google-refresh-token", Assert.Single(identity.LoggedOutRefreshTokens));
        Assert.Contains(user.Id, identity.Users.Keys);
        Assert.Empty(identity.AssignedRoles);
        Assert.Empty(sessions.Inserted);
    }

    [Fact]
    public async Task Handle_SignsUpANewStudentAndReturnsATokenThatCarriesTheRole()
    {
        var identity = new FakeIdentityProviderClient();
        var user = AddGoogleUser(identity);
        var students = new FakeStudentService();
        var (handler, sessions, _) = Build(identity, students);

        var result = await handler.Handle(
            new GoogleLoginCommand("id-token", Roles.Student, "us", "America/New_York", "EN"),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal("refreshed-access-token", result.Data.AccessToken);
        Assert.Equal((user.Id, Roles.Student), Assert.Single(identity.AssignedRoles));
        Assert.Equal((user.Id, GoogleUserName, "US"), Assert.Single(students.Created));
        var updated = identity.Users[user.Id];
        Assert.Equal("US", updated.Country);
        Assert.Equal("America/New_York", updated.TimeZone);
        Assert.Equal("en", updated.Locale);
        Assert.True(updated.EmailVerified);
        Assert.Single(sessions.Inserted);
        Assert.Empty(identity.LoggedOutRefreshTokens);
    }

    [Fact]
    public async Task Handle_SignsUpANewParentWithoutAStudentProfile()
    {
        var identity = new FakeIdentityProviderClient();
        var user = AddGoogleUser(identity);
        var students = new FakeStudentService();
        var (handler, _, _) = Build(identity, students);

        var result = await handler.Handle(new GoogleLoginCommand("id-token", Roles.Parent, "GB", null, null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal((user.Id, Roles.Parent), Assert.Single(identity.AssignedRoles));
        Assert.Empty(students.Created);
    }

    [Fact]
    public async Task Handle_DeletesTheNewAccountWhenTheStudentProfileFails()
    {
        var identity = new FakeIdentityProviderClient();
        AddGoogleUser(identity);
        var students = new FakeStudentService { CreateFailure = IdentityProviderErrors.OperationFailed };
        var (handler, sessions, _) = Build(identity, students);

        var result = await handler.Handle(new GoogleLoginCommand("id-token", Roles.Student, "US", null, null), default);

        Assert.Equal(IdentityProviderErrors.OperationFailed, result.Error);
        Assert.Empty(identity.Users);
        Assert.Empty(sessions.Inserted);
    }

    [Theory]
    [InlineData(Roles.Mentor)]
    [InlineData(Roles.Teacher)]
    [InlineData(Roles.MentorAdmin)]
    [InlineData(Roles.ContentAdmin)]
    [InlineData(Roles.SuperAdmin)]
    public async Task Handle_RefusesAStaffAccountAndEndsItsKeycloakSession(string staffRole)
    {
        var identity = new FakeIdentityProviderClient();
        var user = AddGoogleUser(identity);
        identity.RealmRoles[user.Id] = [staffRole];
        var (handler, sessions, _) = Build(identity, new FakeStudentService());

        var result = await handler.Handle(new GoogleLoginCommand("id-token", Roles.Student, "US", null, null), default);

        Assert.Equal(AuthenticationErrors.GoogleSignInNotAllowed, result.Error);
        Assert.Equal("google-refresh-token", Assert.Single(identity.LoggedOutRefreshTokens));
        Assert.Empty(identity.AssignedRoles);
        Assert.Contains(user.Id, identity.Users.Keys);
        Assert.Empty(sessions.Inserted);
    }

    [Fact]
    public async Task Handle_RefusesAUserWhoIsAlsoStaff()
    {
        var identity = new FakeIdentityProviderClient();
        var user = AddGoogleUser(identity);
        identity.RealmRoles[user.Id] = [Roles.Parent, Roles.Teacher];
        var (handler, _, _) = Build(identity, new FakeStudentService());

        var result = await handler.Handle(new GoogleLoginCommand("id-token", null, null, null, null), default);

        Assert.Equal(AuthenticationErrors.GoogleSignInNotAllowed, result.Error);
    }

    [Fact]
    public async Task Handle_PassesOnTheStaffRefusalKeycloakReports()
    {
        var identity = new FakeIdentityProviderClient();
        var (handler, sessions, _) = Build(identity, new FakeStudentService());
        identity.GoogleLoginResult = Result.Failure<IdentityTokenResult>(AuthenticationErrors.GoogleSignInNotAllowed);

        var result = await handler.Handle(new GoogleLoginCommand("id-token", null, null, null, null), default);

        Assert.Equal(AuthenticationErrors.GoogleSignInNotAllowed, result.Error);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(sessions.Inserted);
    }

    [Fact]
    public async Task Handle_ReportsARejectedGoogleToken()
    {
        var identity = new FakeIdentityProviderClient();
        var (handler, sessions, _) = Build(identity, new FakeStudentService());
        identity.GoogleLoginResult = Result.Failure<IdentityTokenResult>(AuthenticationErrors.InvalidGoogleToken);

        var result = await handler.Handle(new GoogleLoginCommand("forged", Roles.Student, "US", null, null), default);

        Assert.Equal(AuthenticationErrors.InvalidGoogleToken, result.Error);
        Assert.Empty(identity.LoggedOutRefreshTokens);
        Assert.Empty(sessions.Inserted);
    }

    private static UserResponse AddGoogleUser(FakeIdentityProviderClient identity)
    {
        var user = new UserResponse(Guid.NewGuid(), GoogleUserName, "aziz@gmail.com", true, null, true, null);
        identity.Users[user.Id] = user;
        identity.GoogleLoginResult = Tokens("google", user.Id);
        identity.RefreshResult = Tokens("refreshed", user.Id);
        return user;
    }

    private static Result<IdentityTokenResult> Tokens(string prefix, Guid userId) =>
        Result.Success(new IdentityTokenResult(
            $"{prefix}-access-token",
            $"{prefix}-refresh-token",
            null,
            "Bearer",
            1800,
            2592000,
            userId,
            new DateTime(2026, 9, 18, 12, 30, 0, DateTimeKind.Utc)));

    private static (GoogleLoginCommandHandler, FakeUserSessionRepository, FakeUnitOfWork) Build(
        FakeIdentityProviderClient identity,
        FakeStudentService students)
    {
        var sessions = new FakeUserSessionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new GoogleLoginCommandHandler(identity, students, sessions, unitOfWork, new FakeClock());

        return (handler, sessions, unitOfWork);
    }
}
