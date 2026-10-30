namespace FourPrep.Modules.Content.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;
using FourPrep.Modules.Content.Application.Abstractions.Storage;
using FourPrep.Modules.Content.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 10, 30, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Action? BeforeSave { get; set; }

    public Exception? SaveException { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        BeforeSave?.Invoke();

        if (SaveException is not null)
        {
            throw SaveException;
        }

        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeNewsPostRepository : INewsPostRepository
{
    public List<NewsPost> NewsPosts { get; } = [];

    public NewsPost Add(string slug = "first-news")
    {
        var newsPost = NewsPostSamples.Details() with { Slug = slug };
        var created = newsPost.ToNewsPost(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        NewsPosts.Add(created);
        return created;
    }

    public Task<NewsPost?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(NewsPosts.SingleOrDefault(x => x.Id == id));

    public Task<bool> IsSlugTakenAsync(string slug, Guid? exceptNewsPostId, CancellationToken cancellationToken = default) =>
        Task.FromResult(NewsPosts.Any(x => x.Slug == slug && x.Id != exceptNewsPostId));

    public Exception? CoverInUseException { get; set; }

    public Task<bool> IsCoverInUseAsync(Guid fileId, Guid exceptNewsPostId, CancellationToken cancellationToken = default)
    {
        if (CoverInUseException is not null)
        {
            throw CoverInUseException;
        }

        return Task.FromResult(NewsPosts.Any(x => x.CoverFileId == fileId && x.Id != exceptNewsPostId));
    }

    public void Insert(NewsPost newsPost) => NewsPosts.Add(newsPost);

    public void Remove(NewsPost newsPost) => NewsPosts.Remove(newsPost);
}

internal sealed class FakeStorageService : IStorageService
{
    public Dictionary<Guid, StorageFile> Files { get; } = [];

    public HashSet<Guid> Published { get; } = [];

    public List<Guid> Unpublished { get; } = [];

    public Error? PublishFailure { get; set; }

    public StorageFile AddFile(Guid uploadedByUserId, string contentType = "image/png")
    {
        var file = new StorageFile(Guid.NewGuid(), contentType, uploadedByUserId);
        Files[file.Id] = file;
        return file;
    }

    public static string PublicUrl(Guid fileId) => $"https://cdn.test/public/{fileId:N}";

    public Task<Result<StorageFile>> GetFileAsync(Guid fileId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Files.TryGetValue(fileId, out var file)
            ? Result.Success(file)
            : Result.Failure<StorageFile>(Error.NotFound("StoredFile.NotFound", "Not found.", "Topilmadi.", "Не найден.")));

    public Task<Result<string>> PublishFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        if (PublishFailure is not null)
        {
            return Task.FromResult(Result.Failure<string>(PublishFailure));
        }

        Published.Add(fileId);
        return Task.FromResult(Result.Success(PublicUrl(fileId)));
    }

    public Exception? UnpublishException { get; set; }

    public Task<Result> UnpublishFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        if (UnpublishException is not null)
        {
            throw UnpublishException;
        }

        Published.Remove(fileId);
        Unpublished.Add(fileId);
        return Task.FromResult(Result.Success());
    }
}

internal static class NewsPostSamples
{
    public static NewsPostDetails Details() =>
        new(
            " ielts-results-2026 ",
            new Dictionary<string, string> { ["uz"] = " IELTS natijalari ", ["en"] = "IELTS results" },
            new Dictionary<string, string> { ["uz"] = "**12 ta** o'quvchi 7.0 dan oshdi.", ["en"] = "**12** students scored above 7.0." },
            " admissions ");
}
