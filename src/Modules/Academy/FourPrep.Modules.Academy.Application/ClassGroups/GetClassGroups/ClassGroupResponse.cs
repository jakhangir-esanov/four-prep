namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

using FourPrep.Modules.Academy.Domain.ClassGroups;

public sealed record ClassGroupResponse
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseSlug { get; init; } = string.Empty;
    public Dictionary<string, string> CourseName { get; init; } = [];
    public string Code { get; init; } = string.Empty;
    public ClassMode Mode { get; init; }
    public string? CountryCode { get; init; }
    public string? City { get; init; }
    public string? Address { get; init; }
    public Dictionary<string, string> EntryLevel { get; init; } = [];
    public string TimeZone { get; init; } = string.Empty;
    public DateOnly StartsOn { get; init; }
    public int Weeks { get; init; }
    public int Capacity { get; init; }
    public ClassGroupStatus Status { get; init; }
    public Guid? TeacherUserId { get; init; }
    public string? TeacherName { get; init; }
    public IReadOnlyList<ScheduleSlotResponse> Schedule { get; init; } = [];
    public DateTime CreatedOnUtc { get; init; }

    internal ClassGroupResponse ForPublic() => this with { TeacherUserId = null };
}

public sealed record ScheduleSlotResponse
{
    public DayOfWeek Day { get; init; }
    public TimeOnly StartTime { get; init; }
    public int DurationMinutes { get; init; }
}
