namespace FourPrep.Modules.Catalogue.Presentation.Programs.Requests;

internal sealed record ReplaceProgramDeadlinesRequest
{
    public IReadOnlyList<DeadlineRequest>? Deadlines { get; init; }
}

internal sealed record DeadlineRequest
{
    public string? Round { get; init; }
    public DateOnly DueDate { get; init; }
    public string? SourceUrl { get; init; }
    public DateOnly CheckedOn { get; init; }
}
