namespace FourPrep.Modules.Content.Application.NewsPosts.UpdateNewsPost;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;

public sealed record UpdateNewsPostCommand(Guid Id, NewsPostDetails Details) : ICommand;

internal sealed class UpdateNewsPostCommandHandler(
    IUnitOfWork unitOfWork,
    INewsPostRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateNewsPostCommand>
{
    public async Task<Result> Handle(UpdateNewsPostCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await repository.GetAsync(request.Id, cancellationToken);
        if (newsPost is null)
        {
            return Result.Failure(NewsPostErrors.NotFound);
        }

        if (await repository.IsSlugTakenAsync(request.Details.NormalizedSlug, newsPost.Id, cancellationToken))
        {
            return Result.Failure(NewsPostErrors.SlugTaken);
        }

        request.Details.ApplyTo(newsPost, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
