namespace FourPrep.Modules.Academy.Application.Courses.CreateCourse;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record CreateCourseCommand(CourseDetails Details) : ICommand<Guid>;

internal sealed class CreateCourseCommandHandler(
    IUnitOfWork unitOfWork,
    ICourseRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateCourseCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        if (await repository.IsSlugTakenAsync(request.Details.NormalizedSlug, null, cancellationToken))
        {
            return Result.Failure<Guid>(CourseErrors.SlugTaken);
        }

        var course = request.Details.ToCourse(dateTimeProvider.UtcNow);

        repository.Insert(course);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return course.Id;
    }
}
