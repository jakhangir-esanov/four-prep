namespace FourPrep.Modules.Content.Application.NewsPosts.RemoveNewsPostCover;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;
using FourPrep.Modules.Content.Application.Abstractions.Storage;

public sealed record RemoveNewsPostCoverCommand(Guid Id) : ICommand;

internal sealed class RemoveNewsPostCoverCommandHandler(
    IUnitOfWork unitOfWork,
    IStorageService storageService,
    INewsPostRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveNewsPostCoverCommand>
{
    public async Task<Result> Handle(RemoveNewsPostCoverCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await repository.GetAsync(request.Id, cancellationToken);
        if (newsPost is null)
        {
            return Result.Failure(NewsPostErrors.NotFound);
        }

        if (newsPost.CoverFileId is not { } coverFileId)
        {
            return Result.Success();
        }

        await storageService.UnpublishFileAsync(coverFileId, cancellationToken);

        newsPost.RemoveCover(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
