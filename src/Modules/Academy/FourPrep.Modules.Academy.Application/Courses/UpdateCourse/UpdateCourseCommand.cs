namespace FourPrep.Modules.Academy.Application.Courses.UpdateCourse;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record UpdateCourseCommand(Guid Id, CourseDetails Details) : ICommand;

internal sealed class UpdateCourseCommandHandler(
    IUnitOfWork unitOfWork,
    ICourseRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateCourseCommand>
{
    public async Task<Result> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await repository.GetAsync(request.Id, cancellationToken);
        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (await repository.IsSlugTakenAsync(request.Details.NormalizedSlug, course.Id, cancellationToken))
        {
            return Result.Failure(CourseErrors.SlugTaken);
        }

        request.Details.ApplyTo(course, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
