namespace FourPrep.Modules.Content.Application.NewsPosts.SetNewsPostCover;

using FourPrep.Common.Domain;
using Microsoft.Extensions.Logging;
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
    IDateTimeProvider dateTimeProvider,
    ILogger<SetNewsPostCoverCommandHandler> logger) : ICommandHandler<SetNewsPostCoverCommand>
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

        try
        {
            if (newsPost.CoverFileId is { } previousCoverFileId)
            {
                await storageService.UnpublishFileAsync(previousCoverFileId, cancellationToken);
            }

            newsPost.SetCover(request.FileId, publicUrl.Data, dateTimeProvider.UtcNow);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TakeOfflineUnlessInUseAsync(request.FileId, newsPost.Id);
            throw;
        }

        return Result.Success();
    }

    private async Task TakeOfflineUnlessInUseAsync(Guid fileId, Guid newsPostId)
    {
        try
        {
            if (await IsInUseElsewhereAsync(fileId, newsPostId))
            {
                return;
            }

            var unpublished = await storageService.UnpublishFileAsync(fileId, CancellationToken.None);
            if (unpublished.IsFailure)
            {
                logger.LogError(
                    "Cover {FileId} stays public after news post {NewsPostId} failed to save: {ErrorCode}",
                    fileId,
                    newsPostId,
                    unpublished.Error.Code);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Cover {FileId} stays public after news post {NewsPostId} failed to save",
                fileId,
                newsPostId);
        }
    }

    private async Task<bool> IsInUseElsewhereAsync(Guid fileId, Guid newsPostId)
    {
        try
        {
            return await repository.IsCoverInUseAsync(fileId, newsPostId, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not tell whether cover {FileId} belongs to another news post; taking it offline",
                fileId);
            return false;
        }
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
