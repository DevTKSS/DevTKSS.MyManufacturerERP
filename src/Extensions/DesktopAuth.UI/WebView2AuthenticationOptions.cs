namespace DevTKSS.Extensions.Uno.Authentication.Desktop.UI;

public record WebView2AuthenticationOptions
{
    /// <summary>
    /// Gets or sets the URI to navigate to for starting the authentication process.
    /// </summary>
    public Uri? StartUri { get; init; }

    /// <summary>
    /// Gets or sets the URI that indicates the end of the authentication process.
    /// </summary>
    public Uri? EndUri { get; init; }

    /// <summary>
    /// Expected OAuth2 state that must match the callback.
    /// </summary>
    public string? ExpectedState { get; init; }

    /// <summary>
    /// PKCE code verifier used for the authorization request.
    /// </summary>
    public string? CodeVerifier { get; init; }
}
