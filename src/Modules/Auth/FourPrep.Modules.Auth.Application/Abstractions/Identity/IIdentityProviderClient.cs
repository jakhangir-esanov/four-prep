namespace FourPrep.Modules.Auth.Application.Abstractions.Identity;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Paging;
using FourPrep.Modules.Auth.Application.Users.GetUser;

public interface IIdentityProviderClient
{
    Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        UserAttributes attributes,
        bool emailVerified,
        bool isActive,
        string? password = null,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateUserAsync(
        string keycloakUserId,
        string username,
        string email,
        UserAttributes attributes,
        bool emailVerified,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityTokenResult>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityTokenResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Result<PagedList<UserResponse>>> SearchUsersAsync(
        string? search,
        bool? isEnabled,
        IPagingRequest paging,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<UserResponse>>> GetRealmRoleMembersAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>>> GetUserRealmRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> AssignRealmRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<Result> SetUserEnabledAsync(
        Guid userId,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> SetUserPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default);

    // Keycloak sends these emails in the user's locale, with a link that expires.
    Task<Result> SendVerificationEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> SendPasswordResetEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
