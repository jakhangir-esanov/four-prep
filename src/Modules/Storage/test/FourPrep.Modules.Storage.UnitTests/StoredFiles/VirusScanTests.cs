namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.StoredFiles.CompleteUpload;
using FourPrep.Modules.Storage.Application.StoredFiles.ScanUploadedFiles;

public class VirusScanTests
{
    private const string EicarMarker = "EICAR-STANDARD-ANTIVIRUS-TEST-FILE";

    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public async Task Complete_AsksForAScanAndLeavesTheFileWaitingForIt()
    {
        var files = new FakeStoredFileRepository();
        var storage = new FakeObjectStorage();
        var queue = new FakeVirusScanQueue();
        var file = files.Add(OwnerId, sizeBytes: FileSamples.Pdf.Length);
        storage.Objects[file.UploadKey] = FileSamples.Pdf;

        var result = await new CompleteUploadCommandHandler(
                new FakeUnitOfWork(), new FakeCurrentUser(OwnerId), storage, queue, files, new FakeClock())
            .Handle(new CompleteUploadCommand(file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(ScanStatus.Pending, file.ScanStatus);
        Assert.Equal(1, queue.Requests);
    }

    [Fact]
    public async Task Complete_DoesNotAskForAScanWhenTheUploadIsRefused()
    {
        var files = new FakeStoredFileRepository();
        var storage = new FakeObjectStorage();
        var queue = new FakeVirusScanQueue();
        var file = files.Add(OwnerId, "photo.png", "image/png", FileSamples.Pdf.Length);
        storage.Objects[file.UploadKey] = FileSamples.Pdf;

        await new CompleteUploadCommandHandler(
                new FakeUnitOfWork(), new FakeCurrentUser(OwnerId), storage, queue, files, new FakeClock())
            .Handle(new CompleteUploadCommand(file.Id), default);

        Assert.Equal(0, queue.Requests);
    }

    [Fact]
    public async Task Scan_MarksACleanFileWithItsSha256()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        var file = Uploaded(files, storage, FileSamples.Pdf);

        var result = await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);

        Assert.Equal(1, result.Data);
        Assert.Equal(ScanStatus.Clean, file.ScanStatus);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(FileSamples.Pdf)), file.Sha256);
        Assert.Equal(new FakeClock().UtcNow, file.ScannedOnUtc);
        Assert.Equal(StoredFileStatus.Uploaded, file.Status);
        Assert.True(storage.Objects.ContainsKey(file.ObjectKey));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Scan_DeletesAnInfectedFile()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        scanner.InfectedContents.Add(EicarMarker);
        byte[] infected = [.. FileSamples.Pdf, .. Encoding.ASCII.GetBytes(EicarMarker)];
        var file = Uploaded(files, storage, infected);

        await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);

        Assert.Equal(ScanStatus.Infected, file.ScanStatus);
        Assert.Equal(StoredFileStatus.Deleted, file.Status);
        Assert.Equal(new FakeClock().UtcNow, file.DeletedOnUtc);
        Assert.Null(file.Sha256);
        Assert.Equal([file.ObjectKey], storage.Deleted);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Scan_LeavesFilesThatAreNotWaitingAlone()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        var pending = files.Add(OwnerId);
        var clean = Uploaded(files, storage, FileSamples.Pdf);
        clean.MarkClean("abc", new FakeClock().UtcNow);

        var result = await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);

        Assert.Equal(0, result.Data);
        Assert.Equal(ScanStatus.NotScanned, pending.ScanStatus);
        Assert.Equal("abc", clean.Sha256);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Scan_TakesOneBatchPerRun()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        for (var i = 0; i < ScanUploadedFilesCommandHandler.BatchSize + 3; i++)
        {
            Uploaded(files, storage, FileSamples.Pdf);
        }

        var result = await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);

        Assert.Equal(ScanUploadedFilesCommandHandler.BatchSize, result.Data);
        Assert.Equal(3, files.Files.Count(x => x.ScanStatus == ScanStatus.Pending));
    }

    [Fact]
    public async Task Scan_KeepsTheOtherResultsWhenOneFileCannotBeScanned()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        scanner.InfectedContents.Add(EicarMarker);
        scanner.BrokenContents.Add("BROKEN");
        var infected = Uploaded(files, storage, [.. FileSamples.Pdf, .. Encoding.ASCII.GetBytes(EicarMarker)]);
        var broken = Uploaded(files, storage, [.. FileSamples.Pdf, .. "BROKEN"u8]);
        var clean = Uploaded(files, storage, FileSamples.Pdf);

        var result = await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);

        Assert.Equal(3, result.Data);
        Assert.Equal(ScanStatus.Infected, infected.ScanStatus);
        Assert.Equal(ScanStatus.Pending, broken.ScanStatus);
        Assert.Equal(1, broken.ScanAttempts);
        Assert.Equal(ScanStatus.Clean, clean.ScanStatus);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Scan_CountsAMissingObjectAsAFailedAttempt()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        var file = Uploaded(files, storage, FileSamples.Pdf);
        storage.Objects.Remove(file.ObjectKey);

        await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);

        Assert.Equal(1, file.ScanAttempts);
        Assert.Equal(ScanStatus.Pending, file.ScanStatus);
    }

    [Fact]
    public async Task Scan_GivesUpAfterThreeFailedAttempts()
    {
        var (files, storage, scanner, unitOfWork) = Build();
        scanner.BrokenContents.Add("BROKEN");
        var file = Uploaded(files, storage, [.. FileSamples.Pdf, .. "BROKEN"u8]);

        for (var i = 0; i < StoredFile.MaxScanAttempts; i++)
        {
            await Handler(files, storage, scanner, unitOfWork).Handle(new ScanUploadedFilesCommand(), default);
        }

        Assert.Equal(ScanStatus.Failed, file.ScanStatus);
        Assert.Equal(StoredFile.MaxScanAttempts, file.ScanAttempts);
        Assert.Equal(new FakeClock().UtcNow, file.ScannedOnUtc);
        Assert.Empty(await files.GetAwaitingScanAsync(10));
    }

    [Fact]
    public async Task Scan_PutsFilesThatFailedBehindFreshOnes()
    {
        var files = new FakeStoredFileRepository();
        var storage = new FakeObjectStorage();
        var failedOnce = Uploaded(files, storage, FileSamples.Pdf);
        failedOnce.RecordScanFailure(new FakeClock().UtcNow);
        var fresh = Uploaded(files, storage, FileSamples.Pdf);

        var next = await files.GetAwaitingScanAsync(1);

        Assert.Equal(fresh.Id, Assert.Single(next).Id);
        Assert.NotEqual(failedOnce.Id, next[0].Id);
    }

    private static StoredFile Uploaded(FakeStoredFileRepository files, FakeObjectStorage storage, byte[] content)
    {
        var file = files.Add(OwnerId, sizeBytes: content.Length);
        file.CompleteUpload(content.Length, new FakeClock().UtcNow);
        storage.Objects[file.ObjectKey] = content;
        return file;
    }

    private static (FakeStoredFileRepository, FakeObjectStorage, FakeVirusScanner, FakeUnitOfWork) Build() =>
        (new FakeStoredFileRepository(), new FakeObjectStorage(), new FakeVirusScanner(), new FakeUnitOfWork());

    private static ScanUploadedFilesCommandHandler Handler(
        FakeStoredFileRepository files,
        FakeObjectStorage storage,
        FakeVirusScanner scanner,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, scanner, storage, files, new FakeClock(), NullLogger<ScanUploadedFilesCommandHandler>.Instance);
}
