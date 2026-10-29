namespace FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;

public interface IVirusScanner
{
    Task<VirusScanResult> ScanAsync(Stream content, CancellationToken cancellationToken = default);
}
