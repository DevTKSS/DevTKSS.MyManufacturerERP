namespace DevTKSS.Extensions.OAuth.Requests;

public record AuthorizationCodeRequest
{
    /// <summary>
    /// Value MUST be set to <see cref="OAuthDefaults.Values.Code">code</see>.
    /// </summary>
    [JsonPropertyName(OAuthDefaults.Keys.ResponseType)]
    public string ResponseType { get; set; } = OAuthDefaults.Values.Code;
    /// <summary>
    /// The client identifier as described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-2.2"/>Section 2.2
    /// </summary>
    [JsonPropertyName(OAuthDefaults.Keys.ClientId)]
    public required string ClientId { get; set; }
    /// <summary>
    /// A URI your app uses to receive the authorization code or error message;
    /// As described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2"/>Section 3.1.2.
    /// </summary>
    [JsonPropertyName(OAuthDefaults.Keys.RedirectUri)]
    public required string RedirectUri { get; set; }
    /// <summary>
    /// The scope of the access request as described by <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.3"/>Section 3.3.
    /// Can not be empty.
    /// </summary>
    [JsonPropertyName(OAuthDefaults.Keys.Scope)]
    public required string Scope { get; set; }
    /// <summary>
    /// An opaque value used by the client to maintain state between the request and callback.
    /// The authorization server includes this value when redirecting the user-agent back to the client.
    /// The parameter SHOULD be used for preventing cross-site request forgery
    /// as described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-10.12"/>Section 10.12
    /// </summary>
    [JsonPropertyName(OAuthDefaults.Keys.State)]
    public required string State { get; set; }

    /// <summary>
    /// The PKCE code challenge SHA256-image of a random value for this request flow;
    /// Use <see cref="OAuth2Utilitys.GenerateCodeChallenge(string)"/> to generate this value.
    /// </summary>
    [JsonPropertyName(OAuthDefaults.Keys.Pkce.CodeChallenge)]
    public required string CodeChallenge { get; set; }
    /// <summary>
    /// Gets or sets the code challenge method used for Proof Key for Code Exchange (PKCE) in OAuth 2.0 authorization flows.
    /// </summary>
    /// <remarks>
    /// The code challenge method determines how the code challenge is generated and verified during the PKCE process.
    /// Must always be the value S256
    /// </remarks>

    [JsonPropertyName(OAuthDefaults.Keys.Pkce.CodeChallengeMethod)]
    public string CodeChallengeMethod { get; set; } = OAuthDefaults.Values.S256;
}
