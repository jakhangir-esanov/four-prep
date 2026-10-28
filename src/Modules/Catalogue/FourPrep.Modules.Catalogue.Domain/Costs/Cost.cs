namespace FourPrep.Modules.Catalogue.Domain.Costs;

public sealed class Cost
{
    public Guid Id { get; set; }
    public int AcademicYear { get; set; }
    public long TuitionMinor { get; set; }
    public long? LivingMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateOnly CheckedOn { get; set; }

    public static Cost Create(
        int academicYear,
        long tuitionMinor,
        long? livingMinor,
        string currency,
        string sourceUrl,
        DateOnly checkedOn)
    {
        return new Cost
        {
            Id = Guid.NewGuid(),
            AcademicYear = academicYear,
            TuitionMinor = tuitionMinor,
            LivingMinor = livingMinor,
            Currency = currency,
            SourceUrl = sourceUrl,
            CheckedOn = checkedOn
        };
    }
}
