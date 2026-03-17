namespace DevTKSS.Extensions.OAuth.Utils;

/// <summary>
/// Extensions for <see cref="WebAuthRequest"/> to handle URL matching and query parameter extraction.
/// </summary>
internal static class WebAuthRequestExtensions
{
    /// <summary>
    /// Checks if the destination URI matches the callback URL configured in the request.
    /// Allows partial matching to accommodate providers that add query/hash parameters.
    /// </summary>
    /// <param name="request">The web auth request containing the callback URL.</param>
    /// <param name="destination">The destination URI to check.</param>
    /// <returns>True if the destination matches the callback URL; otherwise false.</returns>
    internal static bool IsRedirectMatch(this WebAuthRequest? request, Uri? destination)
    {
        if (request is null || destination is null)
        {
            return false;
        }

        var redirectUri = request.CallbackUrl;
        if (string.IsNullOrWhiteSpace(redirectUri) || !Uri.TryCreate(redirectUri, UriKind.Absolute, out var expected))
        {
            return false;
        }

        // Some providers may add query/hash parts; allow a partial match first.
        if (destination.OriginalString.Contains(redirectUri, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return destination.Scheme.Equals(expected.Scheme, StringComparison.OrdinalIgnoreCase)
               && destination.Authority.Equals(expected.Authority, StringComparison.OrdinalIgnoreCase)
               && destination.AbsolutePath.Equals(expected.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }

}
