namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Students;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public sealed record GetStudentAcademyApplicationsQuery(Guid StudentUserId)
    : IQuery<IReadOnlyList<AcademyApplicationResponse>>;

internal sealed class GetStudentAcademyApplicationsQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetStudentAcademyApplicationsQuery, IReadOnlyList<AcademyApplicationResponse>>
{
    public async Task<Result<IReadOnlyList<AcademyApplicationResponse>>> Handle(
        GetStudentAcademyApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await studentService.HasFamilyAccessAsync(currentUser.UserId, request.StudentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<IReadOnlyList<AcademyApplicationResponse>>(access.Error);
        }

        if (!access.Data)
        {
            return Result.Failure<IReadOnlyList<AcademyApplicationResponse>>(AcademyApplicationErrors.NotYourStudent);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var applications = await AcademyApplicationList.ReadForStudentAsync(connection, request.StudentUserId, cancellationToken);

        return Result.Success<IReadOnlyList<AcademyApplicationResponse>>(
            [.. applications.Select(application => application.ForFamily())]);
    }
}
