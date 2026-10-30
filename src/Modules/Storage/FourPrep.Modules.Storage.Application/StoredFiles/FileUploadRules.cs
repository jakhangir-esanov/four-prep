namespace FourPrep.Modules.Storage.Application.StoredFiles;

using FourPrep.Modules.Storage.Domain.StoredFiles;

internal static class FileUploadRules
{
    public const int HeaderLength = 16;

    public const long MaxSizeBytes = 20L * 1024L * 1024L;

    private const string Pdf = "application/pdf";
    private const string Jpeg = "image/jpeg";
    private const string Png = "image/png";
    private const string Heic = "image/heic";

    private static readonly Dictionary<string, string[]> ExtensionsByType = new(StringComparer.OrdinalIgnoreCase)
    {
        [Pdf] = [".pdf"],
        [Jpeg] = [".jpg", ".jpeg"],
        [Png] = [".png"],
        [Heic] = [".heic"]
    };

    private static readonly string[] PublicMediaTypes = [Jpeg, Png];

    private static readonly string[] HeicBrands = ["heic", "heix", "heim", "heis", "mif1", "msf1"];

    public static bool IsAllowedType(string? contentType) =>
        contentType is not null && ExtensionsByType.ContainsKey(contentType.Trim());

    public static bool IsAllowedFor(StoredFilePurpose purpose, string? contentType) =>
        IsAllowedType(contentType) &&
        (purpose != StoredFilePurpose.PublicMedia || PublicMediaTypes.Contains(contentType!.Trim(), StringComparer.OrdinalIgnoreCase));

    public static bool MatchesExtension(string? fileName, string? contentType) =>
        IsAllowedType(contentType) &&
        !string.IsNullOrWhiteSpace(fileName) &&
        ExtensionsByType[contentType!.Trim()].Contains(Path.GetExtension(fileName.Trim()), StringComparer.OrdinalIgnoreCase);

    public static bool MatchesSignature(string contentType, ReadOnlySpan<byte> header) => contentType switch
    {
        Pdf => StartsWith(header, "%PDF-"u8),
        Jpeg => StartsWith(header, [0xFF, 0xD8, 0xFF]),
        Png => StartsWith(header, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        Heic => IsHeic(header),
        _ => false
    };

    public static string NormalizedType(string contentType) => contentType.Trim().ToLowerInvariant();

    public static string SafeFileName(string fileName) => Path.GetFileName(fileName.Trim());

    private static bool IsHeic(ReadOnlySpan<byte> header) =>
        header.Length >= 12 &&
        header[4..8].SequenceEqual("ftyp"u8) &&
        HeicBrands.Contains(System.Text.Encoding.ASCII.GetString(header[8..12]));

    private static bool StartsWith(ReadOnlySpan<byte> header, ReadOnlySpan<byte> prefix) =>
        header.Length >= prefix.Length && header[..prefix.Length].SequenceEqual(prefix);
}
