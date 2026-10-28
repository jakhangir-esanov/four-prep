namespace FourPrep.Modules.Catalogue.Application.Programs.GetMajors;

public sealed record MajorResponse
{
    public string Field { get; init; } = string.Empty;
    public int ProgramCount { get; init; }
}
