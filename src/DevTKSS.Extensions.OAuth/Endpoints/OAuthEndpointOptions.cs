namespace DevTKSS.Extensions.OAuth.Endpoints;

public class OAuthEndpointOptions
{
	public const string ConfigurationSection = "EndpointOptions";

	public string? AuthorizationEndpoint { get; init; } // not sure if this makes really sense since refit endpoints can not be set dynamically after compilation
	public string? UserInfoEndpoint { get; init; }
	public string? TokenEndpoint { get; init; }
}
