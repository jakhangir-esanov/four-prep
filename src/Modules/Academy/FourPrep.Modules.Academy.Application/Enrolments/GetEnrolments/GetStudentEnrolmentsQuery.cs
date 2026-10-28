namespace FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Application.Abstractions.Students;
using FourPrep.Modules.Academy.Application.Abstractions.Authentication;

public sealed record GetStudentEnrolmentsQuery(Guid StudentUserId) : IQuery<IReadOnlyList<EnrolmentResponse>>;

internal sealed class GetStudentEnrolmentsQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetStudentEnrolmentsQuery, IReadOnlyList<EnrolmentResponse>>
{
    public async Task<Result<IReadOnlyList<EnrolmentResponse>>> Handle(
        GetStudentEnrolmentsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await FamilyAccess.CheckAsync(studentService, currentUser.UserId, request.StudentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<IReadOnlyList<EnrolmentResponse>>(access.Error);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var enrolments = await EnrolmentReader.ReadForStudentAsync(connection, request.StudentUserId, cancellationToken);

        return Result.Success<IReadOnlyList<EnrolmentResponse>>(
            [.. enrolments.Select(enrolment => enrolment.ForFamily(currentUser.UserId))]);
    }
}
