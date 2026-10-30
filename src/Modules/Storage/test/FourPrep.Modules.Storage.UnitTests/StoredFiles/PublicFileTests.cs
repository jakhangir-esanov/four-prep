namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.StoredFiles.DeleteFile;
using FourPrep.Modules.Storage.Application.StoredFiles.PublishFile;
using FourPrep.Modules.Storage.Application.StoredFiles.UnpublishFile;

public class PublicFileTests
{
    private static readonly DateTime Now = new FakeClock().UtcNow;

    [Fact]
    public void Publish_GivesACleanFileARandomPublicKey()
    {
        var file = CleanFile();

        var result = file.Publish(Now);

        Assert.True(result.IsSuccess);
        Assert.StartsWith("public/", file.PublicKey);
        Assert.NotEqual(file.ObjectKey, file.PublicKey);
        Assert.Equal(Now, file.PublishedOnUtc);
    }

    [Fact]
    public void Publish_KeepsTheKeyOfAFileThatIsAlreadyPublic()
    {
        var file = CleanFile();
        file.Publish(Now);
        var publicKey = file.PublicKey;

        var result = file.Publish(Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(publicKey, file.PublicKey);
        Assert.Equal(Now, file.PublishedOnUtc);
    }

    [Theory]
    [InlineData(ScanStatus.NotScanned)]
    [InlineData(ScanStatus.Pending)]
    public void Publish_WaitsForTheVirusScan(ScanStatus scanStatus)
    {
        var file = CleanFile();
        file.ScanStatus = scanStatus;

        var result = file.Publish(Now);

        Assert.Equal(StoredFileErrors.NotScanned, result.Error);
        Assert.Null(file.PublicKey);
    }

    [Fact]
    public void Publish_RefusesAFileWhoseScanFailed()
    {
        var file = CleanFile();
        file.ScanStatus = ScanStatus.Failed;

        var result = file.Publish(Now);

        Assert.Equal(StoredFileErrors.ScanFailed, result.Error);
        Assert.Null(file.PublicKey);
    }

    [Fact]
    public void Publish_RefusesAFileThatIsNotUploaded()
    {
        var file = new FakeStoredFileRepository().Add(Guid.NewGuid());

        var result = file.Publish(Now);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
        Assert.Null(file.PublicKey);
    }

    [Fact]
    public void Delete_TakesThePublicKeyAway()
    {
        var file = CleanFile();
        file.Publish(Now);

        file.Delete(Now);

        Assert.Null(file.PublicKey);
        Assert.Null(file.PublishedOnUtc);
    }

    [Fact]
    public async Task PublishFile_CopiesTheFileToThePublicBucketAndReturnsItsUrl()
    {
        var (files, storage, unitOfWork) = Build();
        var file = CleanFile(files, storage);

        var result = await PublishHandler(files, storage, unitOfWork).Handle(new PublishFileCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal($"https://cdn.test/{file.PublicKey}", result.Data);
        var copy = storage.PublicObjects[file.PublicKey!];
        Assert.Equal(FileSamples.Png, copy.Bytes);
        Assert.Equal("image/png", copy.ContentType);
        Assert.True(storage.Objects.ContainsKey(file.ObjectKey));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task PublishFile_ReturnsTheSameUrlWithoutCopyingAgain()
    {
        var (files, storage, unitOfWork) = Build();
        var file = CleanFile(files, storage);
        var first = await PublishHandler(files, storage, unitOfWork).Handle(new PublishFileCommand(file.Id), default);

        var second = await PublishHandler(files, storage, unitOfWork).Handle(new PublishFileCommand(file.Id), default);

        Assert.Equal(first.Data, second.Data);
        Assert.Equal(1, storage.PublicCopies);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task PublishFile_CopiesNothingBeforeTheVirusScan()
    {
        var (files, storage, unitOfWork) = Build();
        var file = CleanFile(files, storage);
        file.ScanStatus = ScanStatus.Pending;

        var result = await PublishHandler(files, storage, unitOfWork).Handle(new PublishFileCommand(file.Id), default);

        Assert.Equal(StoredFileErrors.NotScanned, result.Error);
        Assert.Empty(storage.PublicObjects);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task PublishFile_ReportsAnUnknownFile()
    {
        var (files, storage, unitOfWork) = Build();

        var result = await PublishHandler(files, storage, unitOfWork).Handle(new PublishFileCommand(Guid.NewGuid()), default);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UnpublishFile_RemovesThePublicCopyAndKeepsTheFile()
    {
        var (files, storage, unitOfWork) = Build();
        var file = CleanFile(files, storage);
        await PublishHandler(files, storage, new FakeUnitOfWork()).Handle(new PublishFileCommand(file.Id), default);

        var result = await new UnpublishFileCommandHandler(unitOfWork, storage, files)
            .Handle(new UnpublishFileCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(storage.PublicObjects);
        Assert.Null(file.PublicKey);
        Assert.Equal(StoredFileStatus.Uploaded, file.Status);
        Assert.True(storage.Objects.ContainsKey(file.ObjectKey));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UnpublishFile_IsHarmlessForAPrivateFile()
    {
        var (files, storage, unitOfWork) = Build();
        var file = CleanFile(files, storage);

        var result = await new UnpublishFileCommandHandler(unitOfWork, storage, files)
            .Handle(new UnpublishFileCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UnpublishFile_ReportsAnUnknownFile()
    {
        var (files, storage, unitOfWork) = Build();

        var result = await new UnpublishFileCommandHandler(unitOfWork, storage, files)
            .Handle(new UnpublishFileCommand(Guid.NewGuid()), default);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteFile_RemovesThePublicCopyToo()
    {
        var (files, storage, unitOfWork) = Build();
        var file = CleanFile(files, storage);
        await PublishHandler(files, storage, new FakeUnitOfWork()).Handle(new PublishFileCommand(file.Id), default);

        var result = await new DeleteFileCommandHandler(unitOfWork, storage, files, new FakeClock())
            .Handle(new DeleteFileCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(storage.PublicObjects);
        Assert.Null(file.PublicKey);
        Assert.Equal(StoredFileStatus.Deleted, file.Status);
    }

    [Fact]
    public void Validators_RequireTheFile()
    {
        Assert.Contains(
            new PublishFileCommandValidator().Validate(new PublishFileCommand(Guid.Empty)).Errors,
            error => error.PropertyName == nameof(PublishFileCommand.FileId));
        Assert.Contains(
            new UnpublishFileCommandValidator().Validate(new UnpublishFileCommand(Guid.Empty)).Errors,
            error => error.PropertyName == nameof(UnpublishFileCommand.FileId));
    }

    private static (FakeStoredFileRepository Files, FakeObjectStorage Storage, FakeUnitOfWork UnitOfWork) Build() =>
        (new FakeStoredFileRepository(), new FakeObjectStorage(), new FakeUnitOfWork());

    private static PublishFileCommandHandler PublishHandler(
        FakeStoredFileRepository files,
        FakeObjectStorage storage,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, storage, files, new FakeClock());

    private static StoredFile CleanFile() => CleanFile(new FakeStoredFileRepository(), new FakeObjectStorage());

    private static StoredFile CleanFile(FakeStoredFileRepository files, FakeObjectStorage storage)
    {
        var file = files.Add(Guid.NewGuid(), "cover.png", "image/png");
        file.CompleteUpload(64, Now.AddMinutes(-5));
        file.MarkClean(new string('a', 64), Now.AddMinutes(-4));
        storage.Objects[file.ObjectKey] = FileSamples.Png;
        return file;
    }
}
