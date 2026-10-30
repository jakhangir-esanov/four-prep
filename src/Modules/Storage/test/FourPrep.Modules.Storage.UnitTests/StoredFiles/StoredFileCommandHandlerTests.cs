namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.StoredFiles.CompleteUpload;
using FourPrep.Modules.Storage.Application.StoredFiles.CreateUploadUrl;
using FourPrep.Modules.Storage.Application.StoredFiles.RemoveAbandonedUploads;

public class StoredFileCommandHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public async Task CreateUploadUrl_RecordsAPendingFileUnderRandomKeys()
    {
        var (files, storage, unitOfWork) = Build();

        var result = await CreateHandler(files, storage, unitOfWork)
            .Handle(new CreateUploadUrlCommand(" scans/Passport.PDF ", " Application/PDF ", 64), default);

        Assert.True(result.IsSuccess);
        var file = Assert.Single(files.Files);
        Assert.Equal(result.Data.FileId, file.Id);
        Assert.Equal("Passport.PDF", file.FileName);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal(StoredFileStatus.Pending, file.Status);
        Assert.Equal(OwnerId, file.UploadedByUserId);
        Assert.Equal(StoredFilePurpose.Document, file.Purpose);
        Assert.StartsWith("uploads/", file.UploadKey);
        Assert.StartsWith("docs/", file.ObjectKey);
        Assert.DoesNotContain("Passport", file.UploadKey + file.ObjectKey, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("PUT", result.Data.Method);
        Assert.Equal("application/pdf", result.Data.Headers["Content-Type"]);
        Assert.Equal(new FakeClock().UtcNow + StoredFile.UploadWindow, result.Data.ExpiresOnUtc);
        Assert.Contains(file.UploadKey, result.Data.UploadUrl);
        Assert.DoesNotContain(file.ObjectKey, result.Data.UploadUrl);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateUploadUrl_RefusesMoreThanTenUnfinishedUploads()
    {
        var (files, storage, unitOfWork) = Build();
        for (var i = 0; i < StoredFile.MaxPendingPerUser; i++)
        {
            files.Add(OwnerId);
        }

        files.Add(Guid.NewGuid());

        var result = await CreateHandler(files, storage, unitOfWork)
            .Handle(new CreateUploadUrlCommand("a.pdf", "application/pdf", 64), default);

        Assert.Equal(StoredFileErrors.TooManyPending, result.Error);
        Assert.Equal(StoredFile.MaxPendingPerUser + 1, files.Files.Count);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateUploadUrl_DoesNotCountFinishedUploadsAgainstTheLimit()
    {
        var (files, storage, unitOfWork) = Build();
        for (var i = 0; i < StoredFile.MaxPendingPerUser; i++)
        {
            files.Add(OwnerId).CompleteUpload(64, new FakeClock().UtcNow);
        }

        var result = await CreateHandler(files, storage, unitOfWork)
            .Handle(new CreateUploadUrlCommand("a.pdf", "application/pdf", 64), default);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("application/pdf", "a.pdf")]
    [InlineData("image/png", "a.png")]
    [InlineData("image/jpeg", "a.jpg")]
    [InlineData("image/heic", "a.heic")]
    public async Task Complete_MovesACheckedFileToAKeyNobodyCanUploadTo(string contentType, string fileName)
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId, fileName, contentType);
        storage.Objects[file.UploadKey] = Sample(contentType);

        var result = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(StoredFileStatus.Uploaded, file.Status);
        Assert.Equal(new FakeClock().UtcNow, file.UploadedOnUtc);
        Assert.Equal(Sample(contentType), storage.Objects[file.ObjectKey]);
        Assert.False(storage.Objects.ContainsKey(file.UploadKey));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_HidesAnotherUsersFile()
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId);
        storage.Objects[file.UploadKey] = FileSamples.Pdf;

        var result = await CompleteHandler(files, storage, unitOfWork, Guid.NewGuid()).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
        Assert.Equal(StoredFileStatus.Pending, file.Status);
        Assert.True(storage.Objects.ContainsKey(file.UploadKey));
        Assert.Empty(storage.Deleted);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_ReportsAnUnknownOrDeletedFile()
    {
        var (files, storage, unitOfWork) = Build();
        var deleted = files.Add(OwnerId);
        deleted.Delete(new FakeClock().UtcNow);

        var unknown = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(Guid.NewGuid()), default);
        var gone = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(deleted.Id), default);

        Assert.Equal(StoredFileErrors.NotFound, unknown.Error);
        Assert.Equal(StoredFileErrors.NotFound, gone.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_WaitsUntilTheBytesReachStorage()
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId);

        var result = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.NotUploaded, result.Error);
        Assert.Equal(StoredFileStatus.Pending, file.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_RefusesASecondConfirmation()
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId);
        storage.Objects[file.UploadKey] = FileSamples.Pdf;
        await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);
        storage.Objects[file.UploadKey] = FileSamples.Png;

        var result = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.AlreadyUploaded, result.Error);
        Assert.Equal(FileSamples.Pdf, storage.Objects[file.ObjectKey]);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_DeletesBytesThatDoNotMatchTheType()
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId, "photo.png", "image/png");
        storage.Objects[file.UploadKey] = FileSamples.Pdf;

        var result = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.ContentMismatch, result.Error);
        Assert.Equal([file.UploadKey], storage.Deleted);
        Assert.False(storage.Objects.ContainsKey(file.ObjectKey));
        Assert.Equal(StoredFileStatus.Pending, file.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_DeletesAFileOfAnotherSizeWithoutReadingIt()
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId, sizeBytes: FileSamples.Pdf.Length + 1);
        storage.Objects[file.UploadKey] = FileSamples.Pdf;

        var result = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.SizeMismatch, result.Error);
        Assert.Equal([file.UploadKey], storage.Deleted);
        Assert.Equal(0, storage.HeaderReads);
        Assert.Equal(StoredFileStatus.Pending, file.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_TreatsAnEmptyUploadAsTheWrongSize()
    {
        var (files, storage, unitOfWork) = Build();
        var file = files.Add(OwnerId);
        storage.Objects[file.UploadKey] = [];

        var result = await CompleteHandler(files, storage, unitOfWork, OwnerId).Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.SizeMismatch, result.Error);
        Assert.Equal(0, storage.HeaderReads);
        Assert.Equal([file.UploadKey], storage.Deleted);
    }

    [Fact]
    public async Task RemoveAbandonedUploads_DeletesBothKeysOfFilesUnconfirmedForAnHour()
    {
        var (files, storage, unitOfWork) = Build();
        var old = StoredFile.Create("old.pdf", "application/pdf", 64, StoredFilePurpose.Document, OwnerId, new FakeClock().UtcNow.AddHours(-2));
        var recent = StoredFile.Create("new.pdf", "application/pdf", 64, StoredFilePurpose.Document, OwnerId, new FakeClock().UtcNow.AddMinutes(-30));
        var confirmed = StoredFile.Create("done.pdf", "application/pdf", 64, StoredFilePurpose.Document, OwnerId, new FakeClock().UtcNow.AddDays(-2));
        confirmed.CompleteUpload(64, new FakeClock().UtcNow.AddDays(-2));
        files.Files.AddRange([old, recent, confirmed]);
        storage.Objects[old.UploadKey] = FileSamples.Pdf;

        var result = await new RemoveAbandonedUploadsCommandHandler(unitOfWork, storage, files, new FakeClock())
            .Handle(new RemoveAbandonedUploadsCommand(), default);

        Assert.Equal(1, result.Data);
        Assert.Equal(StoredFileStatus.Deleted, old.Status);
        Assert.Equal(new FakeClock().UtcNow, old.DeletedOnUtc);
        Assert.Equal([old.UploadKey, old.ObjectKey], storage.Deleted);
        Assert.Equal(StoredFileStatus.Pending, recent.Status);
        Assert.Equal(StoredFileStatus.Uploaded, confirmed.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task RemoveAbandonedUploads_TakesOneBatchPerRun()
    {
        var (files, storage, unitOfWork) = Build();
        for (var i = 0; i < RemoveAbandonedUploadsCommandHandler.BatchSize + 5; i++)
        {
            files.Files.Add(StoredFile.Create("old.pdf", "application/pdf", 64, StoredFilePurpose.Document, OwnerId, new FakeClock().UtcNow.AddHours(-2)));
        }

        var result = await new RemoveAbandonedUploadsCommandHandler(unitOfWork, storage, files, new FakeClock())
            .Handle(new RemoveAbandonedUploadsCommand(), default);

        Assert.Equal(RemoveAbandonedUploadsCommandHandler.BatchSize, result.Data);
        Assert.Equal(5, files.Files.Count(x => x.Status == StoredFileStatus.Pending));
    }

    [Fact]
    public async Task RemoveAbandonedUploads_SavesNothingWhenThereIsNothingToRemove()
    {
        var (files, storage, unitOfWork) = Build();

        var result = await new RemoveAbandonedUploadsCommandHandler(unitOfWork, storage, files, new FakeClock())
            .Handle(new RemoveAbandonedUploadsCommand(), default);

        Assert.Equal(0, result.Data);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static byte[] Sample(string contentType) => contentType switch
    {
        "image/png" => FileSamples.Png,
        "image/jpeg" => FileSamples.Jpeg,
        "image/heic" => FileSamples.Heic,
        _ => FileSamples.Pdf
    };

    private static (FakeStoredFileRepository, FakeObjectStorage, FakeUnitOfWork) Build() =>
        (new FakeStoredFileRepository(), new FakeObjectStorage(), new FakeUnitOfWork());

    private static CreateUploadUrlCommandHandler CreateHandler(
        FakeStoredFileRepository files,
        FakeObjectStorage storage,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(OwnerId), storage, files, new FakeClock());

    private static CompleteUploadCommandHandler CompleteHandler(
        FakeStoredFileRepository files,
        FakeObjectStorage storage,
        FakeUnitOfWork unitOfWork,
        Guid currentUserId) =>
        new(unitOfWork, new FakeCurrentUser(currentUserId), storage, new FakeVirusScanQueue(), files, new FakeClock());
}
