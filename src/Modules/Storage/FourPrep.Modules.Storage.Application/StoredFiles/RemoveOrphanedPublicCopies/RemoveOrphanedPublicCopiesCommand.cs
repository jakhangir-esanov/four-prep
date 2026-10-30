namespace FourPrep.Modules.Storage.Application.StoredFiles.RemoveOrphanedPublicCopies;

using FourPrep.Common.Domain;
using Microsoft.Extensions.Logging;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record RemoveOrphanedPublicCopiesCommand : ICommand<int>;

internal sealed class RemoveOrphanedPublicCopiesCommandHandler(
    IObjectStorage objectStorage,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider,
    ILogger<RemoveOrphanedPublicCopiesCommandHandler> logger) : ICommandHandler<RemoveOrphanedPublicCopiesCommand, int>
{
    internal static readonly TimeSpan KeptUnclaimed = TimeSpan.FromHours(1);

    internal const int MinOrphansToDoubt = 20;

    internal const double MaxOrphanShare = 0.5;

    private const int CheckBatchSize = 1000;

    public async Task<Result<int>> Handle(RemoveOrphanedPublicCopiesCommand request, CancellationToken cancellationToken)
    {
        var cutoffUtc = dateTimeProvider.UtcNow - KeptUnclaimed;
        var listed = 0;
        var orphans = new List<string>();
        string? pageToken = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = await objectStorage.ListPublicAsync(pageToken, cancellationToken);
            listed += page.Objects.Count;

            var candidates = page.Objects
                .Where(publicObject => publicObject.LastModifiedUtc < cutoffUtc)
                .Select(publicObject => publicObject.Key)
                .ToList();
            orphans.AddRange(await UnclaimedAsync(candidates, cancellationToken));

            pageToken = NextPageToken(pageToken, page);
        }
        while (pageToken is not null);

        if (LooksLikeAStorageMismatch(orphans.Count, listed))
        {
            logger.LogError(
                "Refused to remove {Orphans} of {Listed} public copies: the database and the public bucket look out of step",
                orphans.Count,
                listed);

            return Result.Failure<int>(StoredFileErrors.OrphanCleanupRefused);
        }

        return await RemoveAsync(orphans, cancellationToken);
    }

    private static string? NextPageToken(string? currentToken, PublicObjectPage page) =>
        page.Objects.Count == 0 ||
        string.IsNullOrEmpty(page.NextPageToken) ||
        page.NextPageToken == currentToken
            ? null
            : page.NextPageToken;

    private static bool LooksLikeAStorageMismatch(int orphans, int listed) =>
        orphans >= MinOrphansToDoubt && orphans > listed * MaxOrphanShare;

    private async Task<List<string>> UnclaimedAsync(List<string> keys, CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return keys;
        }

        var inUse = await repository.GetPublicKeysInUseAsync(keys, cancellationToken);

        return [.. keys.Where(key => !inUse.Contains(key))];
    }

    private async Task<int> RemoveAsync(List<string> orphans, CancellationToken cancellationToken)
    {
        var removed = 0;

        foreach (var batch in orphans.Chunk(CheckBatchSize))
        {
            foreach (var key in await UnclaimedAsync([.. batch], cancellationToken))
            {
                if (await TryRemoveAsync(key, cancellationToken))
                {
                    removed++;
                }
            }
        }

        return removed;
    }

    private async Task<bool> TryRemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await objectStorage.DeletePublicAsync(key, cancellationToken);
            return true;
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Could not remove orphaned public copy {PublicKey}", key);
            return false;
        }
    }
}
