using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DevTKSS.Extensions.OAuth;

/// <summary>
/// Helper class for Etsy OAuth2 PKCE and token handling.
/// </summary>
public static partial class OAuth2Utilitys
{
    /// <summary>
    /// Generates a random code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeVerifier(int bytes = 32)
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes)).TrimEnd('=')
        .Replace('+', '-').Replace('/', '_');

    /// <summary>
    /// Generates a code challenge from the code verifier for PKCE.
    /// </summary>
    public static string? GenerateCodeChallenge([NotNullIfNotNull(nameof(codeVerifier))]string codeVerifier)
    {
        return string.IsNullOrEmpty(codeVerifier) ? null : Convert.ToBase64String(CreateSha256HashBytes(codeVerifier));
    }
    /// <summary>
    /// Computes the SHA-256 hash of the specified code verifier and returns the result as a byte array.
    /// </summary>
    /// <remarks>The code verifier is encoded using ASCII before hashing. This method is commonly used in PKCE
    /// (Proof Key for Code Exchange) flows to generate a code challenge.</remarks>
    /// <param name="codeVerifier">The code verifier string to hash. Cannot be null or empty.</param>
    /// <returns>A byte array containing the SHA-256 hash of the code verifier.</returns>
    public static byte[] CreateSha256HashBytes(string codeVerifier)
    {
        using (var challengeBytes = SHA256.Create())
        {
            return challengeBytes.ComputeHash(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
        }
    }
    // Regex source generator: parses key=value pairs in a query string. Last value wins on duplicates.
    [GeneratedRegex(@"([^?&=]+)=([^&]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking)]
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
                Uri.UnescapeDataString(m.Groups[1].Value),
                Uri.UnescapeDataString(m.Groups[2].Value)))
            .GroupBy(kv => kv.Key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.Ordinal);
    }
}
