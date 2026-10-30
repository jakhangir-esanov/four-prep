namespace FourPrep.Modules.Meeting.Infrastructure.StaffSchedules;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Infrastructure.Database;

internal sealed class StaffScheduleRepository(MeetingDbContext dbContext) : IStaffScheduleRepository
{
    public async Task<StaffSchedule?> GetByStaffUserIdAsync(Guid staffUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.StaffSchedules.SingleOrDefaultAsync(x => x.StaffUserId == staffUserId, cancellationToken);
    }

    public void Insert(StaffSchedule schedule)
    {
        dbContext.StaffSchedules.Add(schedule);
    }
}
