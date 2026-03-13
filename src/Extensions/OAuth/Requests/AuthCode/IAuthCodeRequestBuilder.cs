namespace DevTKSS.Extensions.OAuth.Requests;

public interface IAuthCodeRequestBuilder
{
    IAuthCodeRequestBuilder WithAuthorizationState(AuthorizationState authState);
    IAuthCodeRequestBuilder WithClientId(string clientId);
    IAuthCodeRequestBuilder WithScope(string scope);
    IAuthCodeRequestBuilder WithScopes(string[] scopes);
    IAuthCodeRequestBuilder WithScopes(IEnumerable<string> scopes);
    IAuthCodeRequestBuilder WithScopeSeperator(string separator);
}
