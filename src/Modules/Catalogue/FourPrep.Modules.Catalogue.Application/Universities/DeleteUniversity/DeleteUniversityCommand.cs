namespace FourPrep.Modules.Catalogue.Application.Universities.DeleteUniversity;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;

public sealed record DeleteUniversityCommand(Guid Id) : ICommand;

internal sealed class DeleteUniversityCommandHandler(
    IUnitOfWork unitOfWork,
    IUniversityRepository repository) : ICommandHandler<DeleteUniversityCommand>
{
    public async Task<Result> Handle(DeleteUniversityCommand request, CancellationToken cancellationToken)
    {
        var university = await repository.GetAsync(request.Id, cancellationToken);
        if (university is null)
        {
            return Result.Failure(UniversityErrors.NotFound);
        }

        repository.Remove(university);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
