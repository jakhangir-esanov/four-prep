namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversities;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Application.Facts;

public sealed record GetUniversitiesQuery(
    PagingRequest<UniversitySummaryResponse> Paging,
    UniversityFilter? Filter = null) : IPagedListQuery<UniversitySummaryResponse>;

internal sealed class GetUniversitiesQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider)
    : IPagedListQueryHandler<GetUniversitiesQuery, UniversitySummaryResponse>
{
    public async Task<PagedList<UniversitySummaryResponse>> Handle(
        GetUniversitiesQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return await UniversityList.ReadAsync(
            connection,
            request.Paging,
            request.Filter ?? UniversityFilter.None,
            StaleFacts.CheckedBefore(dateTimeProvider.UtcNow),
            cancellationToken);
    }
}
