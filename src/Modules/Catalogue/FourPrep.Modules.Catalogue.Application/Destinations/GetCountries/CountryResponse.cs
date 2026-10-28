namespace FourPrep.Modules.Catalogue.Application.Destinations.GetCountries;

public sealed record CountryResponse
{
    public string CountryCode { get; init; } = string.Empty;
    public int UniversityCount { get; init; }
    public bool IsServiceDestination { get; init; }
    public string? VisaType { get; init; }
}
