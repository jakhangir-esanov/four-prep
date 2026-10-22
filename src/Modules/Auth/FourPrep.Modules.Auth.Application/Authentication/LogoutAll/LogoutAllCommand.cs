namespace FourPrep.Modules.Auth.Application.Authentication.LogoutAll;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record LogoutAllCommand : ICommand;

internal sealed class LogoutAllCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository) : ICommandHandler<LogoutAllCommand>
{
    public async Task<Result> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
    {
        await sessionRepository.RevokeAllByUserIdAsync(
            currentUser.UserId,
            dateTimeProvider.UtcNow,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
