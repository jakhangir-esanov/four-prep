namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Admission.Domain.AcademyApplications;

public sealed record GetAcademyApplicationQuery(Guid Id) : IQuery<AcademyApplicationResponse>;

internal sealed class GetAcademyApplicationQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetAcademyApplicationQuery, AcademyApplicationResponse>
{
    public async Task<Result<AcademyApplicationResponse>> Handle(
        GetAcademyApplicationQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var application = await AcademyApplicationList.ReadAsync(connection, request.Id, cancellationToken);

        return application is null
            ? Result.Failure<AcademyApplicationResponse>(AcademyApplicationErrors.NotFound)
            : application;
    }
}
