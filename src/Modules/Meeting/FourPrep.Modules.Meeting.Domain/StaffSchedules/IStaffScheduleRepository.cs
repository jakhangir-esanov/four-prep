namespace FourPrep.Modules.Meeting.Domain.StaffSchedules;

public interface IStaffScheduleRepository
{
    Task<StaffSchedule?> GetByStaffUserIdAsync(Guid staffUserId, CancellationToken cancellationToken = default);

    void Insert(StaffSchedule schedule);
}
