namespace FourPrep.Modules.Catalogue.Application.Programs.CreateProgram;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record CreateProgramCommand(Guid UniversityId, ProgramDetails Details) : ICommand<Guid>;

internal sealed class CreateProgramCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IProgramRepository programRepository,
    IUniversityRepository universityRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateProgramCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProgramCommand request, CancellationToken cancellationToken)
    {
        if (!await universityRepository.ExistsAsync(request.UniversityId, cancellationToken))
        {
            return Result.Failure<Guid>(UniversityErrors.NotFound);
        }

        var program = request.Details.ToProgram(request.UniversityId, currentUser.UserId, dateTimeProvider.UtcNow);

        programRepository.Insert(program);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return program.Id;
    }
}
