namespace DevTKSS.Extensions.OAuth.Endpoints;

public class OAuthEndpointOptions
{
	public const string ConfigurationSection = "EndpointOptions";

	public string? AuthorizationEndpoint { get; init; }
	public string? UserInfoEndpoint { get; init; }
	public string? TokenEndpoint { get; init; }

    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}
