namespace FourPrep.Modules.Catalogue.Domain.Universities;

public interface IUniversityRepository
{
    Task<University?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<University?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsSlugTakenAsync(string slug, Guid? exceptUniversityId, CancellationToken cancellationToken = default);

    void Insert(University university);

    void Remove(University university);
}
