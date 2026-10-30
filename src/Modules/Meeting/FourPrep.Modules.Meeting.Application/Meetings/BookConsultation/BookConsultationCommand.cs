namespace FourPrep.Modules.Meeting.Application.Meetings.BookConsultation;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Application.Abstractions.Auth;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record BookConsultationCommand(
    Guid StudentUserId,
    Guid StaffUserId,
    DateTime StartsOnUtc,
    MeetingMode Mode,
    string? PhoneNumber = null) : ICommand<Guid>;

internal sealed class BookConsultationCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuthService authService,
    IStudentService studentService,
    IMeetingRepository meetingRepository,
    IStaffScheduleRepository scheduleRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<BookConsultationCommand, Guid>
{
    private static readonly string[] ConsultantRoles = [Roles.Mentor, Roles.MentorAdmin, Roles.SuperAdmin];

    public async Task<Result<Guid>> Handle(BookConsultationCommand request, CancellationToken cancellationToken)
    {
        var student = await CheckStudentAsync(request, cancellationToken);
        if (student.IsFailure)
        {
            return Result.Failure<Guid>(student.Error);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var startsOnUtc = UtcTimes.From(request.StartsOnUtc);

        var schedule = await scheduleRepository.GetByStaffUserIdAsync(request.StaffUserId, cancellationToken);
        if (schedule is null ||
            !IsOffered(schedule, request.Mode, startsOnUtc, utcNow) ||
            !await IsConsultantAsync(schedule.StaffUserId, cancellationToken))
        {
            return Result.Failure<Guid>(MeetingErrors.SlotNotAvailable);
        }

        if (await meetingRepository.HasStaffOverlapAsync(
                schedule.StaffUserId,
                startsOnUtc,
                startsOnUtc + ConsultationSlots.Duration,
                cancellationToken))
        {
            return Result.Failure<Guid>(MeetingErrors.SlotTaken);
        }

        if (await meetingRepository.HasScheduledConsultationAsync(request.StudentUserId, cancellationToken))
        {
            return Result.Failure<Guid>(MeetingErrors.AlreadyBooked);
        }

        if (await meetingRepository.CountScheduledBookedByAsync(currentUser.UserId, cancellationToken) >= Meeting.MaxScheduledPerBooker)
        {
            return Result.Failure<Guid>(MeetingErrors.TooManyBooked);
        }

        var meeting = Book(request, student.Data, schedule, startsOnUtc, utcNow);

        meetingRepository.Insert(meeting);
        schedule.RecordBooking(utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return meeting.Id;
    }

    private async Task<Result<MeetingStudent>> CheckStudentAsync(
        BookConsultationCommand request,
        CancellationToken cancellationToken)
    {
        var access = await studentService.HasFamilyAccessAsync(currentUser.UserId, request.StudentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<MeetingStudent>(access.Error);
        }

        if (!access.Data)
        {
            return Result.Failure<MeetingStudent>(MeetingErrors.NotYourStudent);
        }

        var student = await studentService.GetStudentAsync(request.StudentUserId, cancellationToken);
        if (student.IsSuccess && student.Data.MentorUserId is { } mentorUserId && mentorUserId != request.StaffUserId)
        {
            return Result.Failure<MeetingStudent>(MeetingErrors.MentorOnly);
        }

        return student;
    }

    private async Task<bool> IsConsultantAsync(Guid staffUserId, CancellationToken cancellationToken)
    {
        var roles = await authService.GetUserRolesAsync(staffUserId, cancellationToken);

        return roles.IsSuccess && roles.Data.Any(ConsultantRoles.Contains);
    }

    private static bool IsOffered(StaffSchedule schedule, MeetingMode mode, DateTime startsOnUtc, DateTime utcNow) =>
        ConsultationSlots.IsBookableAt(startsOnUtc, utcNow) &&
        ConsultationSlots.IsOffered(
            schedule.TimeZone,
            [.. schedule.Windows.Select(window => new WeeklyWindow(window.Day, window.StartTime, window.EndTime, window.Mode))],
            mode,
            startsOnUtc);

    private Meeting Book(
        BookConsultationCommand request,
        MeetingStudent student,
        StaffSchedule schedule,
        DateTime startsOnUtc,
        DateTime utcNow)
    {
        var isOnline = request.Mode == MeetingMode.Online;
        var isOffice = request.Mode == MeetingMode.Office;

        return Meeting.BookConsultation(
            request.Mode,
            request.StudentUserId,
            student.DisplayName,
            request.Mode == MeetingMode.Phone ? PhoneNumbers.NormalizeOrNull(request.PhoneNumber) : null,
            currentUser.UserId,
            schedule.StaffUserId,
            schedule.DisplayName,
            startsOnUtc,
            isOnline ? schedule.MeetingUrl : null,
            isOffice ? schedule.OfficeCountryCode : null,
            isOffice ? schedule.OfficeCity : null,
            isOffice ? schedule.OfficeAddress : null,
            utcNow);
    }
}
