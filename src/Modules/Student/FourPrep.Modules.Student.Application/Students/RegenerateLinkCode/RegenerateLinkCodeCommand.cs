namespace FourPrep.Modules.Student.Application.Students.RegenerateLinkCode;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record RegenerateLinkCodeCommand : ICommand<string>;

internal sealed class RegenerateLinkCodeCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentRepository repository,
    ILinkCodeGenerator linkCodeGenerator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RegenerateLinkCodeCommand, string>
{
    public async Task<Result<string>> Handle(RegenerateLinkCodeCommand request, CancellationToken cancellationToken)
    {
        var student = await repository.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        if (student is null)
        {
            return Result.Failure<string>(StudentErrors.NotFound);
        }

        var linkCode = await UniqueLinkCodes.NextAsync(linkCodeGenerator, repository, cancellationToken);
        if (linkCode.IsFailure)
        {
            return linkCode;
        }

        student.ChangeLinkCode(linkCode.Data, dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return linkCode.Data;
    }
}
