namespace FourPrep.Modules.Content.Application.NewsPosts.SetNewsPostCover;

using FluentValidation;

internal sealed class SetNewsPostCoverCommandValidator : AbstractValidator<SetNewsPostCoverCommand>
{
    public SetNewsPostCoverCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FileId).NotEmpty();
    }
}
