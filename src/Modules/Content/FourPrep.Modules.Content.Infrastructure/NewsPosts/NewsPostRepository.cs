namespace FourPrep.Modules.Content.Infrastructure.NewsPosts;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Infrastructure.Database;

internal sealed class NewsPostRepository(ContentDbContext dbContext) : INewsPostRepository
{
    public async Task<NewsPost?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.NewsPosts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsSlugTakenAsync(
        string slug,
        Guid? exceptNewsPostId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NewsPosts.AnyAsync(
            x => x.Slug == slug && (exceptNewsPostId == null || x.Id != exceptNewsPostId),
            cancellationToken);
    }

    public async Task<bool> IsCoverInUseAsync(
        Guid fileId,
        Guid exceptNewsPostId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NewsPosts.AnyAsync(
            x => x.CoverFileId == fileId && x.Id != exceptNewsPostId,
            cancellationToken);
    }

    public void Insert(NewsPost newsPost)
    {
        dbContext.NewsPosts.Add(newsPost);
    }

    public void Remove(NewsPost newsPost)
    {
        dbContext.NewsPosts.Remove(newsPost);
    }
}
