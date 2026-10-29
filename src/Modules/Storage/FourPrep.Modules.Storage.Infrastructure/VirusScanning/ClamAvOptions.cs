namespace FourPrep.Modules.Storage.Infrastructure.VirusScanning;

public sealed class ClamAvOptions
{
    public const string SectionName = "Storage:ClamAv";

    public const string IncompleteMessage = "Storage:ClamAv needs Host and a Port between 1 and 65535.";

    private const int MaxPort = 65535;

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public int TimeoutSeconds { get; init; } = 60;

    public static bool IsComplete(ClamAvOptions options) =>
        !string.IsNullOrWhiteSpace(options.Host) && options.Port is > 0 and <= MaxPort && options.TimeoutSeconds > 0;
}
