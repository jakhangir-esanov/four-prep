namespace FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Application.Facts;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Domain.AdmissionRequirements;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record AdmissionRequirementInput(
    IntakeTerm IntakeTerm,
    int IntakeYear,
    IReadOnlyDictionary<Exam, decimal>? EnglishMinimums,
    SatPolicy? SatPolicy,
    int? SatMidRangeLow,
    int? SatMidRangeHigh,
    decimal? MinimumGpa,
    decimal? GpaScale,
    IReadOnlyList<string>? Documents,
    int? EssaysCount,
    bool? InterviewRequired,
    string SourceUrl,
    DateOnly CheckedOn)
{
    internal AdmissionRequirement ToRequirement() =>
        AdmissionRequirement.Create(
            IntakeTerm,
            IntakeYear,
            new Dictionary<Exam, decimal>(EnglishMinimums ?? new Dictionary<Exam, decimal>()),
            SatPolicy,
            SatMidRangeLow,
            SatMidRangeHigh,
            MinimumGpa,
            GpaScale,
            [.. (Documents ?? []).Select(TextInput.TrimmedOrNull).OfType<string>()],
            EssaysCount,
            InterviewRequired,
            SourceUrl.Trim(),
            CheckedOn);
}

public sealed record ReplaceProgramRequirementsCommand(
    Guid ProgramId,
    IReadOnlyList<AdmissionRequirementInput> Requirements) : ICommand;

internal sealed class ReplaceProgramRequirementsCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IProgramRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ReplaceProgramRequirementsCommand>
{
    public async Task<Result> Handle(ReplaceProgramRequirementsCommand request, CancellationToken cancellationToken)
    {
        var program = await repository.GetAsync(request.ProgramId, cancellationToken);
        if (program is null)
        {
            return Result.Failure(ProgramErrors.NotFound);
        }

        program.ReplaceRequirements(
            request.Requirements.Select(requirement => requirement.ToRequirement()),
            currentUser.UserId,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
