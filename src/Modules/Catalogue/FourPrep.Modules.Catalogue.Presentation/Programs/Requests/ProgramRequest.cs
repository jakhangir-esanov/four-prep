namespace FourPrep.Modules.Catalogue.Presentation.Programs.Requests;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Application.Programs;

internal sealed record ProgramRequest
{
    public string? Name { get; init; }
    public DegreeLevel DegreeLevel { get; init; }
    public string? Field { get; init; }
    public string? Language { get; init; }
    public int? DurationMonths { get; init; }
    public string? SourceUrl { get; init; }
    public DateOnly CheckedOn { get; init; }

    public ProgramDetails ToDetails() =>
        new(
            Name ?? string.Empty,
            DegreeLevel,
            Field ?? string.Empty,
            Language ?? string.Empty,
            DurationMonths,
            SourceUrl ?? string.Empty,
            CheckedOn);
}
