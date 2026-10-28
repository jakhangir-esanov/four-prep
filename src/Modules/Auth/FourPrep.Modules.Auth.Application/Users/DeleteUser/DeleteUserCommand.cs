namespace FourPrep.Modules.Auth.Application.Users.DeleteUser;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;

internal sealed class DeleteUserCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == currentUser.UserId)
        {
            return Result.Failure(UserErrors.CannotDeleteSelf);
        }

        return await AccountErasure.EraseAsync(
            identityProviderClient,
            studentService,
            sessionRepository,
            unitOfWork,
            request.UserId,
            cancellationToken);
    }
}
