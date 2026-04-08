using System.Diagnostics.CodeAnalysis;
namespace DevTKSS.Extensions.OAuth.Requests;

/// <summary>
/// Represents an authentication navigation request that can control navigation visibility.
/// </summary>
/// <param name="StartUrl">The authorization start URL.</param>
/// <param name="CallbackUrl">The callback URL to handle the authorization response.</param>
/// <param name="NavigateVisible">Whether the navigation UI should be visible.</param>
public record AuthNavigationRequest(string StartUrl, string CallbackUrl, bool NavigateVisible = false)
    : WebAuthRequest(StartUrl, CallbackUrl)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthNavigationRequest"/> record from an existing <see cref="WebAuthRequest"/>.
    /// </summary>
    /// <param name="webAuthRequest">The web authentication request to copy values from.</param>
    /// <param name="navigateVisible">Whether the navigation UI should be visible aka interactive authorization flow.</param>
    public AuthNavigationRequest(WebAuthRequest webAuthRequest, bool navigateVisible = false, bool isLoopbackHost = false, bool isHttpOnly = false, bool isFixedPort = false)
        : this(webAuthRequest.StartUrl, webAuthRequest.CallbackUrl, navigateVisible)
    {
        if (string.IsNullOrWhiteSpace(CallbackUrl) || string.IsNullOrWhiteSpace(StartUrl))
        {
            throw new ArgumentException("StartUrl and CallbackUrl must be provided.", nameof(webAuthRequest));
        }

        if (!Uri.TryCreate(CallbackUrl, UriKind.Absolute, out var callback))
        {
            throw new ArgumentException("CallbackUrl must be a valid absolute URI.", nameof(webAuthRequest));
        }
        
        if (isLoopbackHost && !IsLoopbackHost(callback.Host))
        {
            throw new ArgumentException("For loopback host scenarios, CallbackUrl must use a loopback host (e.g. 'localhost' or '127.0.0.1' ).", nameof(webAuthRequest));
        }

        if (isHttpOnly && !string.Equals(callback.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only http loopback redirect URIs are supported.");
        }

        if (isFixedPort && callback.Port <= 0)
        {
            throw new ArgumentException("For fixed port scenarios, CallbackUrl must specify a valid port number.", nameof(webAuthRequest));
        }
    }

    public AuthorizationState AuthState { get; } = new AuthorizationState();

    /// <summary>
    /// Determines whether <paramref name="uri"/> matches the configured OAuth redirect URI.
    /// </summary>
    /// <param name="uri">The URI to test.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> is the callback URI; otherwise <see langword="false"/>.</returns>
    public bool IsCallbackUri(Uri? uri)
    {
        if (uri is null)
        {
            return false;
        }

        if (!Uri.TryCreate(CallbackUrl, UriKind.Absolute, out var expected))
        {
            return false;
        }

        if (uri.OriginalString.Contains(CallbackUrl, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return uri.Scheme.Equals(expected.Scheme, StringComparison.OrdinalIgnoreCase)
            && uri.Authority.Equals(expected.Authority, StringComparison.OrdinalIgnoreCase)
            && uri.AbsolutePath.Equals(expected.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsLoopbackHost(string host) =>
       string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
       string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Validates OAuth callback URI and returns query parameters if valid.
    /// </summary>
    /// <param name="callbackUri">The callback URI to validate.</param>
    /// <param name="parameters">Query parameters if validation succeeds; null otherwise.</param>
    /// <returns>True if callback is valid; false otherwise.</returns>

    public bool IsValidCallback(
        string? redirectUri, [NotNullWhen(true)]out IDictionary<string, string>? parameters)
    {
       parameters = null;
        if (string.IsNullOrWhiteSpace(redirectUri) || !Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var query = OAuth2Utilitys.GetQueryParameters(uri);
        if (query is not { Count: > 0 })
        {
            return false;
        }

        var isError = !query.TryGetErrorCode(out _);

        var isStateMatching = !query.TryGetState(out var state) && AuthState.IsValidState(state);
        
        parameters = query;
        return (isError || isStateMatching) == false;
    }
}
