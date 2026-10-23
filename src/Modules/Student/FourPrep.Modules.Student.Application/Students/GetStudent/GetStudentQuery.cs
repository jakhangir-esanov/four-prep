namespace FourPrep.Modules.Student.Application.Students.GetStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;

public sealed record GetStudentQuery(Guid Id) : IQuery<StudentResponse>;

internal sealed class GetStudentQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetStudentQuery, StudentResponse>
{
    public async Task<Result<StudentResponse>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var student = await StudentReader.ReadByIdAsync(connection, request.Id, cancellationToken);

        return student is null
            ? Result.Failure<StudentResponse>(StudentErrors.NotFound)
            : student;
    }
}
