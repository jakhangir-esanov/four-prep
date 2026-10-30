namespace FourPrep.Modules.Meeting.Application.Meetings;

internal static class MeetingLinks
{
    public const int MaxLength = 500;

    private const string ZoomHost = "zoom.us";
    private const string GoogleMeetHost = "meet.google.com";

    public static bool IsValid(string? url) =>
        url is not null &&
        Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps &&
        uri.IsDefaultPort &&
        string.IsNullOrEmpty(uri.UserInfo) &&
        IsMeetingHost(uri.IdnHost);

    private static bool IsMeetingHost(string host) =>
        host.Equals(GoogleMeetHost, StringComparison.OrdinalIgnoreCase) ||
        host.Equals(ZoomHost, StringComparison.OrdinalIgnoreCase) ||
        host.EndsWith($".{ZoomHost}", StringComparison.OrdinalIgnoreCase);

    public static string? Normalize(string? url) => string.IsNullOrWhiteSpace(url) ? null : url.Trim();
}
