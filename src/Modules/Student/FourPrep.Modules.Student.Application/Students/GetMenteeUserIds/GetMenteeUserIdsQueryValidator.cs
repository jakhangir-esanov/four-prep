namespace FourPrep.Modules.Student.Application.Students.GetMenteeUserIds;

using FluentValidation;

internal sealed class GetMenteeUserIdsQueryValidator : AbstractValidator<GetMenteeUserIdsQuery>
{
    public GetMenteeUserIdsQueryValidator()
    {
        RuleFor(x => x.MentorUserId).NotEmpty();
    }
}
