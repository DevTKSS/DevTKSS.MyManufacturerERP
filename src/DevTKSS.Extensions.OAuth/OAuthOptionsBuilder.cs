namespace DevTKSS.Extensions.OAuth;

public class OAuthOptionsBuilder
{
    private string? _authorizationEndpoint;
    private string? _tokenEndpoint;
    private string? _userInfoEndpoint;
    private string? _clientID;
    private string? _redirectUri;
    private string[] _scopes;
    private OAuthOptionsBuilder(OAuthOptions? options)
    {
        _authorizationEndpoint = options?.AuthorizationEndpoint;
        _tokenEndpoint = options?.TokenEndpoint;
        _userInfoEndpoint = options?.UserInfoEndpoint;
        _clientID = options?.ClientID;
        _redirectUri = options?.RedirectUri;
        _scopes = options?.Scopes ?? [];
    }
    public static OAuthOptionsBuilder Create(OAuthOptions? options = null)
    => new (options);
    
    public OAuthOptionsBuilder WithAuthorizationEndpoint(string endpoint)
    {
        _authorizationEndpoint = endpoint;
        return this;
    }
    public OAuthOptionsBuilder WithUserInfoEndpoint(string endpoint)
    {
        _userInfoEndpoint = endpoint;
        return this;
    }
    public OAuthOptionsBuilder WithTokenEndpoint(string endpoint)
    {
        _tokenEndpoint = endpoint;
        return this;
    }
    public OAuthOptionsBuilder WithClientID(string clientId)
    {
        _clientID = clientId;
        return this;
    }
    public OAuthOptionsBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = redirectUri;
        return this;
    }
    public OAuthOptionsBuilder WithScopes(string[] scopes)
    {
        _scopes = scopes;
        return this;
    }
    public OAuthOptions Build()
    {
        var options = new OAuthOptions
        {
            AuthorizationEndpoint = _authorizationEndpoint,
            UserInfoEndpoint = _userInfoEndpoint,
            TokenEndpoint = _tokenEndpoint,
            ClientID = _clientID,
            RedirectUri = _redirectUri,
            Scopes = _scopes
        };
        // Validate the options
        var validator = new OAuthOptionsValidator();
        var validationResult = validator.Validate(options);
        if(!validationResult.IsValid)
          throw new ValidationException(validationResult.Errors.First().ErrorMessage);
        return options;
    
    }
}