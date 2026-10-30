namespace FourPrep.Modules.Content.Application.NewsPosts.CreateNewsPost;

using FluentValidation;

internal sealed class CreateNewsPostCommandValidator : AbstractValidator<CreateNewsPostCommand>
{
    public CreateNewsPostCommandValidator()
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new NewsPostDetailsValidator());
    }
}
