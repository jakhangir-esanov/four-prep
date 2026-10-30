namespace FourPrep.Modules.Storage.Application.StoredFiles.PublishFile;

using FluentValidation;

internal sealed class PublishFileCommandValidator : AbstractValidator<PublishFileCommand>
{
    public PublishFileCommandValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
