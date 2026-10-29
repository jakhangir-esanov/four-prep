namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using FluentValidation;

internal sealed class GetDownloadUrlQueryValidator : AbstractValidator<GetDownloadUrlQuery>
{
    public GetDownloadUrlQueryValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
