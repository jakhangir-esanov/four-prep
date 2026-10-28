namespace FourPrep.Modules.Catalogue.Presentation.Destinations.Requests;

internal sealed record SetDestinationRequest
{
    public string? VisaType { get; init; }
}
