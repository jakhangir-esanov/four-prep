namespace FourPrep.Modules.Content.Application.NewsPosts.UnpublishNewsPost;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;

public sealed record UnpublishNewsPostCommand(Guid Id) : ICommand;

internal sealed class UnpublishNewsPostCommandHandler(
    IUnitOfWork unitOfWork,
    INewsPostRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UnpublishNewsPostCommand>
{
    public async Task<Result> Handle(UnpublishNewsPostCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await repository.GetAsync(request.Id, cancellationToken);
        if (newsPost is null)
        {
            return Result.Failure(NewsPostErrors.NotFound);
        }

        var unpublished = newsPost.Unpublish(dateTimeProvider.UtcNow);
        if (unpublished.IsFailure)
        {
            return unpublished;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
