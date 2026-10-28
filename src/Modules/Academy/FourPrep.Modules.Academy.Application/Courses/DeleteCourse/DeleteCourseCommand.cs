namespace FourPrep.Modules.Academy.Application.Courses.DeleteCourse;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record DeleteCourseCommand(Guid Id) : ICommand;

internal sealed class DeleteCourseCommandHandler(
    IUnitOfWork unitOfWork,
    ICourseRepository courseRepository,
    IClassGroupRepository classGroupRepository) : ICommandHandler<DeleteCourseCommand>
{
    public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetAsync(request.Id, cancellationToken);
        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (await classGroupRepository.ExistsForCourseAsync(course.Id, cancellationToken))
        {
            return Result.Failure(CourseErrors.HasClassGroups);
        }

        courseRepository.Remove(course);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
