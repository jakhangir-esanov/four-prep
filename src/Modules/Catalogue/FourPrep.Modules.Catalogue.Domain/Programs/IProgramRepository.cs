namespace FourPrep.Modules.Catalogue.Domain.Programs;

public interface IProgramRepository
{
    Task<Program?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Program?> GetByNameAsync(Guid universityId, string name, CancellationToken cancellationToken = default);

    Task<bool> IsNameTakenAsync(
        Guid universityId,
        string name,
        Guid? exceptProgramId,
        CancellationToken cancellationToken = default);

    void Insert(Program program);

    void Remove(Program program);
}
