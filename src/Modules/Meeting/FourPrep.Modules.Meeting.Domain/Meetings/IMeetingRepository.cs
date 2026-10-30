namespace FourPrep.Modules.Meeting.Domain.Meetings;

public interface IMeetingRepository
{
    Task<Meeting?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasStaffOverlapAsync(
        Guid staffUserId,
        DateTime startsOnUtc,
        DateTime endsOnUtc,
        CancellationToken cancellationToken = default);

    Task<int> CountScheduledBookedByAsync(Guid bookedByUserId, CancellationToken cancellationToken = default);

    Task<bool> HasScheduledConsultationAsync(Guid studentUserId, CancellationToken cancellationToken = default);

    void Insert(Meeting meeting);
}
