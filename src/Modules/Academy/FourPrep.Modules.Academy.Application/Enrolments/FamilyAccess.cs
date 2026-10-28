namespace FourPrep.Modules.Academy.Application.Enrolments;

using FourPrep.Common.Domain;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Application.Abstractions.Students;

internal static class FamilyAccess
{
    public static async Task<Result> CheckAsync(
        IStudentService studentService,
        Guid userId,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        var hasAccess = await studentService.HasFamilyAccessAsync(userId, studentUserId, cancellationToken);
        if (hasAccess.IsFailure)
        {
            return Result.Failure(hasAccess.Error);
        }

        return hasAccess.Data ? Result.Success() : Result.Failure(EnrolmentErrors.NotYourStudent);
    }
}
