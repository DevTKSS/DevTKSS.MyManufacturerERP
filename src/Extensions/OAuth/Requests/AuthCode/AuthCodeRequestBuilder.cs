namespace DevTKSS.Extensions.OAuth.Requests;

public class AuthCodeRequestBuilder : IAuthCodeRequestBuilder
{
    private AuthorizationState? _authState;
    private string? _clientId;
    private string? _redirectUri;
    private List<string> _scope = [];
    private string _scopeSeparator = " ";
    private AuthCodeRequestBuilder() { }
    public static AuthCodeRequestBuilder Empty() => new();
    public AuthCodeRequestBuilder WithAuthorizationState(AuthorizationState authState)
    {
        ArgumentNullException.ThrowIfNull(authState);
        _authState = authState;
        return this;
    }
    public AuthCodeRequestBuilder WithRedirectUri(string redirectUri)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(redirectUri);
        _redirectUri = redirectUri; 
        return this;
    }
    public AuthCodeRequestBuilder WithClientId(string clientId)
    {
        ArgumentNullException.ThrowIfNull(clientId);
        _clientId = clientId;
        return this;
    }
    public AuthCodeRequestBuilder WithScope(string scope)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(scope);
        _scope.Add(scope);
        return this;
    }
    public AuthCodeRequestBuilder WithScopes(string[] scopes)
    {
        foreach (var scope in scopes)
        {
            WithScope(scope);
        }
        return this;
    }
    public AuthCodeRequestBuilder WithScopes(IEnumerable<string> scopes)
    {
        foreach (var scope in scopes)
        {
            WithScope(scope);
        }
        return this;
    }
    public AuthCodeRequestBuilder WithScopeSeperator(string separator = " ")
    {
        _scopeSeparator = separator;
        return this;
    }
    public AuthorizationCodeRequest ToRequest()
    {
        ArgumentNullException.ThrowIfNull(_authState);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_clientId);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_redirectUri);
        ArgumentOutOfRangeException.ThrowIfZero(_scope.Count, nameof(_scope));

        return new AuthorizationCodeRequest
        {
            ClientId = _clientId!,
            RedirectUri = _redirectUri,
            Scope = _scope.JoinBy(_scopeSeparator),
            State = _authState.State,
            CodeChallenge = _authState.CodeChallenge
        };
    }
}
