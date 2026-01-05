namespace DevTKSS.Extensions.OAuth.Endpoints;

public class OAuthEndpointOptions : EndpointOptions
{
	public const string SectionName = "OAuthOptions";

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
        ArgumentException.ThrowIfNullOrWhiteSpace(AuthorizationEndpoint, nameof(AuthorizationEndpoint));
        ArgumentException.ThrowIfNullOrWhiteSpace(TokenEndpoint, nameof(TokenEndpoint));
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId, nameof(ClientId));
        if (!UsePkce && string.IsNullOrWhiteSpace(ClientSecret))
            throw new ArgumentException("ClientSecret must be provided in OAuthEndpointOptions if Pkce is enabled");
        ArgumentException.ThrowIfNullOrWhiteSpace(RedirectUri, nameof(RedirectUri));
        ArgumentOutOfRangeException.ThrowIfZero(Scopes?.Length ?? 0, nameof(Scopes));
    }
}
