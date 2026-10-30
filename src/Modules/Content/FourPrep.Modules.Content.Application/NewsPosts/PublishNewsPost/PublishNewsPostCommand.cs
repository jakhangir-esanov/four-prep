namespace FourPrep.Modules.Content.Application.NewsPosts.PublishNewsPost;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;

public sealed record PublishNewsPostCommand(Guid Id) : ICommand;

internal sealed class PublishNewsPostCommandHandler(
    IUnitOfWork unitOfWork,
    INewsPostRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<PublishNewsPostCommand>
{
    public async Task<Result> Handle(PublishNewsPostCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await repository.GetAsync(request.Id, cancellationToken);
        if (newsPost is null)
        {
            return Result.Failure(NewsPostErrors.NotFound);
        }

        var published = newsPost.Publish(dateTimeProvider.UtcNow);
        if (published.IsFailure)
        {
            return published;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
