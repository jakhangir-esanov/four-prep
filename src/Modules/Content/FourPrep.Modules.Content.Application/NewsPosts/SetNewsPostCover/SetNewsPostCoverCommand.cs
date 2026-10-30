namespace FourPrep.Modules.Content.Application.NewsPosts.SetNewsPostCover;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;
using FourPrep.Modules.Content.Application.Abstractions.Storage;
using FourPrep.Modules.Content.Application.Abstractions.Authentication;

public sealed record SetNewsPostCoverCommand(Guid Id, Guid FileId) : ICommand;

internal sealed class SetNewsPostCoverCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStorageService storageService,
    INewsPostRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SetNewsPostCoverCommand>
{
    private static readonly string[] CoverContentTypes = ["image/jpeg", "image/png"];

    public async Task<Result> Handle(SetNewsPostCoverCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await repository.GetAsync(request.Id, cancellationToken);
        if (newsPost is null)
        {
            return Result.Failure(NewsPostErrors.NotFound);
        }

        if (newsPost.CoverFileId == request.FileId)
        {
            return Result.Success();
        }

        var cover = await CheckCoverAsync(request.FileId, newsPost.Id, cancellationToken);
        if (cover.IsFailure)
        {
            return cover;
        }

        var publicUrl = await storageService.PublishFileAsync(request.FileId, cancellationToken);
        if (publicUrl.IsFailure)
        {
            return publicUrl;
        }

        if (newsPost.CoverFileId is { } previousCoverFileId)
        {
            await storageService.UnpublishFileAsync(previousCoverFileId, cancellationToken);
        }

        newsPost.SetCover(request.FileId, publicUrl.Data, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> CheckCoverAsync(Guid fileId, Guid newsPostId, CancellationToken cancellationToken)
    {
        var file = await storageService.GetFileAsync(fileId, cancellationToken);
        if (file.IsFailure || file.Data.UploadedByUserId != currentUser.UserId)
        {
            return Result.Failure(NewsPostErrors.CoverNotFound);
        }

        if (!CoverContentTypes.Contains(file.Data.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(NewsPostErrors.CoverNotAnImage);
        }

        return await repository.IsCoverInUseAsync(fileId, newsPostId, cancellationToken)
            ? Result.Failure(NewsPostErrors.CoverInUse)
            : Result.Success();
    }
}
