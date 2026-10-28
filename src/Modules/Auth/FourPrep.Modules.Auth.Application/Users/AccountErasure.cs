namespace FourPrep.Modules.Auth.Application.Users;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

internal static class AccountErasure
{
    public static async Task<Result> EraseAsync(
        IIdentityProviderClient identityProviderClient,
        IStudentService studentService,
        IUserSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var dataErased = await studentService.EraseUserDataAsync(userId, cancellationToken);
        if (dataErased.IsFailure)
        {
            return dataErased;
        }

        var deleted = await identityProviderClient.DeleteUserAsync(userId.ToString(), cancellationToken);
        if (deleted.IsFailure && deleted.Error != IdentityProviderErrors.UserNotFound)
        {
            return deleted;
        }

        await sessionRepository.RemoveAllByUserIdAsync(userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
