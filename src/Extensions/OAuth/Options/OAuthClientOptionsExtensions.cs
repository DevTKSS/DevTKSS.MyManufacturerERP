namespace DevTKSS.Extensions.OAuth.Utils;
// TODO: Refactor and implement in code
public static class OAuthClientOptionsExtensions
{
    /// <summary>
    /// Creates an <see cref="AuthorizationCodeRequest"/> from the configured client options and provided authorization state.
    /// </summary>
    /// <param name="options">The OAuth client options to read from.</param>
    /// <param name="configure">Optional customization hook for the request builder.</param>
    /// <returns>A fully populated <see cref="AuthorizationCodeRequest"/> instance.</returns>
    public static AuthorizationCodeRequest ToAuthCodeRequest(
        this OAuthClientOptions options,
        Action<IAuthCodeRequestBuilder>? configure = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Scopes);
        ArgumentOutOfRangeException.ThrowIfZero(options.Scopes.Length, nameof(options.Scopes));

        var builder = AuthorizationCodeRequest.WithBuilder()
                    .WithClientId(options.ClientId!)
                    .WithCallbackUri(options.CallbackUri!)
                    .WithScopes(options.Scopes);

        configure?.Invoke(builder);

        return builder.ToRequest();
    }

    /// <summary>
    /// Builds the authorization start URL for the OAuth authorization code flow.
    /// </summary>
    /// <param name="options">The OAuth client options to read from.</param>
    /// <param name="authorizationState">The authorization state containing the PKCE values and state.</param>
    /// <param name="configure">Optional customization hook for the request builder.</param>
    /// <returns>The fully constructed authorization start URL.</returns>
    public static string BuildAuthorizationStartUrl(
        this OAuthClientOptions options,
        Action<IAuthCodeRequestBuilder>? configure = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var request = options.ToAuthCodeRequest(configure);
        var queryParameters = request.ToDictionary();

        if (!options.UsePkce)
        {
            queryParameters.Remove(OAuthDefaults.Keys.Pkce.CodeChallenge);
            queryParameters.Remove(OAuthDefaults.Keys.Pkce.CodeChallengeMethod);
        }

        var builder = new UriBuilder(options.AuthorizationEndpoint!).AppendQueryParameters(queryParameters);
        return builder.Uri.ToString();
    }

}