namespace FourPrep.Modules.Admission.Infrastructure.AcademyApplications;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Admission.Infrastructure.Database;
using FourPrep.Modules.Admission.Domain.AcademyApplications;

internal sealed class AcademyApplicationRepository(AdmissionDbContext dbContext) : IAcademyApplicationRepository
{
    public async Task<AcademyApplication?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AcademyApplications.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> HasOpenAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AcademyApplications.AnyAsync(
            x => x.StudentUserId == studentUserId &&
                 (x.Status == AcademyApplicationStatus.Submitted || x.Status == AcademyApplicationStatus.Accepted),
            cancellationToken);
    }

    public void Insert(AcademyApplication application)
    {
        dbContext.AcademyApplications.Add(application);
    }
}
