namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.StoredFiles.DeleteFile;
using FourPrep.Modules.Storage.Application.StoredFiles.GetStoredFile;
using FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

public class FileAccessTests
{
    private static readonly DateTime Now = new FakeClock().UtcNow;

    [Fact]
    public async Task DownloadUrl_IsGivenForACleanFileWithTheFiveMinuteWindow()
    {
        var storage = new FakeObjectStorage();

        var result = await FileDownloads.CreateUrlAsync(File(StoredFileStatus.Uploaded, ScanStatus.Clean), storage, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now + StoredFile.DownloadWindow, result.Data.ExpiresOnUtc);
        Assert.Contains("docs/key", result.Data.DownloadUrl);
        Assert.Contains("passport.pdf", result.Data.DownloadUrl);
    }

    [Theory]
    [InlineData(ScanStatus.Pending)]
    [InlineData(ScanStatus.NotScanned)]
    public async Task DownloadUrl_WaitsForTheVirusScan(ScanStatus scanStatus)
    {
        var result = await FileDownloads.CreateUrlAsync(File(StoredFileStatus.Uploaded, scanStatus), new FakeObjectStorage(), Now);

        Assert.Equal(StoredFileErrors.NotScanned, result.Error);
    }

    [Fact]
    public async Task DownloadUrl_AsksForANewUploadWhenTheScanFailed()
    {
        var result = await FileDownloads.CreateUrlAsync(File(StoredFileStatus.Uploaded, ScanStatus.Failed), new FakeObjectStorage(), Now);

        Assert.Equal(StoredFileErrors.ScanFailed, result.Error);
    }

    [Theory]
    [InlineData(StoredFileStatus.Pending, ScanStatus.NotScanned)]
    [InlineData(StoredFileStatus.Deleted, ScanStatus.Infected)]
    [InlineData(StoredFileStatus.Deleted, ScanStatus.Clean)]
    public async Task DownloadUrl_ReportsAFileThatIsNotStored(StoredFileStatus status, ScanStatus scanStatus)
    {
        var result = await FileDownloads.CreateUrlAsync(File(status, scanStatus), new FakeObjectStorage(), Now);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DownloadUrl_ReportsAMissingFile()
    {
        var result = await FileDownloads.CreateUrlAsync(null, new FakeObjectStorage(), Now);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Delete_RemovesBothKeysAndKeepsTheRecord()
    {
        var files = new FakeStoredFileRepository();
        var storage = new FakeObjectStorage();
        var unitOfWork = new FakeUnitOfWork();
        var file = files.Add(Guid.NewGuid());
        file.CompleteUpload(64, Now);
        storage.Objects[file.ObjectKey] = FileSamples.Pdf;

        var result = await new DeleteFileCommandHandler(unitOfWork, storage, files, new FakeClock())
            .Handle(new DeleteFileCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(StoredFileStatus.Deleted, file.Status);
        Assert.Equal(Now, file.DeletedOnUtc);
        Assert.Equal([file.UploadKey, file.ObjectKey], storage.Deleted);
        Assert.Single(files.Files);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_IsHarmlessForAFileAlreadyDeleted()
    {
        var files = new FakeStoredFileRepository();
        var storage = new FakeObjectStorage();
        var unitOfWork = new FakeUnitOfWork();
        var file = files.Add(Guid.NewGuid());
        file.Delete(Now.AddDays(-1));

        var result = await new DeleteFileCommandHandler(unitOfWork, storage, files, new FakeClock())
            .Handle(new DeleteFileCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now.AddDays(-1), file.DeletedOnUtc);
        Assert.Empty(storage.Deleted);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_ReportsAnUnknownFile()
    {
        var result = await new DeleteFileCommandHandler(new FakeUnitOfWork(), new FakeObjectStorage(), new FakeStoredFileRepository(), new FakeClock())
            .Handle(new DeleteFileCommand(Guid.NewGuid()), default);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
    }

    [Fact]
    public void StoredFile_IsReadyOnlyWhenUploadedAndClean()
    {
        Assert.True(new StoredFileResponse { Status = StoredFileStatus.Uploaded, ScanStatus = ScanStatus.Clean }.IsReady);
        Assert.False(new StoredFileResponse { Status = StoredFileStatus.Uploaded, ScanStatus = ScanStatus.Pending }.IsReady);
        Assert.False(new StoredFileResponse { Status = StoredFileStatus.Deleted, ScanStatus = ScanStatus.Clean }.IsReady);
    }

    [Fact]
    public void Validators_RequireTheFile()
    {
        Assert.False(new DeleteFileCommandValidator().Validate(new DeleteFileCommand(Guid.Empty)).IsValid);
        Assert.False(new GetStoredFileQueryValidator().Validate(new GetStoredFileQuery(Guid.Empty)).IsValid);
        Assert.False(new GetDownloadUrlQueryValidator().Validate(new GetDownloadUrlQuery(Guid.Empty)).IsValid);
        Assert.False(new GetMyDownloadUrlQueryValidator().Validate(new GetMyDownloadUrlQuery(Guid.Empty)).IsValid);
        Assert.True(new DeleteFileCommandValidator().Validate(new DeleteFileCommand(Guid.NewGuid())).IsValid);
    }

    private static DownloadableFile File(StoredFileStatus status, ScanStatus scanStatus) =>
        new()
        {
            ObjectKey = "docs/key",
            FileName = "passport.pdf",
            ContentType = "application/pdf",
            UploadedByUserId = Guid.NewGuid(),
            Status = status,
            ScanStatus = scanStatus
        };
}
