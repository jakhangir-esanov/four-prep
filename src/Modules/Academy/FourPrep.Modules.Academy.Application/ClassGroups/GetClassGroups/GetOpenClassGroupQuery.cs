namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;

public sealed record GetOpenClassGroupQuery(Guid Id) : IQuery<ClassGroupResponse>;

internal sealed class GetOpenClassGroupQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetOpenClassGroupQuery, ClassGroupResponse>
{
    public async Task<Result<ClassGroupResponse>> Handle(
        GetOpenClassGroupQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var classGroup = await ClassGroupList.ReadAsync(
            connection,
            request.Id,
            ClassGroupStatus.Open,
            cancellationToken);

        return classGroup is null
            ? Result.Failure<ClassGroupResponse>(ClassGroupErrors.NotFound)
            : classGroup.ForPublic();
    }
}
