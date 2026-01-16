namespace DevTKSS.Extensions.OAuth.Options;

public class OAuthClientOptions : EndpointOptions
{
	public const string SectionName = "OAuthClient";

    public string? AuthorizationEndpoint { get; set; }
	public string? UserInfoEndpoint { get; set; }
	public string? TokenEndpoint { get; set; }

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    public string? RedirectUri { get; set; }
    public string[]? Scopes { get; set; }
    public TokenKeyOptions TokenKeys { get; set; } = new();

    public bool UsePkce { get; set; } = true;
    public double HttpTimeoutSeconds { get; internal set; }
}
