namespace FourPrep.Modules.Admission.Domain.AcademyApplications;

using FourPrep.Common.Domain;
using FourPrep.Modules.Admission.Domain.ReportedScores;

public sealed class AcademyApplication
{
    private readonly List<ReportedScore> _scores = [];

    public bool IsOpen => Status is AcademyApplicationStatus.Submitted or AcademyApplicationStatus.Accepted;

    public Guid Id { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public AcademyApplicationStatus Status { get; set; }
    public List<ApplicationInterest> Interests { get; set; } = [];
    public IReadOnlyCollection<ReportedScore> Scores => _scores;
    public List<ContactChannel> ContactChannels { get; set; } = [];
    public string? PhoneNumber { get; set; }
    public string? TelegramUsername { get; set; }
    public string MeetingLanguage { get; set; } = string.Empty;
    public string? Question { get; set; }
    public bool ParentConsentConfirmed { get; set; }
    public DateTimeOffset SubmittedOnUtc { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedOnUtc { get; set; }
    public string? DeclineReason { get; set; }
    public DateTimeOffset? WithdrawnOnUtc { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static AcademyApplication Create(
        Guid studentUserId,
        string studentName,
        IEnumerable<ApplicationInterest> interests,
        IEnumerable<ReportedScore> scores,
        IEnumerable<ContactChannel> contactChannels,
        string? phoneNumber,
        string? telegramUsername,
        string meetingLanguage,
        string? question,
        bool parentConsentConfirmed,
        DateTime utcNow)
    {
        var application = new AcademyApplication
        {
            Id = Guid.NewGuid(),
            StudentUserId = studentUserId,
            StudentName = studentName,
            Status = AcademyApplicationStatus.Submitted,
            Interests = [.. interests],
            ContactChannels = [.. contactChannels],
            PhoneNumber = phoneNumber,
            TelegramUsername = telegramUsername,
            MeetingLanguage = meetingLanguage,
            Question = question,
            ParentConsentConfirmed = parentConsentConfirmed,
            SubmittedOnUtc = utcNow,
            CreatedOnUtc = utcNow,
            UpdatedOnUtc = utcNow
        };

        application._scores.AddRange(scores);

        return application;
    }

    public Result Accept(Guid reviewedByUserId, DateTime utcNow)
    {
        if (Status != AcademyApplicationStatus.Submitted)
        {
            return Result.Failure(AcademyApplicationErrors.NotSubmitted);
        }

        Status = AcademyApplicationStatus.Accepted;
        ReviewedByUserId = reviewedByUserId;
        ReviewedOnUtc = utcNow;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public Result Decline(Guid reviewedByUserId, string reason, DateTime utcNow)
    {
        if (Status != AcademyApplicationStatus.Submitted)
        {
            return Result.Failure(AcademyApplicationErrors.NotSubmitted);
        }

        Status = AcademyApplicationStatus.Declined;
        ReviewedByUserId = reviewedByUserId;
        ReviewedOnUtc = utcNow;
        DeclineReason = reason.Trim();
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public Result Withdraw(DateTime utcNow)
    {
        if (Status != AcademyApplicationStatus.Submitted)
        {
            return Result.Failure(AcademyApplicationErrors.NotSubmitted);
        }

        Status = AcademyApplicationStatus.Withdrawn;
        WithdrawnOnUtc = utcNow;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }
}
