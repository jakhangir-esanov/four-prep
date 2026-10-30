namespace FourPrep.Modules.Storage.Application.StoredFiles.GetStoredFile;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;

public sealed record GetStoredFileQuery(Guid FileId) : IQuery<StoredFileResponse>;

internal sealed class GetStoredFileQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetStoredFileQuery, StoredFileResponse>
{
    private const string Sql =
        $"""
        SELECT
            f.id AS {nameof(StoredFileResponse.Id)},
            f.file_name AS {nameof(StoredFileResponse.FileName)},
            f.content_type AS {nameof(StoredFileResponse.ContentType)},
            f.size_bytes AS {nameof(StoredFileResponse.SizeBytes)},
            f.purpose AS {nameof(StoredFileResponse.Purpose)},
            f.uploaded_by_user_id AS {nameof(StoredFileResponse.UploadedByUserId)},
            f.status AS {nameof(StoredFileResponse.Status)},
            f.scan_status AS {nameof(StoredFileResponse.ScanStatus)},
            f.sha256 AS {nameof(StoredFileResponse.Sha256)},
            f.uploaded_on_utc AS {nameof(StoredFileResponse.UploadedOnUtc)},
            f.deleted_on_utc AS {nameof(StoredFileResponse.DeletedOnUtc)}
        FROM storage.stored_files f
        WHERE f.id = @FileId
        """;

    public async Task<Result<StoredFileResponse>> Handle(GetStoredFileQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var file = await connection.QuerySingleOrDefaultAsync<StoredFileResponse>(
            new CommandDefinition(Sql, new { request.FileId }, cancellationToken: cancellationToken));

        return file is null
            ? Result.Failure<StoredFileResponse>(StoredFileErrors.NotFound)
            : file;
    }
}
