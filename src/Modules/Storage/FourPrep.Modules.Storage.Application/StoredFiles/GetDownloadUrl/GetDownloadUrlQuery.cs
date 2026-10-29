namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public sealed record GetDownloadUrlQuery(Guid FileId) : IQuery<DownloadUrlResponse>;

internal sealed class GetDownloadUrlQueryHandler(
    ICurrentUser currentUser,
    IObjectStorage objectStorage,
    IDateTimeProvider dateTimeProvider,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDownloadUrlQuery, DownloadUrlResponse>
{
    private const string Sql =
        $"""
        SELECT
            f.object_key AS {nameof(DownloadableFile.ObjectKey)},
            f.file_name AS {nameof(DownloadableFile.FileName)},
            f.content_type AS {nameof(DownloadableFile.ContentType)}
        FROM storage.stored_files f
        WHERE f.id = @FileId
          AND f.uploaded_by_user_id = @UserId
          AND f.status = @Uploaded
        """;

    public async Task<Result<DownloadUrlResponse>> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var file = await connection.QuerySingleOrDefaultAsync<DownloadableFile>(
            new CommandDefinition(
                Sql,
                new { request.FileId, currentUser.UserId, Uploaded = (int)StoredFileStatus.Uploaded },
                cancellationToken: cancellationToken));

        if (file is null)
        {
            return Result.Failure<DownloadUrlResponse>(StoredFileErrors.NotFound);
        }

        var expiresOnUtc = dateTimeProvider.UtcNow + StoredFile.DownloadWindow;
        var url = await objectStorage.CreateDownloadUrlAsync(file.ObjectKey, file.FileName, file.ContentType, expiresOnUtc);

        return new DownloadUrlResponse(url, expiresOnUtc);
    }
}
