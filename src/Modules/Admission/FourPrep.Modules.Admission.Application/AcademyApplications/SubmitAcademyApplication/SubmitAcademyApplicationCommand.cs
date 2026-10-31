namespace FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Admission.Domain.ReportedScores;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Students;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public sealed record SubmitAcademyApplicationCommand(
    IReadOnlyList<ApplicationInterest> Interests,
    IReadOnlyList<ReportedScoreInput> Scores,
    IReadOnlyList<ContactChannel> ContactChannels,
    string? PhoneNumber,
    string? TelegramUsername,
    string MeetingLanguage,
    string? Question,
    bool ParentConsentConfirmed) : ICommand<Guid>;

internal sealed class SubmitAcademyApplicationCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IAcademyApplicationRepository repository,
    ITimeZoneContext timeZoneContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SubmitAcademyApplicationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SubmitAcademyApplicationCommand request, CancellationToken cancellationToken)
    {
        if (await repository.HasOpenAsync(currentUser.UserId, cancellationToken))
        {
            return Result.Failure<Guid>(AcademyApplicationErrors.AlreadyApplied);
        }

        var student = await studentService.GetStudentAsync(currentUser.UserId, cancellationToken);
        if (student.IsFailure)
        {
            return Result.Failure<Guid>(student.Error);
        }

        if (!request.ParentConsentConfirmed &&
            !StudentAge.IsKnownAdult(student.Data.DateOfBirth, timeZoneContext.Today()))
        {
            return Result.Failure<Guid>(AcademyApplicationErrors.ParentConsentRequired);
        }

        var application = Create(request, student.Data, dateTimeProvider.UtcNow);

        repository.Insert(application);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Id;
    }

    private static AcademyApplication Create(
        SubmitAcademyApplicationCommand request,
        AdmissionStudent student,
        DateTime utcNow)
    {
        var channels = request.ContactChannels;

        return AcademyApplication.Create(
            student.UserId,
            student.DisplayName,
            request.Interests,
            request.Scores.Select(score => ReportedScore.Create(score.Exam, score.Score)),
            channels,
            channels.Contains(ContactChannel.Phone) ? PhoneNumbers.NormalizeOrNull(request.PhoneNumber) : null,
            channels.Contains(ContactChannel.Telegram) ? TelegramUsernames.NormalizeOrNull(request.TelegramUsername) : null,
            Locales.Normalize(request.MeetingLanguage),
            string.IsNullOrWhiteSpace(request.Question) ? null : request.Question.Trim(),
            request.ParentConsentConfirmed,
            utcNow);
    }
}
