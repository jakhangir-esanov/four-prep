namespace FourPrep.Modules.Catalogue.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 10, 23, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeTimeZoneContext : ITimeZoneContext
{
    public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;

    public DateOnly Today() => new(2026, 10, 23);

    public DateTime ToLocal(DateTime utc) => utc;
}

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeDestinationRepository : IDestinationRepository
{
    public List<Destination> Destinations { get; } = [];

    public Destination Add(string countryCode, string visaType = "F-1")
    {
        var destination = Destination.Create(countryCode, visaType, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Destinations.Add(destination);
        return destination;
    }

    public Task<Destination?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(Destinations.SingleOrDefault(x => x.CountryCode == countryCode));

    public void Insert(Destination destination) => Destinations.Add(destination);

    public void Remove(Destination destination) => Destinations.Remove(destination);
}

internal sealed class FakeUniversityRepository : IUniversityRepository
{
    public List<University> Universities { get; } = [];

    public University Add(string slug = "mit")
    {
        var university = University.Create(
            slug,
            "Massachusetts Institute of Technology",
            "US",
            "Cambridge",
            "https://www.mit.edu",
            null,
            null,
            null,
            false,
            [],
            "https://www.mit.edu/admissions",
            new DateOnly(2026, 1, 1),
            Guid.NewGuid(),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Universities.Add(university);
        return university;
    }

    public Task<University?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Universities.SingleOrDefault(x => x.Id == id));

    public Task<University?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(Universities.SingleOrDefault(x => x.Slug == slug));

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Universities.Any(x => x.Id == id));

    public Task<bool> IsSlugTakenAsync(string slug, Guid? exceptUniversityId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Universities.Any(x => x.Slug == slug && x.Id != exceptUniversityId));

    public void Insert(University university) => Universities.Add(university);

    public void Remove(University university) => Universities.Remove(university);
}

internal sealed class FakeProgramRepository : IProgramRepository
{
    public List<Program> Programs { get; } = [];

    public Program Add(Guid universityId, string name = "BSc Computer Science")
    {
        var program = Program.Create(
            universityId,
            name,
            Common.Domain.Enums.DegreeLevel.Bachelor,
            "Computer Science",
            "en",
            48,
            "https://www.mit.edu/programs/cs",
            new DateOnly(2026, 1, 1),
            Guid.NewGuid(),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Programs.Add(program);
        return program;
    }

    public Task<Program?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Programs.SingleOrDefault(x => x.Id == id));

    public Task<Program?> GetByNameAsync(Guid universityId, string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(Programs.SingleOrDefault(x => x.UniversityId == universityId && x.Name == name));

    public Task<bool> IsNameTakenAsync(
        Guid universityId,
        string name,
        Guid? exceptProgramId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Programs.Any(x => x.UniversityId == universityId && x.Name == name && x.Id != exceptProgramId));

    public void Insert(Program program) => Programs.Add(program);

    public void Remove(Program program) => Programs.Remove(program);
}

internal sealed class FakeCsvReader(params string[] lines) : ICsvReader
{
    private const char Separator = ',';
    private const int FirstDataRow = 2;

    public Error? Failure { get; init; }

    public Result<CsvTable> Read(Stream stream)
    {
        if (Failure is not null)
        {
            return Result.Failure<CsvTable>(Failure);
        }

        string[] columns = lines.Length == 0 ? [] : lines[0].Split(Separator);
        var rows = lines
            .Skip(1)
            .Select((line, index) => new CsvRow(
                index + FirstDataRow,
                columns.Zip(line.Split(Separator)).ToDictionary(cell => cell.First, cell => cell.Second)))
            .ToList();

        return new CsvTable(columns, rows);
    }
}
