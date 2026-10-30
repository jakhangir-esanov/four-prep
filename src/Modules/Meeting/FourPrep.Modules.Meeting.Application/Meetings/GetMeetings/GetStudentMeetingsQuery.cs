namespace FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record GetStudentMeetingsQuery(Guid StudentUserId) : IQuery<IReadOnlyList<MeetingResponse>>;

internal sealed class GetStudentMeetingsQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetStudentMeetingsQuery, IReadOnlyList<MeetingResponse>>
{
    public async Task<Result<IReadOnlyList<MeetingResponse>>> Handle(
        GetStudentMeetingsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await studentService.HasFamilyAccessAsync(currentUser.UserId, request.StudentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<IReadOnlyList<MeetingResponse>>(access.Error);
        }

        if (!access.Data)
        {
            return Result.Failure<IReadOnlyList<MeetingResponse>>(MeetingErrors.NotYourStudent);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var meetings = await MeetingList.ReadForStudentAsync(connection, request.StudentUserId, cancellationToken);

        return Result.Success<IReadOnlyList<MeetingResponse>>(
            [.. meetings.Select(meeting => meeting.ForFamily(currentUser.UserId))]);
    }
}
