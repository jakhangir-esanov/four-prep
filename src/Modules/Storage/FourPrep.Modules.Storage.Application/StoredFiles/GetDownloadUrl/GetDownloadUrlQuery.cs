namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record GetDownloadUrlQuery(Guid FileId) : IQuery<DownloadUrlResponse>;

internal sealed class GetDownloadUrlQueryHandler(
    IObjectStorage objectStorage,
    IDateTimeProvider dateTimeProvider,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDownloadUrlQuery, DownloadUrlResponse>
{
    public async Task<Result<DownloadUrlResponse>> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var file = await FileDownloads.ReadAsync(connection, request.FileId, cancellationToken);

        return await FileDownloads.CreateUrlAsync(file, objectStorage, dateTimeProvider.UtcNow);
    }
}
