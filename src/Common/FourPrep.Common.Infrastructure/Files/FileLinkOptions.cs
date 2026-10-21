namespace FourPrep.Common.Infrastructure.Files;

public sealed class FileLinkOptions
{
    public const string SectionName = "FileLinks";

    public string? SigningKey { get; init; }

    public int LifetimeHours { get; init; } = 6;

    public bool RequireSignedVideos { get; init; }
}
