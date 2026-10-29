namespace FourPrep.Modules.Storage.UnitTests.StoredFiles;

using FourPrep.Modules.Storage.Application.StoredFiles.CompleteUpload;

public class CompleteUploadCommandValidatorTests
{
    private static readonly CompleteUploadCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheFileIsGiven_Passes()
    {
        Assert.True(Validator.Validate(new CompleteUploadCommand(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WhenTheFileIsMissing_Fails()
    {
        var error = Assert.Single(Validator.Validate(new CompleteUploadCommand(Guid.Empty)).Errors);

        Assert.Equal(nameof(CompleteUploadCommand.FileId), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }
}
