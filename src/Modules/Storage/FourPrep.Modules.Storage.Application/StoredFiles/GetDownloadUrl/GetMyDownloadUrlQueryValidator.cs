namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using FluentValidation;

internal sealed class GetMyDownloadUrlQueryValidator : AbstractValidator<GetMyDownloadUrlQuery>
{
    public GetMyDownloadUrlQueryValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
    }
}
