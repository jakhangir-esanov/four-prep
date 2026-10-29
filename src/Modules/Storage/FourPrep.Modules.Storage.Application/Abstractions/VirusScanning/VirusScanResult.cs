namespace FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;

public sealed record VirusScanResult(bool IsClean, string? Threat)
{
    public static readonly VirusScanResult Clean = new(true, null);

    public static VirusScanResult Infected(string threat) => new(false, threat);
}
