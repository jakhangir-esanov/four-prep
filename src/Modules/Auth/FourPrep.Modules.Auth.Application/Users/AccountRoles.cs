namespace FourPrep.Modules.Auth.Application.Users;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

internal static class AccountRoles
{
    public static async Task<Result> GrantAsync(
        IIdentityProviderClient identityProviderClient,
        IStudentService studentService,
        string keycloakUserId,
        string username,
        string? country,
        string role,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(keycloakUserId);

        var roleAssigned = await identityProviderClient.AssignRealmRoleAsync(userId, role, cancellationToken);
        if (roleAssigned.IsFailure)
        {
            await identityProviderClient.DeleteUserAsync(keycloakUserId, cancellationToken);
            return roleAssigned;
        }

        if (role != Roles.Student)
        {
            return Result.Success();
        }

        string keycloakUsername = username.ToLowerInvariant();
        var studentCreated = await studentService.CreateStudentAsync(
            userId,
            keycloakUsername,
            country,
            cancellationToken);

        if (studentCreated.IsFailure)
        {
            await identityProviderClient.DeleteUserAsync(keycloakUserId, cancellationToken);
            return Result.Failure(studentCreated.Error);
        }

        return Result.Success();
    }
}
