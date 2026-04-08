namespace DevTKSS.Extensions.OAuth.Requests;

public static class AuthorizationCodeRequestExtensions
{
    /// <summary>
    /// Converts this request to a dictionary suitable for query parameters.
    /// </summary>
    /// <returns>A dictionary representing the query parameters for this request.</returns>
    public static IDictionary<string, string> ToDictionary(this AuthorizationCodeRequest request)
    {
        return new Dictionary<string, string>
        {
            [OAuthDefaults.Keys.ResponseType] = request.ResponseType,
            [OAuthDefaults.Keys.ClientId] = request.ClientId,
            [OAuthDefaults.Keys.RedirectUri] = request.RedirectUri,
            [OAuthDefaults.Keys.Scope] = request.Scope,
            [OAuthDefaults.Keys.State] = request.State,
            [OAuthDefaults.Keys.Pkce.CodeChallenge] = request.CodeChallenge,
            [OAuthDefaults.Keys.Pkce.CodeChallengeMethod] = request.CodeChallengeMethod
        };
    }
    public static UriBuilder ToUriBuilder(
        this AuthorizationCodeRequest request,
         string authorizationEndpoint,
          IDictionary<string, string>? extraParameters = null)
    {
        var queryParameters = request.ToDictionary();
        var builder = new UriBuilder(authorizationEndpoint).AppendQueryParameters(queryParameters);
        if (extraParameters is not null)
        {
            builder = builder.AppendQueryParameters(extraParameters);
        }
        return builder;
    }
}