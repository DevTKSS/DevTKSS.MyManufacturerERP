namespace DevTKSS.Extensions.OAuth.Requests;


/// <summary>
/// Represents a web authentication request containing the authorization start URL and callback URL.
/// </summary>
/// <param name="StartUrl">The authorization start URL.</param>
/// <param name="CallbackUrl">The callback URL to handle the authorization response.</param>
// DO NOT CHANGE!!! Equals the WebAuthenticationRequest in Uno's WebAuthenticationBrokerProvider.AuthenticateAsync, which has no implementation on Desktop Platform (Hosted on Windows) by now
public record WebAuthRequest(string StartUrl, string CallbackUrl);
