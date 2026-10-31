namespace FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

using FluentValidation;
using FourPrep.Common.Application.Exams;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Admission.Domain.ReportedScores;
using FourPrep.Modules.Admission.Domain.AcademyApplications;

internal sealed class SubmitAcademyApplicationCommandValidator : AbstractValidator<SubmitAcademyApplicationCommand>
{
    private const int MaxScores = 4;
    private const int MaxQuestionLength = 2000;

    public SubmitAcademyApplicationCommandValidator()
    {
        RuleFor(x => x.Interests)
            .Must(interests => interests is { Count: > 0 } && IsDistinct(interests))
            .WithError(AcademyApplicationErrors.InterestRequired);

        RuleForEach(x => x.Interests).IsInEnum();

        RuleFor(x => x.Scores)
            .Must(scores => scores is not null && scores.Count <= MaxScores)
            .WithError(ReportedScoreErrors.TooManyScores)
            .DependentRules(() =>
                RuleFor(x => x.Scores)
                    .Must(scores => scores.All(score => score is not null) && IsDistinct(scores.Select(score => score.Exam)))
                    .WithError(ReportedScoreErrors.DuplicateExam));

        RuleForEach(x => x.Scores).ChildRules(score =>
        {
            score.RuleFor(x => x.Exam).IsInEnum();
            score.RuleFor(x => x)
                .Must(x => ExamScores.IsValid(x.Exam, x.Score))
                .When(x => Enum.IsDefined(x.Exam))
                .WithError(ReportedScoreErrors.InvalidScore);
        });

        RuleFor(x => x.ContactChannels)
            .Must(channels => channels is { Count: > 0 } && IsDistinct(channels))
            .WithError(AcademyApplicationErrors.ContactChannelRequired);

        RuleForEach(x => x.ContactChannels).IsInEnum();

        RuleFor(x => x.PhoneNumber).PhoneNumber();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .When(x => Chooses(x, ContactChannel.Phone))
            .WithError(AcademyApplicationErrors.PhoneRequired);

        RuleFor(x => x.TelegramUsername)
            .Must(TelegramUsernames.IsValid)
            .When(x => Chooses(x, ContactChannel.Telegram) || !string.IsNullOrWhiteSpace(x.TelegramUsername))
            .WithError(AcademyApplicationErrors.TelegramRequired);

        RuleFor(x => x.MeetingLanguage).NotEmpty().Locale();

        RuleFor(x => x.Question).MaximumLength(MaxQuestionLength);
    }

    private static bool Chooses(SubmitAcademyApplicationCommand command, ContactChannel channel) =>
        command.ContactChannels?.Contains(channel) == true;

    private static bool IsDistinct<T>(IEnumerable<T> values)
    {
        var list = values.ToList();
        return list.Distinct().Count() == list.Count;
    }
}
