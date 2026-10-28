namespace FourPrep.Modules.Catalogue.Application.Programs.DeleteProgram;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;

public sealed record DeleteProgramCommand(Guid Id) : ICommand;

internal sealed class DeleteProgramCommandHandler(
    IUnitOfWork unitOfWork,
    IProgramRepository repository) : ICommandHandler<DeleteProgramCommand>
{
    public async Task<Result> Handle(DeleteProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await repository.GetAsync(request.Id, cancellationToken);
        if (program is null)
        {
            return Result.Failure(ProgramErrors.NotFound);
        }

        repository.Remove(program);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
