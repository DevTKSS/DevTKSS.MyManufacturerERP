namespace DevTKSS.Extensions.OAuth.UI.Desktop;

public static class OAuthAuthenticationBuilderExtensions
{
    // TODO: Implement other extension methods to configure/set OAuthSettings Delegates

    // Add an advanced builder allowing to tweak the options directly
    /// <summary>
    /// Configures the OAuth authentication feature by updating directly the <see cref="OidcClientOptions"/> parameter.
    /// </summary>
    /// <remarks>
    /// A
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static IOAuthAuthenticationBuilder ConfigureOAuthClientOptions(
        this IOAuthAuthenticationBuilder builder,
        Action<OAuthClientOptions> updater)
    {
        if (builder is IBuilder<OAuthSettings> authBuilder)
        {
            if (authBuilder.Settings.Options is null)
            {
                authBuilder.Settings = authBuilder.Settings with
                {
                    Options = new OAuthClientOptions()
                };

            }
            updater(authBuilder.Settings.Options);
        }

        return builder;
    }
}