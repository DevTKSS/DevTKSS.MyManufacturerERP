namespace DevTKSS.Extensions.OAuth.Requests;

/// <summary>
/// Represents a web authentication request containing the authorization start URL and callback URL.
/// </summary>
/// <param name="StartUrl">The authorization start URL.</param>
/// <param name="CallbackUrl">The callback URL to handle the authorization response.</param>
public record WebAuthRequest(string StartUrl, string CallbackUrl); // Equals the WebAuthenticationRequest in Uno's WebAuthenticationBrokerProvider.AuthenticateAsync, which has no implemenentation on Desktop Platform (Hosted on Windows) by now

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
    public AuthNavigationRequest(WebAuthRequest webAuthRequest, bool navigateVisible = false)
        : this(webAuthRequest.StartUrl, webAuthRequest.CallbackUrl, navigateVisible)
    {
    }
}
