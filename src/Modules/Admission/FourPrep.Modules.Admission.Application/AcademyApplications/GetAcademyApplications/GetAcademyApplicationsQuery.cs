namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;

public sealed record GetAcademyApplicationsQuery(
    PagingRequest<AcademyApplicationSummaryResponse> Paging,
    AcademyApplicationFilter? Filter = null) : IPagedListQuery<AcademyApplicationSummaryResponse>;

internal sealed class GetAcademyApplicationsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetAcademyApplicationsQuery, AcademyApplicationSummaryResponse>
{
    public async Task<PagedList<AcademyApplicationSummaryResponse>> Handle(
        GetAcademyApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return await AcademyApplicationList.ReadPageAsync(
            connection,
            request.Paging,
            request.Filter ?? AcademyApplicationFilter.None,
            cancellationToken);
    }
}
