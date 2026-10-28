namespace FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;

public sealed record GetClassGroupEnrolmentsQuery(Guid ClassGroupId) : IQuery<IReadOnlyList<EnrolmentResponse>>;

internal sealed class GetClassGroupEnrolmentsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetClassGroupEnrolmentsQuery, IReadOnlyList<EnrolmentResponse>>
{
    public async Task<Result<IReadOnlyList<EnrolmentResponse>>> Handle(
        GetClassGroupEnrolmentsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return Result.Success(await EnrolmentReader.ReadForClassGroupAsync(connection, request.ClassGroupId, cancellationToken));
    }
}
