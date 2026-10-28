namespace FourPrep.Modules.Catalogue.Domain.AdmissionRequirements;

using FourPrep.Common.Domain.Enums;

public sealed class AdmissionRequirement
{
    public Guid Id { get; set; }
    public IntakeTerm IntakeTerm { get; set; }
    public int IntakeYear { get; set; }
    public Dictionary<Exam, decimal> EnglishMinimums { get; set; } = [];
    public SatPolicy? SatPolicy { get; set; }
    public int? SatMidRangeLow { get; set; }
    public int? SatMidRangeHigh { get; set; }
    public decimal? MinimumGpa { get; set; }
    public decimal? GpaScale { get; set; }
    public List<string> Documents { get; set; } = [];
    public int? EssaysCount { get; set; }
    public bool? InterviewRequired { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public DateOnly CheckedOn { get; set; }

    public static AdmissionRequirement Create(
        IntakeTerm intakeTerm,
        int intakeYear,
        Dictionary<Exam, decimal> englishMinimums,
        SatPolicy? satPolicy,
        int? satMidRangeLow,
        int? satMidRangeHigh,
        decimal? minimumGpa,
        decimal? gpaScale,
        List<string> documents,
        int? essaysCount,
        bool? interviewRequired,
        string sourceUrl,
        DateOnly checkedOn)
    {
        return new AdmissionRequirement
        {
            Id = Guid.NewGuid(),
            IntakeTerm = intakeTerm,
            IntakeYear = intakeYear,
            EnglishMinimums = englishMinimums,
            SatPolicy = satPolicy,
            SatMidRangeLow = satMidRangeLow,
            SatMidRangeHigh = satMidRangeHigh,
            MinimumGpa = minimumGpa,
            GpaScale = gpaScale,
            Documents = documents,
            EssaysCount = essaysCount,
            InterviewRequired = interviewRequired,
            SourceUrl = sourceUrl,
            CheckedOn = checkedOn
        };
    }
}
