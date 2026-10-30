namespace FourPrep.Modules.Content.Application.NewsPosts.DeleteNewsPost;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;
using FourPrep.Modules.Content.Application.Abstractions.Storage;

public sealed record DeleteNewsPostCommand(Guid Id) : ICommand;

internal sealed class DeleteNewsPostCommandHandler(
    IUnitOfWork unitOfWork,
    IStorageService storageService,
    INewsPostRepository repository) : ICommandHandler<DeleteNewsPostCommand>
{
    public async Task<Result> Handle(DeleteNewsPostCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await repository.GetAsync(request.Id, cancellationToken);
        if (newsPost is null)
        {
            return Result.Failure(NewsPostErrors.NotFound);
        }

        if (newsPost.CoverFileId is { } coverFileId)
        {
            await storageService.UnpublishFileAsync(coverFileId, cancellationToken);
        }

        repository.Remove(newsPost);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
