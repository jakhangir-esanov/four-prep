namespace FourPrep.Modules.Storage.Application.StoredFiles.DeleteFile;

using FluentValidation;

internal sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
