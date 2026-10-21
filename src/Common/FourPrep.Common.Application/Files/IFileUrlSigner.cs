namespace FourPrep.Common.Application.Files;

public interface IFileUrlSigner
{
    bool VideosRequireSignature { get; }

    string ContentUrl(Guid fileId);

    string StreamUrl(Guid fileId);

    string PosterUrl(Guid fileId);

    bool IsValid(Guid fileId, long? expires, string? signature);
}
