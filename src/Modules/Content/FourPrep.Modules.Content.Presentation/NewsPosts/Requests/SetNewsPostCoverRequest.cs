namespace FourPrep.Modules.Content.Presentation.NewsPosts.Requests;

internal sealed record SetNewsPostCoverRequest
{
    public Guid FileId { get; init; }
}
