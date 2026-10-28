namespace FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramDeadlines;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Deadlines;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record DeadlineInput(string Round, DateOnly DueDate, string SourceUrl, DateOnly CheckedOn)
{
    internal Deadline ToDeadline() => Deadline.Create(Round.Trim(), DueDate, SourceUrl.Trim(), CheckedOn);
}

public sealed record ReplaceProgramDeadlinesCommand(Guid ProgramId, IReadOnlyList<DeadlineInput> Deadlines) : ICommand;

internal sealed class ReplaceProgramDeadlinesCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IProgramRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ReplaceProgramDeadlinesCommand>
{
    public async Task<Result> Handle(ReplaceProgramDeadlinesCommand request, CancellationToken cancellationToken)
    {
        var program = await repository.GetAsync(request.ProgramId, cancellationToken);
        if (program is null)
        {
            return Result.Failure(ProgramErrors.NotFound);
        }

        program.ReplaceDeadlines(
            request.Deadlines.Select(deadline => deadline.ToDeadline()),
            currentUser.UserId,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
