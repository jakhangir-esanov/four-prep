namespace FourPrep.Modules.Catalogue.Domain.Scholarships;

using FourPrep.Common.Domain.Enums;

public sealed class Scholarship
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ScholarshipKind Kind { get; set; }
    public Dictionary<string, string> Amount { get; set; } = [];
    public Dictionary<string, string> Eligibility { get; set; } = [];
    public string SourceUrl { get; set; } = string.Empty;
    public DateOnly CheckedOn { get; set; }

    public static Scholarship Create(
        string name,
        ScholarshipKind kind,
        Dictionary<string, string> amount,
        Dictionary<string, string> eligibility,
        string sourceUrl,
        DateOnly checkedOn)
    {
        return new Scholarship
        {
            Id = Guid.NewGuid(),
            Name = name,
            Kind = kind,
            Amount = amount,
            Eligibility = eligibility,
            SourceUrl = sourceUrl,
            CheckedOn = checkedOn
        };
    }
}
