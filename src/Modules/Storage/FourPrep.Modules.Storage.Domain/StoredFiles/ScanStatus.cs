namespace FourPrep.Modules.Storage.Domain.StoredFiles;

public enum ScanStatus
{
    NotScanned = 0,
    Pending = 1,
    Clean = 2,
    Infected = 3,
    Failed = 4
}
