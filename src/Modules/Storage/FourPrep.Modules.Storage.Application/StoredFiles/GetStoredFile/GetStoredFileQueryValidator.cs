namespace FourPrep.Modules.Storage.Application.StoredFiles.GetStoredFile;

using FluentValidation;

internal sealed class GetStoredFileQueryValidator : AbstractValidator<GetStoredFileQuery>
{
    public GetStoredFileQueryValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
