namespace FourPrep.Modules.Catalogue.Domain.Destinations;

public sealed class Destination
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static Destination Create(string countryCode, string visaType, DateTime utcNow)
    {
        return new Destination
        {
            Id = Guid.NewGuid(),
            CountryCode = countryCode,
            VisaType = visaType,
            CreatedOnUtc = utcNow,
            UpdatedOnUtc = utcNow
        };
    }

    public void ChangeVisaType(string visaType, DateTime utcNow)
    {
        VisaType = visaType;
        UpdatedOnUtc = utcNow;
    }
}
