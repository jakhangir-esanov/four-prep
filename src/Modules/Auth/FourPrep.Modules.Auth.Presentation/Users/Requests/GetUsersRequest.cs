namespace FourPrep.Modules.Auth.Presentation.Users.Requests;

internal sealed record GetUsersRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public string? Role { get; init; }
}
