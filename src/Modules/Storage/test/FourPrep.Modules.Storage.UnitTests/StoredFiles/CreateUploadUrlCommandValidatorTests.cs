namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.StoredFiles.CreateUploadUrl;

public class CreateUploadUrlCommandValidatorTests
{
    private const long TwentyMegabytes = 20L * 1024L * 1024L;

    private static readonly CreateUploadUrlCommandValidator Validator = new();

    [Theory]
    [InlineData("passport.pdf", "application/pdf")]
    [InlineData("Passport.PDF", "Application/PDF")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("scan.png", "image/png")]
    [InlineData("IMG_0001.HEIC", "image/heic")]
    public void Validate_WhenTheFileIsAnAllowedType_Passes(string fileName, string contentType)
    {
        Assert.True(Validator.Validate(new CreateUploadUrlCommand(fileName, contentType, 1024)).IsValid);
    }

    [Theory]
    [InlineData("cover.jpg", "image/jpeg")]
    [InlineData("cover.png", " Image/PNG ")]
    public void Validate_WhenAPublicImageIsJpgOrPng_Passes(string fileName, string contentType)
    {
        var command = new CreateUploadUrlCommand(fileName, contentType, 1024, StoredFilePurpose.PublicMedia);

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData("brochure.pdf", "application/pdf")]
    [InlineData("IMG_0001.HEIC", "image/heic")]
    public void Validate_WhenAPublicImageIsAnotherAllowedType_FailsWithUnsupportedMediaType(string fileName, string contentType)
    {
        var command = new CreateUploadUrlCommand(fileName, contentType, 1024, StoredFilePurpose.PublicMedia);

        AssertSingleError(command, StoredFileErrors.UnsupportedMediaType.Code, "ContentType");
    }

    [Fact]
    public void Validate_WhenThePurposeIsUnknown_FailsWithInvalidPurpose()
    {
        var command = new CreateUploadUrlCommand("a.pdf", "application/pdf", 1024, (StoredFilePurpose)9);

        AssertSingleError(command, StoredFileErrors.InvalidPurpose.Code, "Purpose");
    }

    [Fact]
    public void Validate_WhenTheTypeHasSpacesAround_Passes()
    {
        Assert.True(Validator.Validate(new CreateUploadUrlCommand("a.pdf", " application/pdf ", 1024)).IsValid);
    }

    [Fact]
    public void Validate_WhenTheFileIsExactlyTwentyMegabytes_Passes()
    {
        Assert.True(Validator.Validate(new CreateUploadUrlCommand("a.pdf", "application/pdf", TwentyMegabytes)).IsValid);
    }

    [Theory]
    [InlineData("setup.exe", "application/x-msdownload")]
    [InlineData("essay.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("photo.webp", "image/webp")]
    [InlineData("note.txt", "")]
    public void Validate_WhenTheTypeIsNotAllowed_FailsWithUnsupportedType(string fileName, string contentType)
    {
        AssertSingleError(new CreateUploadUrlCommand(fileName, contentType, 1024), StoredFileErrors.UnsupportedType.Code, "ContentType");
    }

    [Theory]
    [InlineData("photo.png", "application/pdf")]
    [InlineData("passport", "application/pdf")]
    [InlineData("scan.pdf.exe", "application/pdf")]
    public void Validate_WhenTheExtensionDoesNotMatchTheType_FailsWithUnsupportedType(string fileName, string contentType)
    {
        AssertSingleError(new CreateUploadUrlCommand(fileName, contentType, 1024), StoredFileErrors.UnsupportedType.Code, "FileName");
    }

    [Fact]
    public void Validate_WhenTheFileIsLargerThanTwentyMegabytes_FailsWithTooLarge()
    {
        AssertSingleError(new CreateUploadUrlCommand("a.pdf", "application/pdf", TwentyMegabytes + 1), StoredFileErrors.TooLarge.Code, "SizeBytes");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenTheFileIsEmpty_FailsWithEmpty(long sizeBytes)
    {
        AssertSingleError(new CreateUploadUrlCommand("a.pdf", "application/pdf", sizeBytes), StoredFileErrors.Empty.Code, "SizeBytes");
    }

    [Fact]
    public void Validate_WhenTheFileNameIsMissingOrTooLong_Fails()
    {
        var missing = Assert.Single(Validator.Validate(new CreateUploadUrlCommand(" ", "application/pdf", 10)).Errors);
        var tooLong = Validator.Validate(new CreateUploadUrlCommand(new string('a', 252) + ".pdf", "application/pdf", 10)).Errors;

        Assert.Equal("FileName", missing.PropertyName);
        Assert.Equal("NotEmptyValidator", missing.ErrorCode);
        Assert.Equal("MaximumLengthValidator", Assert.Single(tooLong).ErrorCode);
    }

    private static void AssertSingleError(CreateUploadUrlCommand command, string code, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(command).Errors);

        Assert.Equal(code, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
