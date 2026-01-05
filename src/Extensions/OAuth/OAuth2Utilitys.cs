using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DevTKSS.Extensions.OAuth;

/// <summary>
/// Helper class for Etsy OAuth2 PKCE and token handling.
/// </summary>
public static partial class OAuth2Utilitys
{
    #region Base64 URL Encoding for RFC 7636 PKCE
    /// <summary>
    /// Encodes the supplied octet sequence using Base64 URL encoding without padding along <see href="https://datatracker.ietf.org/doc/html/rfc7636#appendix-A">RFC 7636 Appendix A</see>.
    /// </summary>
    /// <param name="arg">Byte array to encode.</param>
    /// <returns>
    /// A Base64url-encoded string with no padding characters suitable for use as PKCE code_verifier or code_challenge values as described in RFC 7636 Appendix A.
    /// </returns>
    private static string Base64UrlEncode(byte[] arg)
    {
        return Convert.ToBase64String(arg) // Regular base64 encoder
                      .TrimEnd('=') // Remove any trailing '='s
                      .Replace('+', '-') // 62nd char of encoding
                      .Replace('/', '_'); // 63rd char of encoding
    }

    /// <summary>
    /// Computes the SHA-256 hash of the ASCII-encoded <paramref name="codeVerifier"/>.
    /// </summary>
    /// <param name="codeVerifier">The code verifier string to hash (ASCII).</param>
    /// <returns>Byte array containing the SHA-256 digest of the verifier suitable for Base64url encoding.</returns>
    /// <remarks>Implements BASE64URL-ENCODE(SHA256(ASCII(code_verifier))) as required by RFC 7636 (PKCE S256).</remarks>
    private static byte[] CreateSha256HashBytes(string codeVerifier)
    {
        return SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
    }
    #endregion

    #region Generate Code Verifier and Challenge
    /// <summary>
    /// Generates a random code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeVerifier(int bytes = 32)
        => Base64UrlEncode(RandomNumberGenerator.GetBytes(bytes));

    /// <summary>
    /// Generates a code challenge from the code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        return Base64UrlEncode(CreateSha256HashBytes(codeVerifier));
    }
    public static string GenerateState(int bytes = 16)
        => Base64UrlEncode(RandomNumberGenerator.GetBytes(bytes));
    #endregion

    #region Query Parameter Parsing
    // Regex source generator: parses key=value pairs in a query string. Last value wins on duplicates.
    [GeneratedRegex(@"([^?&=]+)=([^&]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex QueryParameterRegex();


    /// <summary>
    /// Parses the query parameters from the given <see cref="Uri"/> into a dictionary.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> from which to extract query parameters.</param>
    /// <returns>A <see cref="IDictionary{TKey, TValue}"/> containing the query parameters as key-value pairs.</returns>
    public static IDictionary<string, string> GetParameters(this Uri uri)
    {
        return QueryParameterRegex()
            .Matches(uri.Query)
            .Cast<Match>()
            .Select(m => new KeyValuePair<string, string>(
                System.Web.HttpUtility.UrlDecode(m.Groups[1].Value),
                System.Web.HttpUtility.UrlDecode(m.Groups[2].Value))
            )
            .GroupBy(kv => kv.Key, StringComparer.Ordinal)
            .ToDictionary(
            g => g.Key,
            g => g.Last().Value
            , StringComparer.Ordinal);
    }
    #endregion
}
