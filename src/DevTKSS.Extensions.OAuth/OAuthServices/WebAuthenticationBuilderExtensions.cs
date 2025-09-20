namespace Uno.Extensions.Authentication.Web;

public static class WebAuthenticationBuilderExtensions
{
    /// <summary>
    /// Configures the web authentication feature to be built with the specified post login callback. This overload allows
    /// for a delegate that will use a service of the specified type, a service provider, a token cache reference, a dictionary of credentials, and a dictionary of tokens for authentication.
    /// The underlying property that will be set to such delegate is located on <see cref="WebAuthenticationSettings.PostLoginCallback"/>.
    /// </summary>
    /// <param name="builder">
    /// The instance of <see cref="IWebAuthenticationBuilder"/> to configure.
    /// </param>
    /// <param name="postLogin">
    /// A delegate—which takes a service provider, a token cache reference, a dictionary of credentials, a dictionary of tokens, and returns a dictionary of tokens (or null)—that will prepare the return value of the post login callback.
    /// </param>
    /// <returns>
    /// An instance of <see cref="IWebAuthenticationBuilder"/> that was passed in.
    /// </returns>
    public static IWebAuthenticationBuilder PostLogin<TWebAuthenticationBuilder>(
        this TWebAuthenticationBuilder builder,
        AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string, IDictionary<string, string>, IDictionary<string, string>?> postLogin)
            where TWebAuthenticationBuilder : IWebAuthenticationBuilder =>
                builder.Property((WebAuthenticationSettings s)
                    => s with { PostLoginCallback = postLogin });

    private static TBuilder Property<TBuilder, TSettings>(
        this TBuilder builder,
        Func<TSettings, TSettings> setProperty)
        where TBuilder : IBuilder
        where TSettings : new()
    {
        if (builder is IBuilder<TSettings> authBuilder)
        {
            authBuilder.Settings = setProperty(authBuilder.Settings);
        }

        return builder;
    }
}