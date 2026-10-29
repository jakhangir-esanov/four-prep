namespace FourPrep.Modules.Storage.Application.StoredFiles.CompleteUpload;

using FluentValidation;

internal sealed class CompleteUploadCommandValidator : AbstractValidator<CompleteUploadCommand>
{
    public CompleteUploadCommandValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
