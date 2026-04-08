namespace DevTKSS.Extensions.OAuth.Requests;

public class AuthCodeRequestBuilder : IAuthCodeRequestBuilder
{
    private AuthorizationState? _authState;
    private string? _clientId;
    private string? _redirectUri;
    private List<string> _scope = [];
    private string _scopeSeparator = " ";
    private AuthCodeRequestBuilder() { }
    public static IAuthCodeRequestBuilder Empty() => new AuthCodeRequestBuilder(); 
    public IAuthCodeRequestBuilder WithAuthorizationState(AuthorizationState authState)
    {
        ArgumentNullException.ThrowIfNull(authState);
        _authState = authState;
        return this;
    }
    public IAuthCodeRequestBuilder WithAuthNavigationRequest(AuthNavigationRequest authNavigationRequest)
    {
        ArgumentNullException.ThrowIfNull(authNavigationRequest);
        WithAuthorizationState(authNavigationRequest.AuthState);
        return this;
    }
    public IAuthCodeRequestBuilder WithCallbackUri(string callbackUri)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(callbackUri);
        _redirectUri = callbackUri;
        return this;
    }
    public IAuthCodeRequestBuilder WithClientId(string clientId)
    {
        ArgumentNullException.ThrowIfNull(clientId);
        _clientId = clientId;
        return this;
    }
    public IAuthCodeRequestBuilder WithScope(string scope)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(scope);
        _scope.Add(scope);
        return this;
    }
    public IAuthCodeRequestBuilder WithScopes(string[] scopes)
    {
        foreach (var scope in scopes)
        {
            WithScope(scope);
        }
        return this;
    }
    public IAuthCodeRequestBuilder WithScopes(IEnumerable<string> scopes)
    {
        foreach (var scope in scopes)
        {
            WithScope(scope);
        }
        return this;
    }
    public IAuthCodeRequestBuilder WithScopeSeperator(string separator = " ")
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
