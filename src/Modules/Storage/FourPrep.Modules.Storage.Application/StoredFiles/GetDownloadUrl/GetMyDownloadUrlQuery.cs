namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public sealed record GetMyDownloadUrlQuery(Guid FileId) : IQuery<DownloadUrlResponse>;

internal sealed class GetMyDownloadUrlQueryHandler(
    ICurrentUser currentUser,
    IObjectStorage objectStorage,
    IDateTimeProvider dateTimeProvider,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetMyDownloadUrlQuery, DownloadUrlResponse>
{
    public async Task<Result<DownloadUrlResponse>> Handle(GetMyDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var file = await FileDownloads.ReadAsync(connection, request.FileId, cancellationToken);

        return await FileDownloads.CreateUrlAsync(
            file?.UploadedByUserId == currentUser.UserId ? file : null,
            objectStorage,
            dateTimeProvider.UtcNow);
    }
}
