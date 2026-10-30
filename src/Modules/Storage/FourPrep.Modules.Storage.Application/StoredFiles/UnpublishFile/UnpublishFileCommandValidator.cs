namespace FourPrep.Modules.Storage.Application.StoredFiles.UnpublishFile;

using FluentValidation;

internal sealed class UnpublishFileCommandValidator : AbstractValidator<UnpublishFileCommand>
{
    public UnpublishFileCommandValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
