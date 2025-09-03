using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Web;

namespace DevTKSS.Extensions.OAuth;

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
    public static string GenerateCodeVerifier()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=')
        .Replace('+', '-').Replace('/', '_');

    /// <summary>
    /// Generates a code challenge from the code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        var challengeBytes = SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes).TrimEnd('=')
            .Replace('+', '-').Replace('/', '_');
    }
    public static NameValueCollection GetQuery(this string? redirectUri, string callbackUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
            return [];
        return redirectUri.StartsWith(callbackUri)
             ? AuthHttpUtility.ExtractArguments(redirectUri)  // authData is a fully qualified url, so need to extract query or fragment
             : AuthHttpUtility.ParseQueryString(redirectUri.TrimStart('#').TrimStart('?')); // authData isn't full url, so just process as query or fragment

    }
}
