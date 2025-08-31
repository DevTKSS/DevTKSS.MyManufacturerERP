namespace DevTKSS.Extensions.OAuth.Options;

public class OAuthEndpointOptionsBuilder
{
    private string? _authorizationEndpoint;
    private string? _tokenEndpoint;
    private string? _userInfoEndpoint;
    private string? _redirectUri;
    private OAuthEndpointOptionsBuilder(OAuthOptions? options = null)
    { 
        _authorizationEndpoint = options?.EndpointOptions.AuthorizationEndpoint;
        _tokenEndpoint = options?.EndpointOptions.TokenEndpoint;
        _userInfoEndpoint = options?.EndpointOptions.UserInfoEndpoint;
        _redirectUri = options?.EndpointOptions.RedirectUri;
    }
    public static OAuthEndpointOptionsBuilder Create(OAuthOptions? options = null)
        => new (options);
    
    public OAuthEndpointOptionsBuilder WithAuthorizationEndpoint(string endpoint)
    {
        _authorizationEndpoint = endpoint;
        return this;
    }
    public OAuthEndpointOptionsBuilder WithUserInfoEndpoint(string endpoint)
    {
        _userInfoEndpoint = endpoint;
        return this;
    }
    public OAuthEndpointOptionsBuilder WithTokenEndpoint(string endpoint)
    {
        _tokenEndpoint = endpoint;
        return this;
    }
    public OAuthEndpointOptionsBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = redirectUri;
        return this;
    }

    internal OAuthEndpointOptions Build()
    {
        return new OAuthEndpointOptions
        {
            AuthorizationEndpoint = _authorizationEndpoint,
            TokenEndpoint = _tokenEndpoint,
            UserInfoEndpoint = _userInfoEndpoint,
            RedirectUri = _redirectUri
        };
    }
}