namespace FourPrep.Modules.Catalogue.Application.Programs;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Programs;

public sealed record ProgramDetails(
    string Name,
    DegreeLevel DegreeLevel,
    string Field,
    string Language,
    int? DurationMonths,
    string SourceUrl,
    DateOnly CheckedOn)
{
    internal Program ToProgram(Guid universityId, Guid checkedByUserId, DateTime utcNow) =>
        Program.Create(
            universityId,
            Name.Trim(),
            DegreeLevel,
            Field.Trim(),
            NormalizedLanguage,
            DurationMonths,
            SourceUrl.Trim(),
            CheckedOn,
            checkedByUserId,
            utcNow);

    internal void ApplyTo(Program program, Guid checkedByUserId, DateTime utcNow) =>
        program.Update(
            Name.Trim(),
            DegreeLevel,
            Field.Trim(),
            NormalizedLanguage,
            DurationMonths,
            SourceUrl.Trim(),
            CheckedOn,
            checkedByUserId,
            utcNow);

    private string NormalizedLanguage => Language.Trim().ToLowerInvariant();
}
