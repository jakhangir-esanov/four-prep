namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversity;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Application.Facts;
using FourPrep.Modules.Catalogue.Domain.Universities;

public sealed record GetUniversityQuery(string Slug) : IQuery<UniversityResponse>;

internal sealed class GetUniversityQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetUniversityQuery, UniversityResponse>
{
    public async Task<Result<UniversityResponse>> Handle(GetUniversityQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var university = await UniversityReader.ReadBySlugAsync(
            connection,
            request.Slug.Trim().ToLowerInvariant(),
            StaleFacts.CheckedBefore(dateTimeProvider.UtcNow),
            cancellationToken);

        return university is null
            ? Result.Failure<UniversityResponse>(UniversityErrors.NotFound)
            : university;
    }
}
