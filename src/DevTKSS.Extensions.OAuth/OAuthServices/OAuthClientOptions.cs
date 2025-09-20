using DevTKSS.Extensions.OAuth.Endpoints;

namespace DevTKSS.Extensions.OAuth.OAuthServices;

public record OAuthClientOptions : AuthCallbackOptions
{   
	public new const string DefaultName = "ClientOptions";
	public string ProviderName { get; init; } = DefaultName;

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
	public OAuthEndpointOptions? EndpointOptions { get; init; }
}
