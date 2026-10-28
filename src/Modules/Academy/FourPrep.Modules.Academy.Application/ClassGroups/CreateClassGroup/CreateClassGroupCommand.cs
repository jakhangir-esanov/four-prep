namespace FourPrep.Modules.Academy.Application.ClassGroups.CreateClassGroup;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record CreateClassGroupCommand(Guid CourseId, ClassGroupDetails Details) : ICommand<Guid>;

internal sealed class CreateClassGroupCommandHandler(
    IUnitOfWork unitOfWork,
    ICourseRepository courseRepository,
    IClassGroupRepository classGroupRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateClassGroupCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateClassGroupCommand request, CancellationToken cancellationToken)
    {
        if (!await courseRepository.ExistsAsync(request.CourseId, cancellationToken))
        {
            return Result.Failure<Guid>(CourseErrors.NotFound);
        }

        if (await classGroupRepository.IsCodeTakenAsync(request.Details.NormalizedCode, null, cancellationToken))
        {
            return Result.Failure<Guid>(ClassGroupErrors.CodeTaken);
        }

        var classGroup = request.Details.ToClassGroup(request.CourseId, dateTimeProvider.UtcNow);

        classGroupRepository.Insert(classGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return classGroup.Id;
    }
}
