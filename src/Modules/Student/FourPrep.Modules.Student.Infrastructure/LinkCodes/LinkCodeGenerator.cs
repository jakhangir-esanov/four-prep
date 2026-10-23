namespace FourPrep.Modules.Student.Infrastructure.LinkCodes;

using System.Security.Cryptography;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;

internal sealed class LinkCodeGenerator : ILinkCodeGenerator
{
    // No 0/O, 1/I/L: parents type the code by hand from the student's screen.
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public string Next() => RandomNumberGenerator.GetString(Alphabet, LinkCodeFormat.Length);
}
