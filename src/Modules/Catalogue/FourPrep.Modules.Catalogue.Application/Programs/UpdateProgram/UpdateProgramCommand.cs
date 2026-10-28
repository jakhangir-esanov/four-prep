namespace FourPrep.Modules.Catalogue.Application.Programs.UpdateProgram;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record UpdateProgramCommand(Guid Id, ProgramDetails Details) : ICommand;

internal sealed class UpdateProgramCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IProgramRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateProgramCommand>
{
    public async Task<Result> Handle(UpdateProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await repository.GetAsync(request.Id, cancellationToken);
        if (program is null)
        {
            return Result.Failure(ProgramErrors.NotFound);
        }

        if (await repository.IsNameTakenAsync(program.UniversityId, request.Details.NormalizedName, program.Id, cancellationToken))
        {
            return Result.Failure(ProgramErrors.NameTaken);
        }

        request.Details.ApplyTo(program, currentUser.UserId, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
