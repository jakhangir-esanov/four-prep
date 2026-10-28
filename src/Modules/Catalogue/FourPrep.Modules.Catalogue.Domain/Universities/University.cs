namespace FourPrep.Modules.Catalogue.Domain.Universities;

using FourPrep.Modules.Catalogue.Domain.Costs;
using FourPrep.Modules.Catalogue.Domain.Scholarships;

public sealed class University
{
    private readonly List<Cost> _costs = [];
    private readonly List<Scholarship> _scholarships = [];

    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Website { get; set; }
    public string? RankingSource { get; set; }
    public int? Ranking { get; set; }
    public decimal? AcceptanceRate { get; set; }
    public bool IsPartner { get; set; }
    public Dictionary<string, string> Description { get; set; } = [];
    public string SourceUrl { get; set; } = string.Empty;
    public DateOnly CheckedOn { get; set; }
    public Guid CheckedByUserId { get; set; }
    public IReadOnlyCollection<Cost> Costs => _costs;
    public IReadOnlyCollection<Scholarship> Scholarships => _scholarships;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static University Create(
        string slug,
        string name,
        string countryCode,
        string? city,
        string? website,
        string? rankingSource,
        int? ranking,
        decimal? acceptanceRate,
        bool isPartner,
        Dictionary<string, string> description,
        string sourceUrl,
        DateOnly checkedOn,
        Guid checkedByUserId,
        DateTime utcNow)
    {
        var university = new University
        {
            Id = Guid.NewGuid(),
            CreatedOnUtc = utcNow
        };

        university.Update(
            slug,
            name,
            countryCode,
            city,
            website,
            rankingSource,
            ranking,
            acceptanceRate,
            isPartner,
            description,
            sourceUrl,
            checkedOn,
            checkedByUserId,
            utcNow);

        return university;
    }

    public void Update(
        string slug,
        string name,
        string countryCode,
        string? city,
        string? website,
        string? rankingSource,
        int? ranking,
        decimal? acceptanceRate,
        bool isPartner,
        Dictionary<string, string> description,
        string sourceUrl,
        DateOnly checkedOn,
        Guid checkedByUserId,
        DateTime utcNow)
    {
        Slug = slug;
        Name = name;
        CountryCode = countryCode;
        City = city;
        Website = website;
        RankingSource = rankingSource;
        Ranking = ranking;
        AcceptanceRate = acceptanceRate;
        IsPartner = isPartner;
        Description = description;
        SourceUrl = sourceUrl;
        CheckedOn = checkedOn;
        CheckedByUserId = checkedByUserId;
        UpdatedOnUtc = utcNow;
    }

    public void ReplaceCosts(IEnumerable<Cost> costs, Guid checkedByUserId, DateTime utcNow)
    {
        _costs.Clear();
        _costs.AddRange(costs);
        CheckedByUserId = checkedByUserId;
        UpdatedOnUtc = utcNow;
    }

    public void ReplaceScholarships(IEnumerable<Scholarship> scholarships, Guid checkedByUserId, DateTime utcNow)
    {
        _scholarships.Clear();
        _scholarships.AddRange(scholarships);
        CheckedByUserId = checkedByUserId;
        UpdatedOnUtc = utcNow;
    }
}
