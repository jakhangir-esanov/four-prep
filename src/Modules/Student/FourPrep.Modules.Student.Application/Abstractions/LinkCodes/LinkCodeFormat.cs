namespace FourPrep.Modules.Student.Application.Abstractions.LinkCodes;

public static class LinkCodeFormat
{
    public const int Length = 8;

    public static string Normalize(string linkCode) => linkCode.Trim().ToUpperInvariant();
}
