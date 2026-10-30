namespace FourPrep.Modules.Content.Domain.NewsPosts;

public interface INewsPostRepository
{
    Task<NewsPost?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsSlugTakenAsync(string slug, Guid? exceptNewsPostId, CancellationToken cancellationToken = default);

    Task<bool> IsCoverInUseAsync(Guid fileId, Guid exceptNewsPostId, CancellationToken cancellationToken = default);

    void Insert(NewsPost newsPost);

    void Remove(NewsPost newsPost);
}
