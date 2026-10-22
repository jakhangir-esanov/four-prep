namespace FourPrep.Modules.Auth.Presentation.UserSessions.Requests;

internal sealed record GetUserSessionsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public Guid? UserId { get; init; }
}
