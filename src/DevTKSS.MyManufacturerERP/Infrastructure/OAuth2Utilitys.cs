using System.Security.Cryptography;

namespace DevTKSS.MyManufacturerERP.Infrastructure;

/// <summary>
/// Helper class for Etsy OAuth2 PKCE and token handling.
/// </summary>
public static class OAuth2Utilitys
{
    /// <summary>
    /// Generates a random _state string for the OAuth2 flow.
    /// </summary>
    public static string GenerateState()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=')
        .Replace('+', '-').Replace('/', '_');

    /// <summary>
    /// Generates a random code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeVerifier() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=')
        .Replace('+', '-').Replace('/', '_');

    /// <summary>
    /// Generates a code challenge from the code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes).TrimEnd('=')
            .Replace('+', '-').Replace('/', '_');
    }
}
