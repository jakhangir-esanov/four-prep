namespace FourPrep.Modules.Student.Infrastructure.LinkCodes;

using System.Security.Cryptography;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;

internal sealed class LinkCodeGenerator : ILinkCodeGenerator
{
    private const string UnambiguousCharacters = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public string Next() => RandomNumberGenerator.GetString(UnambiguousCharacters, LinkCodeFormat.Length);
}
