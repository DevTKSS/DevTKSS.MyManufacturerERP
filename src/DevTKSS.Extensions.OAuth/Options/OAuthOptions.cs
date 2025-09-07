namespace DevTKSS.Extensions.OAuth.Options;

// Inheriting here from Endpoint Options to get the possibility to use this Options directly with Refit client configuration in Uno also, but this isnt a pretty workaround
public class OAuthOptions : EndpointOptions
{
    public const string DefaultName = "OAuth";
    public string ProviderName { get; init; } = DefaultName;

    public new string? Url
    {
        get => base.Url;
        init
        {
            if (value == null)
            {
                base.Url = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Base address cannot be null or empty.", nameof(value));
            if (!value.EndsWith('/'))
                value += '/';
            if (!Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri) || (absoluteUri.Scheme != Uri.UriSchemeHttp && absoluteUri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("AuthorizationEndpoint must be a valid absolute URI with HTTP or HTTPS scheme.");
            base.Url = value;
        }
    }
    /// <summary>
    /// Gets the unique identifier for the client. Might be named as keystring at registration.
    /// </summary>
    public string? ClientID { get; init; }
    /// <summary>
    /// Gets the client secret used for authentication with the external service.
    /// </summary>
    /// <remarks>
    /// Not implemented so far, since not all OAuth providers use a client secret (e.g., public clients or mobile apps) as they are not able to keep them secret.<br/>
    /// Its used for confidential clients only which are used for the passwort flow and implicit flows for example, that do not require user interaction but verification of the calling application.
    /// </remarks>
    public string? ClientSecret { get; init; }
    public string[] Scopes { get; init; } = [];
    public AuthCallbackOptions? CallbackOptions { get; init; }
    public TokenCacheOptions TokenCacheOptions { get; init; } = new ();
    public OAuthEndpointOptions? EndpointOptions { get; init; }
    public TokenCacheKeyOptions TokenCacheKeys { get; init; } = new ();
}
