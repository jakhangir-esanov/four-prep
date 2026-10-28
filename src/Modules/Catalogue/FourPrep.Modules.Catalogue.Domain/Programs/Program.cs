namespace FourPrep.Modules.Catalogue.Domain.Programs;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Deadlines;
using FourPrep.Modules.Catalogue.Domain.AdmissionRequirements;

public sealed class Program
{
    private readonly List<AdmissionRequirement> _requirements = [];
    private readonly List<Deadline> _deadlines = [];

    public Guid Id { get; set; }
    public Guid UniversityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DegreeLevel DegreeLevel { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int? DurationMonths { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public DateOnly CheckedOn { get; set; }
    public Guid CheckedByUserId { get; set; }
    public IReadOnlyCollection<AdmissionRequirement> Requirements => _requirements;
    public IReadOnlyCollection<Deadline> Deadlines => _deadlines;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static Program Create(
        Guid universityId,
        string name,
        DegreeLevel degreeLevel,
        string field,
        string language,
        int? durationMonths,
        string sourceUrl,
        DateOnly checkedOn,
        Guid checkedByUserId,
        DateTime utcNow)
    {
        var program = new Program
        {
            Id = Guid.NewGuid(),
            UniversityId = universityId,
            CreatedOnUtc = utcNow
        };

        program.Update(name, degreeLevel, field, language, durationMonths, sourceUrl, checkedOn, checkedByUserId, utcNow);

        return program;
    }

    public void Update(
        string name,
        DegreeLevel degreeLevel,
        string field,
        string language,
        int? durationMonths,
        string sourceUrl,
        DateOnly checkedOn,
        Guid checkedByUserId,
        DateTime utcNow)
    {
        Name = name;
        DegreeLevel = degreeLevel;
        Field = field;
        Language = language;
        DurationMonths = durationMonths;
        SourceUrl = sourceUrl;
        CheckedOn = checkedOn;
        CheckedByUserId = checkedByUserId;
        UpdatedOnUtc = utcNow;
    }

    public void ReplaceRequirements(IEnumerable<AdmissionRequirement> requirements, Guid checkedByUserId, DateTime utcNow)
    {
        _requirements.Clear();
        _requirements.AddRange(requirements);
        CheckedByUserId = checkedByUserId;
        UpdatedOnUtc = utcNow;
    }

    public void ReplaceDeadlines(IEnumerable<Deadline> deadlines, Guid checkedByUserId, DateTime utcNow)
    {
        _deadlines.Clear();
        _deadlines.AddRange(deadlines);
        CheckedByUserId = checkedByUserId;
        UpdatedOnUtc = utcNow;
    }
}
