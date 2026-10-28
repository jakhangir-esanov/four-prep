namespace FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

using FluentValidation;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Exams;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Application.Facts;
using FourPrep.Modules.Catalogue.Domain.AdmissionRequirements;

internal sealed class ReplaceProgramRequirementsCommandValidator
    : AbstractValidator<ReplaceProgramRequirementsCommand>
{
    private const int MaxRequirements = 20;
    private const int FirstIntakeYear = 2000;
    private const int LastIntakeYear = 2100;
    private const int MaxDocuments = 30;
    private const int MaxDocumentLength = 200;
    private const int MaxEssays = 20;
    private const decimal MaxGpaScale = 100;

    private static readonly HashSet<Exam> EnglishExams = [Exam.Ielts, Exam.Toefl, Exam.Duolingo];

    public ReplaceProgramRequirementsCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Requirements)
            .NotNull()
            .Must(requirements => requirements.Count <= MaxRequirements)
            .Must(requirements => requirements
                .Select(requirement => (requirement.IntakeTerm, requirement.IntakeYear))
                .Distinct()
                .Count() == requirements.Count)
            .WithError(AdmissionRequirementErrors.DuplicateIntake);

        RuleForEach(x => x.Requirements).ChildRules(requirement =>
        {
            requirement.RuleFor(x => x.IntakeTerm).IsInEnum();
            requirement.RuleFor(x => x.IntakeYear)
                .InclusiveBetween(FirstIntakeYear, LastIntakeYear)
                .WithError(AdmissionRequirementErrors.InvalidIntakeYear);

            requirement.RuleFor(x => x.EnglishMinimums)
                .Must(HaveValidEnglishScores)
                .WithError(AdmissionRequirementErrors.InvalidEnglishMinimum);

            requirement.RuleFor(x => x.SatPolicy).IsInEnum();
            requirement.RuleFor(x => x)
                .Must(HaveValidSatRange)
                .OverridePropertyName(nameof(AdmissionRequirementInput.SatMidRangeLow))
                .WithError(AdmissionRequirementErrors.InvalidSatRange);

            requirement.RuleFor(x => x)
                .Must(HaveValidGpa)
                .OverridePropertyName(nameof(AdmissionRequirementInput.MinimumGpa))
                .WithError(AdmissionRequirementErrors.InvalidGpa);

            requirement.RuleFor(x => x.Documents)
                .Must(documents => documents is null || documents.Count <= MaxDocuments);
            requirement.RuleForEach(x => x.Documents).NotEmpty().MaximumLength(MaxDocumentLength);
            requirement.RuleFor(x => x.EssaysCount).InclusiveBetween(0, MaxEssays);
            requirement.RuleFor(x => x.SourceUrl).SourceUrl();
            requirement.RuleFor(x => x.CheckedOn).CheckedOn(timeZoneContext);
        });
    }

    private static bool HaveValidEnglishScores(IReadOnlyDictionary<Exam, decimal>? minimums) =>
        minimums is null
        || minimums.All(minimum => EnglishExams.Contains(minimum.Key) && ExamScores.IsValid(minimum.Key, minimum.Value));

    private static bool HaveValidSatRange(AdmissionRequirementInput requirement) =>
        (requirement.SatMidRangeLow, requirement.SatMidRangeHigh) switch
        {
            (null, null) => true,
            (int low, int high) => ExamScores.IsValid(Exam.Sat, low) && ExamScores.IsValid(Exam.Sat, high) && low <= high,
            _ => false
        };

    private static bool HaveValidGpa(AdmissionRequirementInput requirement) =>
        (requirement.MinimumGpa, requirement.GpaScale) switch
        {
            (null, _) => requirement.GpaScale is null or > 0 and <= MaxGpaScale,
            (decimal minimum, decimal scale) => scale is > 0 and <= MaxGpaScale && minimum >= 0 && minimum <= scale,
            _ => false
        };
}
