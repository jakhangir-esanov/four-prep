namespace FourPrep.Modules.Auth.Application.Authentication;

using System.Text;
using System.Security.Cryptography;

internal static class AccessTokenHash
{
    public static string Of(string accessToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accessToken)));
}
