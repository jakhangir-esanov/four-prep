namespace FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Application.Facts;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Domain.Scholarships;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record ScholarshipInput(
    string Name,
    ScholarshipKind Kind,
    IReadOnlyDictionary<string, string>? Amount,
    IReadOnlyDictionary<string, string>? Eligibility,
    string SourceUrl,
    DateOnly CheckedOn)
{
    internal Scholarship ToScholarship() =>
        Scholarship.Create(
            Name.Trim(),
            Kind,
            LocalizedTexts.Normalize(Amount),
            LocalizedTexts.Normalize(Eligibility),
            SourceUrl.Trim(),
            CheckedOn);
}

public sealed record ReplaceUniversityScholarshipsCommand(
    Guid UniversityId,
    IReadOnlyList<ScholarshipInput> Scholarships) : ICommand;

internal sealed class ReplaceUniversityScholarshipsCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ReplaceUniversityScholarshipsCommand>
{
    public async Task<Result> Handle(ReplaceUniversityScholarshipsCommand request, CancellationToken cancellationToken)
    {
        var university = await repository.GetAsync(request.UniversityId, cancellationToken);
        if (university is null)
        {
            return Result.Failure(UniversityErrors.NotFound);
        }

        university.ReplaceScholarships(
            request.Scholarships.Select(scholarship => scholarship.ToScholarship()),
            currentUser.UserId,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
