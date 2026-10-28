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
    DateOnly CheckedOn)
{
    internal Cost ToCost() =>
        Cost.Create(AcademicYear, TuitionMinor, LivingMinor, Currencies.Normalize(Currency), SourceUrl.Trim(), CheckedOn);
}

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

        university.ReplaceCosts(request.Costs.Select(cost => cost.ToCost()), currentUser.UserId, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
