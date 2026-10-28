namespace FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;

using FourPrep.Modules.Academy.Domain.ClassGroups;

internal sealed record GetClassGroupsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public string? Course { get; init; }
    public ClassMode? Mode { get; init; }
    public ClassGroupStatus? Status { get; init; }
    public Guid? TeacherUserId { get; init; }
    public string? Search { get; init; }
}
