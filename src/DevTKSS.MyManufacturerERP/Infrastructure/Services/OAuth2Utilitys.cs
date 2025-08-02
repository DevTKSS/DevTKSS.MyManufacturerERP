using System.Security.Cryptography;
using System.Text;
using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

/// <summary>
/// Helper class for Etsy OAuth2 PKCE and token handling.
/// </summary>
public static class OAuth2Utilitys
{
    /// <summary>
    /// Generates a random state string for the OAuth2 flow.
    /// </summary>
    public static string GenerateState() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=')
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

    /// <summary>
    /// Sets the current token state and calculates the token expiry.
    /// </summary>
    public static void SetTokenState(ref TokenResponse? currentToken, ref string? refreshToken, ref DateTimeOffset? tokenExpiry, TokenResponse token)
    {
        currentToken = token;
        refreshToken = token.RefreshToken;
        tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn - 60);
    }
}
