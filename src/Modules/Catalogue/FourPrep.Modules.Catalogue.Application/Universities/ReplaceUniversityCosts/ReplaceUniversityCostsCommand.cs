namespace FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Costs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record CostInput(
    int AcademicYear,
    long TuitionMinor,
    long? LivingMinor,
    string Currency,
    string SourceUrl,
    DateOnly CheckedOn);

public sealed record ReplaceUniversityCostsCommand(Guid UniversityId, IReadOnlyList<CostInput> Costs) : ICommand;

internal sealed class ReplaceUniversityCostsCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ReplaceUniversityCostsCommand>
{
    public async Task<Result> Handle(ReplaceUniversityCostsCommand request, CancellationToken cancellationToken)
    {
        var university = await repository.GetAsync(request.UniversityId, cancellationToken);
        if (university is null)
        {
            return Result.Failure(UniversityErrors.NotFound);
        }

        university.ReplaceCosts(
            request.Costs.Select(cost => Cost.Create(
                cost.AcademicYear,
                cost.TuitionMinor,
                cost.LivingMinor,
                Currencies.Normalize(cost.Currency),
                cost.SourceUrl.Trim(),
                cost.CheckedOn)),
            currentUser.UserId,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
