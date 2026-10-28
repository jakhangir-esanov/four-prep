namespace FourPrep.Modules.Auth.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Paging;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeUserSessionRepository : IUserSessionRepository
{
    public List<Guid> RevokedUserIds { get; } = [];

    public List<UserSession> Inserted { get; } = [];

    public Task<UserSession?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<UserSession?> GetByIdentitySessionIdAsync(
        string identitySessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Inserted.LastOrDefault(session => session.IdentitySessionId == identitySessionId));

    public void Insert(UserSession userSession) => Inserted.Add(userSession);

    public Task RevokeAllByUserIdAsync(Guid userId, DateTime revokedOnUtc, CancellationToken cancellationToken = default)
    {
        RevokedUserIds.Add(userId);

        foreach (var session in Inserted.Where(session => session.UserId == userId && !session.IsRevoked))
        {
            session.Revoke(revokedOnUtc);
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeStudentService : IStudentService
{
    public List<(Guid UserId, string UserName, string? Country)> Created { get; } = [];

    public Error? CreateFailure { get; init; }

    public Task<Result<Guid>> CreateStudentAsync(
        Guid userId, string userName, string? country, CancellationToken cancellationToken = default)
    {
        if (CreateFailure is not null)
        {
            return Task.FromResult(Result.Failure<Guid>(CreateFailure));
        }

        Created.Add((userId, userName, country));
        return Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}

internal sealed class FakeIdentityProviderClient : IIdentityProviderClient
{
    public Dictionary<Guid, UserResponse> Users { get; } = [];

    public List<Guid> LoggedOutUserIds { get; } = [];

    public List<(Guid UserId, string Role)> AssignedRoles { get; } = [];

    public Dictionary<Guid, List<string>> RealmRoles { get; } = [];

    public List<string> LoggedOutRefreshTokens { get; } = [];

    public Dictionary<Guid, string> Passwords { get; } = [];

    public List<Guid> VerificationEmails { get; } = [];

    public List<Guid> PasswordResetEmails { get; } = [];

    public Error? CreateFailure { get; init; }

    public Error? AssignRoleFailure { get; init; }

    public Error? LogoutSessionsFailure { get; init; }

    public Result<IdentityTokenResult> LoginResult { get; set; } =
        Result.Failure<IdentityTokenResult>(AuthenticationErrors.InvalidCredentials);

    public Result<IdentityTokenResult> GoogleLoginResult { get; set; } =
        Result.Failure<IdentityTokenResult>(AuthenticationErrors.InvalidGoogleToken);

    public Result<IdentityTokenResult> RefreshResult { get; set; } =
        Result.Failure<IdentityTokenResult>(AuthenticationErrors.InvalidRefreshToken);

    public UserResponse Add(string userName, bool isActive)
    {
        var user = new UserResponse(Guid.NewGuid(), userName, $"{userName}@example.com", true, null, isActive, null);
        Users[user.Id] = user;
        return user;
    }

    public Task<Result<UserResponse>> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.TryGetValue(userId, out var user)
            ? Result.Success(user)
            : Result.Failure<UserResponse>(UserErrors.NotFound));

    public Task<Result> SetUserEnabledAsync(Guid userId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        Users[userId] = Users[userId] with { IsActive = isEnabled };
        return Task.FromResult(Result.Success());
    }

    public Task<Result> LogoutUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (LogoutSessionsFailure is not null)
        {
            return Task.FromResult(Result.Failure(LogoutSessionsFailure));
        }

        LoggedOutUserIds.Add(userId);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<string>> CreateUserAsync(
        string username, string email, UserAttributes attributes, bool emailVerified, bool isActive,
        string? password = null, CancellationToken cancellationToken = default)
    {
        if (CreateFailure is not null)
        {
            return Task.FromResult(Result.Failure<string>(CreateFailure));
        }

        var user = new UserResponse(
            Guid.NewGuid(), username, email, emailVerified, attributes.PhoneNumber, isActive, null,
            attributes.Country, attributes.TimeZone, attributes.Locale);
        Users[user.Id] = user;
        return Task.FromResult(Result.Success(user.Id.ToString()));
    }

    public Task<Result> UpdateUserAsync(
        string keycloakUserId, string username, string email, UserAttributes attributes, bool emailVerified,
        bool isActive, CancellationToken cancellationToken = default)
    {
        var id = Guid.Parse(keycloakUserId);
        Users[id] = Users[id] with
        {
            UserName = username,
            Email = email,
            PhoneNumber = attributes.PhoneNumber,
            Country = attributes.Country,
            TimeZone = attributes.TimeZone,
            Locale = attributes.Locale,
            EmailVerified = emailVerified,
            IsActive = isActive
        };
        return Task.FromResult(Result.Success());
    }

    public Task<Result> DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Users.Remove(Guid.Parse(keycloakUserId))
            ? Result.Success()
            : Result.Failure(IdentityProviderErrors.UserNotFound));
    }

    public Task<Result> SetUserPasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        if (!Users.ContainsKey(userId))
        {
            return Task.FromResult(Result.Failure(UserErrors.NotFound));
        }

        Passwords[userId] = password;
        return Task.FromResult(Result.Success());
    }

    public Task<Result<IdentityTokenResult>> LoginAsync(
        string username, string password, CancellationToken cancellationToken = default) =>
        Task.FromResult(LoginResult);

    public Task<Result<IdentityTokenResult>> LoginWithGoogleAsync(
        string idToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(GoogleLoginResult);

    public Task<Result<IdentityTokenResult>> RefreshAsync(
        string refreshToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(RefreshResult);

    public Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        LoggedOutRefreshTokens.Add(refreshToken);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<UserResponse>> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = Users.Values.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user is null ? Result.Failure<UserResponse>(UserErrors.NotFound) : Result.Success(user));
    }

    public Task<Result> SendVerificationEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        VerificationEmails.Add(userId);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> SendPasswordResetEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        PasswordResetEmails.Add(userId);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<PagedList<UserResponse>>> SearchUsersAsync(
        string? search, bool? isEnabled, IPagingRequest paging, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result<IReadOnlyList<UserResponse>>> GetRealmRoleMembersAsync(
        string roleName, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result<IReadOnlyList<string>>> GetUserRealmRolesAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<IReadOnlyList<string>>(
            RealmRoles.TryGetValue(userId, out var roles) ? roles : []));

    public Task<Result> AssignRealmRoleAsync(
        Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (AssignRoleFailure is not null)
        {
            return Task.FromResult(Result.Failure(AssignRoleFailure));
        }

        AssignedRoles.Add((userId, roleName));
        RealmRoles[userId] = [.. RealmRoles.GetValueOrDefault(userId) ?? [], roleName];
        return Task.FromResult(Result.Success());
    }
}
