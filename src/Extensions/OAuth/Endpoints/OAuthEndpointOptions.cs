namespace DevTKSS.Extensions.OAuth.Endpoints;

public class OAuthEndpointOptions : EndpointOptions
{
	public const string ConfigurationSection = "EndpointOptions";

	public string? AuthorizationEndpoint { get; init; }
	public string? UserInfoEndpoint { get; init; }
	public string? TokenEndpoint { get; init; }

    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    public string? RedirectUri { get; init; }
    public string[]? Scopes { get; init; }
    public TokenKeyOptions TokenKeys { get; init; } = new();

    public bool UsePkce { get; init; } = true;

    public void Valdiate()
    {
        if (string.IsNullOrWhiteSpace(AuthorizationEndpoint))
            throw new ArgumentException("AuthorizationEndpoint must be provided in OAuthEndpointOptions");
        if (string.IsNullOrWhiteSpace(TokenEndpoint))
            throw new ArgumentException("TokenEndpoint must be provided in OAuthEndpointOptions");
        if (string.IsNullOrWhiteSpace(UserInfoEndpoint))
            throw new ArgumentException("UserInfoEndpoint must be provided in OAuthEndpointOptions");
        if (string.IsNullOrWhiteSpace(ClientId))
            throw new ArgumentException("ClientId must be provided in OAuthEndpointOptions");
        if (!UsePkce && string.IsNullOrWhiteSpace(ClientSecret))
            throw new ArgumentException("ClientSecret must be provided in OAuthEndpointOptions if Pkce is enabled");
        if (string.IsNullOrWhiteSpace(RedirectUri))
            throw new ArgumentException("RedirectUri must be provided in OAuthEndpointOptions");
        if (Scopes == null || Scopes.Length == 0)
            throw new ArgumentException("At least one Scope must be provided in OAuthEndpointOptions");
    }
}
