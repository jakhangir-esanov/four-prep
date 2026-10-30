namespace FourPrep.Modules.Content.UnitTests.NewsPosts;

using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.NewsPosts.CreateNewsPost;
using FourPrep.Modules.Content.Application.NewsPosts.DeleteNewsPost;
using FourPrep.Modules.Content.Application.NewsPosts.UpdateNewsPost;
using FourPrep.Modules.Content.Application.NewsPosts.PublishNewsPost;
using FourPrep.Modules.Content.Application.NewsPosts.UnpublishNewsPost;

public class NewsPostCommandHandlerTests
{
    private static readonly DateTime Now = new FakeClock().UtcNow;

    [Fact]
    public async Task Create_AddsADraftWithNormalizedText()
    {
        var (newsPosts, _, unitOfWork) = Build();

        var result = await new CreateNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new CreateNewsPostCommand(NewsPostSamples.Details()), default);

        Assert.True(result.IsSuccess);
        var newsPost = Assert.Single(newsPosts.NewsPosts);
        Assert.Equal(result.Data, newsPost.Id);
        Assert.Equal("ielts-results-2026", newsPost.Slug);
        Assert.Equal("IELTS natijalari", newsPost.Title["uz"]);
        Assert.Equal("admissions", newsPost.Tag);
        Assert.Equal(NewsPostStatus.Draft, newsPost.Status);
        Assert.Null(newsPost.PublishedOnUtc);
        Assert.Equal(Now, newsPost.CreatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_RefusesATakenSlug()
    {
        var (newsPosts, _, unitOfWork) = Build();
        newsPosts.Add("ielts-results-2026");

        var result = await new CreateNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new CreateNewsPostCommand(NewsPostSamples.Details()), default);

        Assert.Equal(NewsPostErrors.SlugTaken, result.Error);
        Assert.Single(newsPosts.NewsPosts);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ChangesThePostAndKeepsItsStatus()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        newsPost.Publish(Now.AddDays(-1));

        var result = await new UpdateNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UpdateNewsPostCommand(newsPost.Id, NewsPostSamples.Details() with { Tag = null }), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("ielts-results-2026", newsPost.Slug);
        Assert.Null(newsPost.Tag);
        Assert.Equal(NewsPostStatus.Published, newsPost.Status);
        Assert.Equal(Now, newsPost.UpdatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_KeepsItsOwnSlug()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add("ielts-results-2026");

        var result = await new UpdateNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UpdateNewsPostCommand(newsPost.Id, NewsPostSamples.Details()), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Update_RefusesAnotherPostsSlug()
    {
        var (newsPosts, _, unitOfWork) = Build();
        newsPosts.Add("ielts-results-2026");
        var newsPost = newsPosts.Add();

        var result = await new UpdateNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UpdateNewsPostCommand(newsPost.Id, NewsPostSamples.Details()), default);

        Assert.Equal(NewsPostErrors.SlugTaken, result.Error);
        Assert.Equal("first-news", newsPost.Slug);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ReportsAnUnknownPost()
    {
        var (newsPosts, _, unitOfWork) = Build();

        var result = await new UpdateNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UpdateNewsPostCommand(Guid.NewGuid(), NewsPostSamples.Details()), default);

        Assert.Equal(NewsPostErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_RemovesThePostAndTakesItsCoverOffline()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var coverFileId = Guid.NewGuid();
        newsPost.SetCover(coverFileId, FakeStorageService.PublicUrl(coverFileId), Now);

        var result = await new DeleteNewsPostCommandHandler(unitOfWork, storage, newsPosts)
            .Handle(new DeleteNewsPostCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(newsPosts.NewsPosts);
        Assert.Equal([coverFileId], storage.Unpublished);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_KeepsThePostWhenItsCoverCannotBeTakenOffline()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        var coverFileId = Guid.NewGuid();
        newsPost.SetCover(coverFileId, FakeStorageService.PublicUrl(coverFileId), Now);
        storage.UnpublishException = new IOException("storage is down");

        await Assert.ThrowsAsync<IOException>(() =>
            new DeleteNewsPostCommandHandler(unitOfWork, storage, newsPosts)
                .Handle(new DeleteNewsPostCommand(newsPost.Id), default));

        Assert.Single(newsPosts.NewsPosts);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_LeavesStorageAloneForAPostWithoutCover()
    {
        var (newsPosts, storage, unitOfWork) = Build();
        var newsPost = newsPosts.Add();

        var result = await new DeleteNewsPostCommandHandler(unitOfWork, storage, newsPosts)
            .Handle(new DeleteNewsPostCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(storage.Unpublished);
    }

    [Fact]
    public async Task Delete_ReportsAnUnknownPost()
    {
        var (newsPosts, storage, unitOfWork) = Build();

        var result = await new DeleteNewsPostCommandHandler(unitOfWork, storage, newsPosts)
            .Handle(new DeleteNewsPostCommand(Guid.NewGuid()), default);

        Assert.Equal(NewsPostErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Publish_PublishesADraftWithTheCurrentTime()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add();

        var result = await new PublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new PublishNewsPostCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewsPostStatus.Published, newsPost.Status);
        Assert.Equal(Now, newsPost.PublishedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Publish_KeepsTheFirstPublicationDateWhenPublishedAgain()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        newsPost.Publish(Now.AddDays(-3));
        newsPost.Unpublish(Now.AddDays(-2));

        var result = await new PublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new PublishNewsPostCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now.AddDays(-3), newsPost.PublishedOnUtc);
    }

    [Fact]
    public async Task Publish_RefusesAPublishedPost()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        newsPost.Publish(Now.AddDays(-1));

        var result = await new PublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new PublishNewsPostCommand(newsPost.Id), default);

        Assert.Equal(NewsPostErrors.AlreadyPublished, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Publish_ReportsAnUnknownPost()
    {
        var (newsPosts, _, unitOfWork) = Build();

        var result = await new PublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new PublishNewsPostCommand(Guid.NewGuid()), default);

        Assert.Equal(NewsPostErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Unpublish_TurnsThePostBackIntoADraft()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add();
        newsPost.Publish(Now.AddDays(-1));

        var result = await new UnpublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UnpublishNewsPostCommand(newsPost.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewsPostStatus.Draft, newsPost.Status);
        Assert.Equal(Now.AddDays(-1), newsPost.PublishedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Unpublish_RefusesADraft()
    {
        var (newsPosts, _, unitOfWork) = Build();
        var newsPost = newsPosts.Add();

        var result = await new UnpublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UnpublishNewsPostCommand(newsPost.Id), default);

        Assert.Equal(NewsPostErrors.NotPublished, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Unpublish_ReportsAnUnknownPost()
    {
        var (newsPosts, _, unitOfWork) = Build();

        var result = await new UnpublishNewsPostCommandHandler(unitOfWork, newsPosts, new FakeClock())
            .Handle(new UnpublishNewsPostCommand(Guid.NewGuid()), default);

        Assert.Equal(NewsPostErrors.NotFound, result.Error);
    }

    private static (FakeNewsPostRepository NewsPosts, FakeStorageService Storage, FakeUnitOfWork UnitOfWork) Build() =>
        (new FakeNewsPostRepository(), new FakeStorageService(), new FakeUnitOfWork());
}
