namespace FourPrep.Modules.Content.Application.NewsPosts.CreateNewsPost;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;

public sealed record CreateNewsPostCommand(NewsPostDetails Details) : ICommand<Guid>;

internal sealed class CreateNewsPostCommandHandler(
    IUnitOfWork unitOfWork,
    INewsPostRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateNewsPostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateNewsPostCommand request, CancellationToken cancellationToken)
    {
        if (await repository.IsSlugTakenAsync(request.Details.NormalizedSlug, null, cancellationToken))
        {
            return Result.Failure<Guid>(NewsPostErrors.SlugTaken);
        }

        var newsPost = request.Details.ToNewsPost(dateTimeProvider.UtcNow);

        repository.Insert(newsPost);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newsPost.Id;
    }
}
