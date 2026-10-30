namespace FourPrep.Modules.Content.UnitTests.NewsPosts;

using FourPrep.Common.Domain;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.NewsPosts.SetNewsPostCover;
using FourPrep.Modules.Content.Application.NewsPosts.RemoveNewsPostCover;

public class NewsPostCoverTests
{
    private static readonly Guid EditorId = Guid.NewGuid();

    private static readonly DateTime Now = new FakeClock().UtcNow;

    [Fact]
    public async Task SetCover_PublishesTheImageAndKeepsItsPublicUrl()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var file = storage.AddFile(EditorId);

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(file.Id, newsPost.CoverFileId);
        Assert.Equal(FakeStorageService.PublicUrl(file.Id), newsPost.CoverUrl);
        Assert.Contains(file.Id, storage.Published);
        Assert.Equal(Now, newsPost.UpdatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetCover_AcceptsAJpeg()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var file = storage.AddFile(EditorId, "image/jpeg");

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SetCover_TakesThePreviousCoverOffline()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var previous = storage.AddFile(EditorId);
        newsPost.SetCover(previous.Id, FakeStorageService.PublicUrl(previous.Id), Now.AddDays(-1));
        var file = storage.AddFile(EditorId);

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(file.Id, newsPost.CoverFileId);
        Assert.Equal([previous.Id], storage.Unpublished);
    }

    [Fact]
    public async Task SetCover_DoesNothingWhenTheImageIsAlreadyTheCover()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var file = storage.AddFile(EditorId);
        newsPost.SetCover(file.Id, FakeStorageService.PublicUrl(file.Id), Now.AddDays(-1));

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(storage.Published);
        Assert.Empty(storage.Unpublished);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetCover_RefusesAFileSomebodyElseUploaded()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var file = storage.AddFile(Guid.NewGuid());

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.Equal(NewsPostErrors.CoverNotFound, result.Error);
        AssertNothingChanged(newsPost, storage, unitOfWork);
    }

    [Fact]
    public async Task SetCover_RefusesAnUnknownFile()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, Guid.NewGuid()), default);

        Assert.Equal(NewsPostErrors.CoverNotFound, result.Error);
        AssertNothingChanged(newsPost, storage, unitOfWork);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/heic")]
    public async Task SetCover_RefusesAFileThatIsNotAWebImage(string contentType)
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var file = storage.AddFile(EditorId, contentType);

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.Equal(NewsPostErrors.CoverNotAnImage, result.Error);
        AssertNothingChanged(newsPost, storage, unitOfWork);
    }

    [Fact]
    public async Task SetCover_RefusesTheCoverOfAnotherPost()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var file = storage.AddFile(EditorId);
        newsPosts.Add("other-news").SetCover(file.Id, FakeStorageService.PublicUrl(file.Id), Now.AddDays(-1));
        var newsPost = newsPosts.Add();

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.Equal(NewsPostErrors.CoverInUse, result.Error);
        AssertNothingChanged(newsPost, storage, unitOfWork);
    }

    [Fact]
    public async Task SetCover_PassesOnStorageRefusals()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var file = storage.AddFile(EditorId);
        var notScanned = Error.Conflict("StoredFile.NotScanned", "Still scanning.", "Tekshirilmoqda.", "Проверяется.");
        storage.PublishFailure = notScanned;

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default);

        Assert.Equal(notScanned, result.Error);
        AssertNothingChanged(newsPost, storage, unitOfWork);
    }

    [Fact]
    public async Task SetCover_ReportsAnUnknownPost()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var file = storage.AddFile(EditorId);

        var result = await SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(Guid.NewGuid(), file.Id), default);

        Assert.Equal(NewsPostErrors.NotFound, result.Error);
        Assert.Empty(storage.Published);
    }

    [Fact]
    public async Task RemoveCover_ClearsTheCoverAndTakesItOffline()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var coverFileId = Guid.NewGuid();
        newsPost.SetCover(coverFileId, FakeStorageService.PublicUrl(coverFileId), Now.AddDays(-1));

        var result = await new RemoveNewsPostCoverCommandHandler(unitOfWork, storage, newsPosts, new FakeClock())
            .Handle(new RemoveNewsPostCoverCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Null(newsPost.CoverFileId);
        Assert.Null(newsPost.CoverUrl);
        Assert.Equal([coverFileId], storage.Unpublished);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task RemoveCover_KeepsTheCoverWhenThePublicCopyCannotBeDeleted()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var coverFileId = Guid.NewGuid();
        newsPost.SetCover(coverFileId, FakeStorageService.PublicUrl(coverFileId), Now.AddDays(-1));
        storage.UnpublishException = new IOException("storage is down");

        await Assert.ThrowsAsync<IOException>(() =>
            new RemoveNewsPostCoverCommandHandler(unitOfWork, storage, newsPosts, new FakeClock())
                .Handle(new RemoveNewsPostCoverCommand(newsPost.Id), default));

        Assert.Equal(coverFileId, newsPost.CoverFileId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetCover_KeepsThePreviousCoverWhenItsPublicCopyCannotBeDeleted()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var previous = storage.AddFile(EditorId);
        newsPost.SetCover(previous.Id, FakeStorageService.PublicUrl(previous.Id), Now.AddDays(-1));
        var file = storage.AddFile(EditorId);
        storage.UnpublishException = new IOException("storage is down");

        await Assert.ThrowsAsync<IOException>(() =>
            SetHandler(newsPosts, storage, unitOfWork).Handle(new SetNewsPostCoverCommand(newsPost.Id, file.Id), default));

        Assert.Equal(previous.Id, newsPost.CoverFileId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task RemoveCover_IsHarmlessForAPostWithoutCover()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();

        var result = await new RemoveNewsPostCoverCommandHandler(unitOfWork, storage, newsPosts, new FakeClock())
            .Handle(new RemoveNewsPostCoverCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(storage.Unpublished);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task RemoveCover_ReportsAnUnknownPost()
    {
        var (newsPosts, storage, unitOfWork) = Build();

        var result = await new RemoveNewsPostCoverCommandHandler(unitOfWork, storage, newsPosts, new FakeClock())
            .Handle(new RemoveNewsPostCoverCommand(Guid.NewGuid()), default);

        Assert.Equal(NewsPostErrors.NotFound, result.Error);
    }

    private static (FakeNewsPostRepository NewsPosts, FakeStorageService Storage, FakeUnitOfWork UnitOfWork) Build() =>
        (new FakeNewsPostRepository(), new FakeStorageService(), new FakeUnitOfWork());

    private static SetNewsPostCoverCommandHandler SetHandler(
        FakeNewsPostRepository newsPosts,
        FakeStorageService storage,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(EditorId), storage, newsPosts, new FakeClock());

    private static void AssertNothingChanged(NewsPost newsPost, FakeStorageService storage, FakeUnitOfWork unitOfWork)
    {
        Assert.Null(newsPost.CoverFileId);
        Assert.Empty(storage.Published);
        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
