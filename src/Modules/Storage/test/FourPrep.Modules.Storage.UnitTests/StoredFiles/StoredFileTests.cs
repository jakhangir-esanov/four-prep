namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.StoredFiles;

public class StoredFileTests
{
    private static readonly DateTime Now = new FakeClock().UtcNow;

    [Fact]
    public void Create_GivesTheUploadAndTheStoredCopyDifferentRandomKeys()
    {
        var first = File();
        var second = File();

        Assert.NotEqual(first.UploadKey, first.ObjectKey);
        Assert.NotEqual(first.ObjectKey, second.ObjectKey);
        Assert.NotEqual(first.UploadKey, second.UploadKey);
    }

    [Fact]
    public void CompleteUpload_RefusesAFileThatIsAlreadyUploaded()
    {
        var file = File();
        file.CompleteUpload(64, Now);

        var result = file.CompleteUpload(64, Now.AddMinutes(1));

        Assert.Equal(StoredFileErrors.AlreadyUploaded, result.Error);
        Assert.Equal(Now, file.UploadedOnUtc);
    }

    [Fact]
    public void CompleteUpload_RefusesADeletedFile()
    {
        var file = File();
        file.Delete(Now);

        var result = file.CompleteUpload(64, Now);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
        Assert.Equal(StoredFileStatus.Deleted, file.Status);
    }

    [Fact]
    public void CompleteUpload_RefusesAnotherSize()
    {
        var file = File();

        var result = file.CompleteUpload(65, Now);

        Assert.Equal(StoredFileErrors.SizeMismatch, result.Error);
        Assert.Equal(StoredFileStatus.Pending, file.Status);
        Assert.Null(file.UploadedOnUtc);
    }

    [Fact]
    public void Delete_KeepsTheFirstDeletionTime()
    {
        var file = File();

        file.Delete(Now);
        file.Delete(Now.AddHours(1));

        Assert.Equal(StoredFileStatus.Deleted, file.Status);
        Assert.Equal(Now, file.DeletedOnUtc);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    [InlineData("image/heic")]
    public void MatchesSignature_RefusesAHeaderThatIsTooShort(string contentType)
    {
        Assert.False(FileUploadRules.MatchesSignature(contentType, [0x25, 0x50]));
    }

    [Fact]
    public void MatchesSignature_RefusesAJpegThatIsReallyAPng()
    {
        Assert.False(FileUploadRules.MatchesSignature("image/jpeg", FileSamples.Png));
    }

    [Fact]
    public void MatchesSignature_RefusesHeicWithoutTheFileTypeBox()
    {
        byte[] header = [0x00, 0x00, 0x00, 0x18, .. "moovheic"u8, .. new byte[4]];

        Assert.False(FileUploadRules.MatchesSignature("image/heic", header));
    }

    [Fact]
    public void MatchesSignature_RefusesHeicWithAnUnknownBrand()
    {
        byte[] header = [0x00, 0x00, 0x00, 0x18, .. "ftypmp42"u8, .. new byte[4]];

        Assert.False(FileUploadRules.MatchesSignature("image/heic", header));
    }

    [Fact]
    public void MatchesSignature_RefusesATypeOutsideTheList()
    {
        Assert.False(FileUploadRules.MatchesSignature("text/html", "<html>"u8.ToArray()));
    }

    private static StoredFile File() => StoredFile.Create("passport.pdf", "application/pdf", 64, Guid.NewGuid(), Now);
}
