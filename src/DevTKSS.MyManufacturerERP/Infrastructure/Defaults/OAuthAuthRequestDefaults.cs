namespace DevTKSS.MyManufacturerERP.Infrastructure.Defaults;

/// <summary>
/// oAuth ResponseValueCode Grant standarized keys <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1"/>
/// </summary>
public class OAuthAuthRequestDefaults
{
    public const string ResponseTypeKey = "response_type";
    /// <summary>
    /// REQUIRED.  Value MUST be set to "code".
    /// </summary>
    public const string ResponseValueCode = "code";
    /// <summary>
    /// REQUIRED.The client identifier as described in Section 2.2.
    /// </summary>
    public const string ClientIdKey = "client_id";
    /// <summary>
    /// OPTIONAL.  As described in Section <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2">3.1.2</see>.
    /// </summary>
    public const string RedirectUriKey = "redirect_uri";
    /// <summary>
    /// RECOMMENDED. An opaque value used by the client to maintain
    /// state between the request and callback.The authorization
    /// server includes this value when redirecting the user-agent back
    /// to the client.
    /// </summary>
    public const string StateKey = "state";
    /// <summary>
    /// OPTIONAL.  The scope of the access request as described by <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.3">Section 3.3</see>.
    /// </summary>
    public const string ScopeKey = "scope";
}
