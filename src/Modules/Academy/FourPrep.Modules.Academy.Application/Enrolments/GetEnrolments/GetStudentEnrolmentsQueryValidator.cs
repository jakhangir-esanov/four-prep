namespace FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;

using FluentValidation;

internal sealed class GetStudentEnrolmentsQueryValidator : AbstractValidator<GetStudentEnrolmentsQuery>
{
    public GetStudentEnrolmentsQueryValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
    }
}
