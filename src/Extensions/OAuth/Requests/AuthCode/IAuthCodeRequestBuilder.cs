using System.ComponentModel.DataAnnotations;

namespace DevTKSS.Extensions.OAuth.Requests;

public interface IAuthCodeRequestBuilder
{
    IAuthCodeRequestBuilder WithAuthorizationState(AuthorizationState authState);
    IAuthCodeRequestBuilder WithClientId([MinLength(1)] string clientId);
    IAuthCodeRequestBuilder WithCallbackUri([MinLength(1)] string redirectUri);
    IAuthCodeRequestBuilder WithScope([MinLength(1)]string scope);
    IAuthCodeRequestBuilder WithScopes([MinLength(1)]string[] scopes);
    IAuthCodeRequestBuilder WithScopes([MinLength(1)]IEnumerable<string> scopes);
    IAuthCodeRequestBuilder WithScopeSeperator([MinLength(1)]string separator);
    AuthorizationCodeRequest ToRequest();
}
