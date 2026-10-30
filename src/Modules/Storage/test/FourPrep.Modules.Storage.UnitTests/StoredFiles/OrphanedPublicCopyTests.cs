namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using Microsoft.Extensions.Logging.Abstractions;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.StoredFiles.PublishFile;
using FourPrep.Modules.Storage.Application.StoredFiles.RemoveOrphanedPublicCopies;

public class OrphanedPublicCopyTests
{
    private static readonly DateTime Now = new FakeClock().UtcNow;

    [Fact]
    public async Task Remove_DeletesAnOldCopyThatNoFileClaims()
    {
        var (files, storage) = Build();
        storage.AddPublicObject("public/orphan", Now.AddHours(-2));

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(1, result.Data);
        Assert.Empty(storage.PublicObjects);
    }

    [Fact]
    public async Task Remove_KeepsCopiesThatFilesStillUse()
    {
        var (files, storage) = Build();
        storage.CopiedOnUtc = Now.AddDays(-30);
        var file = await PublishedCoverAsync(files, storage);

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(0, result.Data);
        Assert.True(storage.PublicObjects.ContainsKey(file.PublicKey!));
    }

    [Fact]
    public async Task Remove_LeavesAFreshCopyThatAPublishMayStillBeSaving()
    {
        var (files, storage) = Build();
        storage.AddPublicObject("public/just-copied", Now.AddMinutes(-10));

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(0, result.Data);
        Assert.Single(storage.PublicObjects);
    }

    [Fact]
    public async Task Remove_GoesThroughEveryPageOfTheBucket()
    {
        var (files, storage) = Build();
        storage.PublicPageSize = 2;
        storage.CopiedOnUtc = Now.AddDays(-1);
        var file = await PublishedCoverAsync(files, storage);
        for (var i = 0; i < 5; i++)
        {
            storage.AddPublicObject($"public/orphan-{i}", Now.AddDays(-1));
        }

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(5, result.Data);
        Assert.Equal([file.PublicKey!], storage.PublicObjects.Keys);
        Assert.True(storage.PublicListCalls >= 3);
    }

    [Fact]
    public async Task Remove_DoesNothingForAnEmptyBucket()
    {
        var (files, storage) = Build();

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(0, result.Data);
        Assert.Equal(1, storage.PublicListCalls);
    }

    [Fact]
    public async Task Remove_RefusesWhenMostCopiesHaveNoFile()
    {
        var (files, storage) = Build();
        for (var i = 0; i < RemoveOrphanedPublicCopiesCommandHandler.MinOrphansToDoubt; i++)
        {
            storage.AddPublicObject($"public/cover-{i:D2}", Now.AddDays(-10));
        }

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(StoredFileErrors.OrphanCleanupRefused, result.Error);
        Assert.Equal(RemoveOrphanedPublicCopiesCommandHandler.MinOrphansToDoubt, storage.PublicObjects.Count);
    }

    [Fact]
    public async Task Remove_StillRemovesAFewOrphansAmongManyLiveCopies()
    {
        var (files, storage) = Build();
        storage.CopiedOnUtc = Now.AddDays(-1);
        for (var i = 0; i < RemoveOrphanedPublicCopiesCommandHandler.MinOrphansToDoubt; i++)
        {
            await PublishedCoverAsync(files, storage);
            storage.AddPublicObject($"public/orphan-{i:D2}", Now.AddDays(-1));
        }

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(RemoveOrphanedPublicCopiesCommandHandler.MinOrphansToDoubt, result.Data);
        Assert.Equal(RemoveOrphanedPublicCopiesCommandHandler.MinOrphansToDoubt, storage.PublicObjects.Count);
    }

    [Fact]
    public async Task Remove_StopsWhenTheStoreRepeatsAPageToken()
    {
        var (files, storage) = Build();
        storage.PublicListOverride = _ => new PublicObjectPage([new PublicObject("public/orphan", Now.AddDays(-1))], "same-token");

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, storage.PublicListCalls);
    }

    [Fact]
    public async Task Remove_StopsWhenTheStoreGivesAnEmptyPageToken()
    {
        var (files, storage) = Build();
        storage.PublicListOverride = _ => new PublicObjectPage([new PublicObject("public/orphan", Now.AddDays(-1))], "");

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, storage.PublicListCalls);
    }

    [Fact]
    public async Task Remove_GoesOnPastACopyThatCannotBeDeleted()
    {
        var (files, storage) = Build();
        storage.AddPublicObject("public/a-stuck", Now.AddDays(-1));
        storage.AddPublicObject("public/b-orphan", Now.AddDays(-1));
        storage.UndeletablePublicKeys.Add("public/a-stuck");

        var result = await Handler(files, storage).Handle(new RemoveOrphanedPublicCopiesCommand(), default);

        Assert.Equal(1, result.Data);
        Assert.Equal(["public/a-stuck"], storage.PublicObjects.Keys);
    }

    private static (FakeStoredFileRepository Files, FakeObjectStorage Storage) Build() =>
        (new FakeStoredFileRepository(), new FakeObjectStorage());

    private static RemoveOrphanedPublicCopiesCommandHandler Handler(FakeStoredFileRepository files, FakeObjectStorage storage) =>
        new(storage, files, new FakeClock(), NullLogger<RemoveOrphanedPublicCopiesCommandHandler>.Instance);

    private static async Task<StoredFile> PublishedCoverAsync(FakeStoredFileRepository files, FakeObjectStorage storage)
    {
        var file = files.Add(Guid.NewGuid(), "cover.png", "image/png", purpose: StoredFilePurpose.PublicMedia);
        file.CompleteUpload(64, Now.AddDays(-31));
        file.MarkClean(new string('a', 64), Now.AddDays(-31));
        storage.Objects[file.ObjectKey] = FileSamples.Png;

        var published = await new PublishFileCommandHandler(new FakeUnitOfWork(), storage, files, new FakeClock())
            .Handle(new PublishFileCommand(file.Id), default);

        Assert.True(published.IsSuccess);
        return file;
    }
}
