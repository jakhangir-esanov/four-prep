namespace FourPrep.Modules.Content.Application.NewsPosts.UpdateNewsPost;

using FluentValidation;

internal sealed class UpdateNewsPostCommandValidator : AbstractValidator<UpdateNewsPostCommand>
{
    public UpdateNewsPostCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Details).NotNull().SetValidator(new NewsPostDetailsValidator());
    }
}
