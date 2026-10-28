namespace FourPrep.Modules.Catalogue.Application.Imports;

using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;

internal sealed class ImportTargets(IUniversityRepository universities, IProgramRepository programs)
{
    private readonly Dictionary<string, University?> _universitiesBySlug = new(StringComparer.Ordinal);

    public async Task<University?> FindUniversityAsync(string slug, CancellationToken cancellationToken)
    {
        if (!_universitiesBySlug.TryGetValue(slug, out var university))
        {
            university = await universities.GetBySlugAsync(slug, cancellationToken);
            _universitiesBySlug[slug] = university;
        }

        return university;
    }

    public async Task<Dictionary<ImportGroup<TInput>, University>> FindUniversitiesAsync<TInput>(
        IEnumerable<ImportGroup<TInput>> groups,
        ImportProblems problems,
        CancellationToken cancellationToken)
    {
        var found = new Dictionary<ImportGroup<TInput>, University>();

        foreach (var group in groups)
        {
            var university = await FindUniversityAsync(group.UniversitySlug, cancellationToken);
            if (university is null)
            {
                problems.Add(group.RowNumbers[0], ImportColumns.UniversitySlug, ImportErrors.UnknownUniversity);
                continue;
            }

            found[group] = university;
        }

        return found;
    }

    public async Task<Dictionary<ImportGroup<TInput>, Program>> FindProgramsAsync<TInput>(
        IEnumerable<ImportGroup<TInput>> groups,
        ImportProblems problems,
        CancellationToken cancellationToken)
    {
        var found = new Dictionary<ImportGroup<TInput>, Program>();

        foreach (var (group, university) in await FindUniversitiesAsync(groups, problems, cancellationToken))
        {
            var program = await programs.GetByNameAsync(university.Id, group.ProgramName ?? string.Empty, cancellationToken);
            if (program is null)
            {
                problems.Add(group.RowNumbers[0], ImportColumns.ProgramName, ImportErrors.UnknownProgram);
                continue;
            }

            found[group] = program;
        }

        return found;
    }
}
