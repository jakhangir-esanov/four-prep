namespace FourPrep.Modules.Auth.Application.Account.ChangeMyPassword;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Users;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword) : ICommand<AuthTokenResponse>;

internal sealed class ChangeMyPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<ChangeMyPasswordCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.GetUserAsync(currentUser.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(user.Error);
        }

        var hasPassword = await identityProviderClient.HasPasswordAsync(currentUser.UserId, cancellationToken);
        if (hasPassword.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(hasPassword.Error);
        }

        if (!hasPassword.Data)
        {
            return Result.Failure<AuthTokenResponse>(UserErrors.PasswordNotSet);
        }

        var verified = await PasswordCheck.VerifyAsync(
            identityProviderClient, user.Data.UserName, request.CurrentPassword, cancellationToken);
        if (verified.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(verified.Error);
        }

        if (string.Equals(request.NewPassword, user.Data.UserName, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AuthTokenResponse>(UserErrors.PasswordNotAllowed);
        }

        var changed = await identityProviderClient.SetUserPasswordAsync(currentUser.UserId, request.NewPassword, cancellationToken);
        if (changed.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(changed.Error);
        }

        var loggedOut = await identityProviderClient.LogoutUserSessionsAsync(currentUser.UserId, cancellationToken);
        if (loggedOut.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(loggedOut.Error);
        }

        await sessionRepository.RevokeAllByUserIdAsync(currentUser.UserId, dateTimeProvider.UtcNow, cancellationToken);

        var login = await identityProviderClient.LoginAsync(user.Data.UserName, request.NewPassword, cancellationToken);
        if (login.IsFailure)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthTokenResponse>(login.Error);
        }

        sessionRepository.Insert(UserSession.Create(
            login.Data.UserId,
            login.Data.SessionId,
            AccessTokenHash.Of(login.Data.AccessToken),
            login.Data.ExpiresAt,
            dateTimeProvider.UtcNow));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthTokenResponse.From(login.Data);
    }
}
