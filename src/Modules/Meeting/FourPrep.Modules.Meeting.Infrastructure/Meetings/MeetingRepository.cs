namespace FourPrep.Modules.Meeting.Infrastructure.Meetings;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Infrastructure.Database;

internal sealed class MeetingRepository(MeetingDbContext dbContext) : IMeetingRepository
{
    public async Task<Meeting?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Meetings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> HasStaffOverlapAsync(
        Guid staffUserId,
        DateTime startsOnUtc,
        DateTime endsOnUtc,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset start = startsOnUtc;
        DateTimeOffset end = endsOnUtc;

        return await dbContext.Meetings.AnyAsync(
            x => x.StaffUserId == staffUserId &&
                 x.Status == MeetingStatus.Scheduled &&
                 x.StartsOnUtc < end &&
                 start < x.StartsOnUtc.AddMinutes(x.DurationMinutes),
            cancellationToken);
    }

    public async Task<int> CountScheduledBookedByAsync(Guid bookedByUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Meetings.CountAsync(
            x => x.BookedByUserId == bookedByUserId && x.Status == MeetingStatus.Scheduled,
            cancellationToken);
    }

    public async Task<bool> HasScheduledConsultationAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Meetings.AnyAsync(
            x => x.StudentUserId == studentUserId &&
                 x.Kind == MeetingKind.Consultation &&
                 x.Status == MeetingStatus.Scheduled,
            cancellationToken);
    }

    public void Insert(Meeting meeting)
    {
        dbContext.Meetings.Add(meeting);
    }
}
