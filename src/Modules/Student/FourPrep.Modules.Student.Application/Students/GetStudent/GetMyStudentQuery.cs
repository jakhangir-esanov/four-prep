namespace FourPrep.Modules.Student.Application.Students.GetStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record GetMyStudentQuery : IQuery<StudentResponse>;

internal sealed class GetMyStudentQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetMyStudentQuery, StudentResponse>
{
    public async Task<Result<StudentResponse>> Handle(GetMyStudentQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var student = await StudentReader.ReadByUserIdAsync(connection, currentUser.UserId, cancellationToken);

        return student is null
            ? Result.Failure<StudentResponse>(StudentErrors.NotFound)
            : student;
    }
}
