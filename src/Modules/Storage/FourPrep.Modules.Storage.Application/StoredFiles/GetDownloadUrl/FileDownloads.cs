namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

internal static class FileDownloads
{
    private const string Sql =
        $"""
        SELECT
            f.object_key AS {nameof(DownloadableFile.ObjectKey)},
            f.file_name AS {nameof(DownloadableFile.FileName)},
            f.content_type AS {nameof(DownloadableFile.ContentType)},
            f.uploaded_by_user_id AS {nameof(DownloadableFile.UploadedByUserId)},
            f.status AS {nameof(DownloadableFile.Status)},
            f.scan_status AS {nameof(DownloadableFile.ScanStatus)}
        FROM storage.stored_files f
        WHERE f.id = @FileId
        """;

    public static async Task<DownloadableFile?> ReadAsync(
        DbConnection connection,
        Guid fileId,
        CancellationToken cancellationToken) =>
        await connection.QuerySingleOrDefaultAsync<DownloadableFile>(
            new CommandDefinition(Sql, new { FileId = fileId }, cancellationToken: cancellationToken));

    public static async Task<Result<DownloadUrlResponse>> CreateUrlAsync(
        DownloadableFile? file,
        IObjectStorage objectStorage,
        DateTime utcNow)
    {
        if (file is null || file.Status != StoredFileStatus.Uploaded)
        {
            return Result.Failure<DownloadUrlResponse>(StoredFileErrors.NotFound);
        }

        switch (file.ScanStatus)
        {
            case ScanStatus.Failed:
                return Result.Failure<DownloadUrlResponse>(StoredFileErrors.ScanFailed);
            case not ScanStatus.Clean:
                return Result.Failure<DownloadUrlResponse>(StoredFileErrors.NotScanned);
        }

        var expiresOnUtc = utcNow + StoredFile.DownloadWindow;
        var url = await objectStorage.CreateDownloadUrlAsync(file.ObjectKey, file.FileName, file.ContentType, expiresOnUtc);

        return new DownloadUrlResponse(url, expiresOnUtc);
    }
}
