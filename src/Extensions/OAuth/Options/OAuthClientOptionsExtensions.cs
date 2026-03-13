namespace DevTKSS.Extensions.OAuth.Utils;
// TODO: Refactor and implement in code
public static class OAuthClientOptionsExtensions
{
    /// <summary>
    /// Creates an <see cref="AuthorizationCodeRequest"/> from the configured client options and provided authorization state.
    /// </summary>
    /// <param name="options">The OAuth client options to read from.</param>
    /// <param name="authorizationState">The authorization state containing the PKCE values and state.</param>
    /// <param name="configure">Optional customization hook for the request builder.</param>
    /// <returns>A fully populated <see cref="AuthorizationCodeRequest"/> instance.</returns>
    public static AuthorizationCodeRequest ToAuthCodeRequest(
        this OAuthClientOptions options,
        AuthorizationState authorizationState,
        Action<IAuthCodeRequestBuilder>? configure = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(authorizationState);

        var builder = AuthorizationCodeRequest.WithBuilder()
                    .WithAuthorizationState(authorizationState)
                    .WithClientId(options.ClientId!)
                    .WithRedirectUri(options.RedirectUri!)
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
        AuthorizationState authorizationState,
        Action<IAuthCodeRequestBuilder>? configure = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(authorizationState);

        var request = options.ToAuthCodeRequest(authorizationState, configure);
        var queryParameters = request.ToDictionary();

        if (!options.UsePkce)
        {
            queryParameters.Remove(OAuthDefaults.Keys.Pkce.CodeChallenge);
            queryParameters.Remove(OAuthDefaults.Keys.Pkce.CodeChallengeMethod);
        }

        var builder = new UriBuilder(options.AuthorizationEndpoint!).AppendQueryParameters(queryParameters);
        return builder.Uri.ToString();
    }

    /// <summary>
    /// Creates a <see cref="WebAuthRequest"/> that contains the authorization start URL and callback URL.
    /// </summary>
    /// <param name="options">The OAuth client options to read from.</param>
    /// <param name="authorizationState">The authorization state containing the PKCE values and state.</param>
    /// <param name="configure">Optional customization hook for the request builder.</param>
    /// <returns>A populated <see cref="WebAuthRequest"/> instance.</returns>
    public static WebAuthRequest ToWebAuthRequest(
        this OAuthClientOptions options,
        AuthorizationState authorizationState,
        Action<IAuthCodeRequestBuilder>? configure = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(authorizationState);

        return new WebAuthRequest(
            options.BuildAuthorizationStartUrl(authorizationState, configure),
            options.RedirectUri!);
    }
}