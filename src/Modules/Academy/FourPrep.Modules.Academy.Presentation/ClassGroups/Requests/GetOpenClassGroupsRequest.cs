namespace FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;

using FourPrep.Modules.Academy.Domain.ClassGroups;

internal sealed record GetOpenClassGroupsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public string? Course { get; init; }
    public ClassMode? Mode { get; init; }
}
