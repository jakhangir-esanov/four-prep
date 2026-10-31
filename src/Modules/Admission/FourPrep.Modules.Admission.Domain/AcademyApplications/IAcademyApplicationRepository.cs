namespace FourPrep.Modules.Admission.Domain.AcademyApplications;

public interface IAcademyApplicationRepository
{
    Task<AcademyApplication?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasOpenAsync(Guid studentUserId, CancellationToken cancellationToken = default);

    void Insert(AcademyApplication application);
}
