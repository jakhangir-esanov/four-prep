namespace FourPrep.Modules.Auth.Application.Account.DeleteMyAccount;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record DeleteMyAccountCommand(string? Password) : ICommand;

internal sealed class DeleteMyAccountCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IStudentService studentService,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<DeleteMyAccountCommand>
{
    private static readonly TimeSpan RecentSignInWindow = TimeSpan.FromMinutes(10);

    public async Task<Result> Handle(DeleteMyAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.GetUserAsync(currentUser.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        var confirmed = await ConfirmAsync(user.Data.UserName, request.Password, cancellationToken);
        if (confirmed.IsFailure)
        {
            return confirmed;
        }

        return await AccountErasure.EraseAsync(
            identityProviderClient,
            studentService,
            sessionRepository,
            unitOfWork,
            currentUser.UserId,
            cancellationToken);
    }

    private async Task<Result> ConfirmAsync(string userName, string? password, CancellationToken cancellationToken)
    {
        var hasPassword = await identityProviderClient.HasPasswordAsync(currentUser.UserId, cancellationToken);
        if (hasPassword.IsFailure)
        {
            return Result.Failure(hasPassword.Error);
        }

        if (hasPassword.Data)
        {
            return await PasswordCheck.VerifyAsync(identityProviderClient, userName, password, cancellationToken);
        }

        return await IsSignedInRecentlyAsync(cancellationToken)
            ? Result.Success()
            : Result.Failure(UserErrors.ReauthenticationRequired);
    }

    private async Task<bool> IsSignedInRecentlyAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(currentUser.SessionId))
        {
            return false;
        }

        var session = await sessionRepository.GetByIdentitySessionIdAsync(currentUser.SessionId, cancellationToken);

        return session is { IsRevoked: false } &&
               session.UserId == currentUser.UserId &&
               session.CreatedOnUtc >= dateTimeProvider.UtcNow - RecentSignInWindow;
    }
}
