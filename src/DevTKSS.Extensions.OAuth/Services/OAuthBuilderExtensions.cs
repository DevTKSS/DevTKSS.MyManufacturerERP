namespace DevTKSS.Extensions.OAuth.Services;

public static class OAuthBuilderExtensions
{
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

    // PrepareLoginStartUri overloads
    public static TOAuthBuilder PrepareLoginStartUri<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<string> prepare)
        where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with
            {
                PrepareLoginStartUri = (services, cache, tokens, loginStartUri, ct) => prepare(ct)
            });

    public static TOAuthBuilder PrepareLoginStartUri<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IDictionary<string, string>?, string> prepare)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with
            {
                PrepareLoginStartUri = (services, cache, tokens, loginStartUri, ct) => prepare(tokens, ct)
            });

    public static TOAuthBuilder PrepareLoginStartUri<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IServiceProvider, string> prepare)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with
            {
                PrepareLoginStartUri = (services, cache, tokens, loginStartUri, ct) => prepare(services, ct)
            });

    public static TOAuthBuilder PrepareLoginStartUri<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, string> prepare)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with { PrepareLoginStartUri = prepare });

    // PostLogin overloads
    public static TOAuthBuilder PostLogin<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IDictionary<string, string>, IDictionary<string, string>?> postLogin)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with
            {
                PostLoginCallback = (services, cache, credentials, redirectUri, tokens, ct) => postLogin(tokens, ct)
            });

    public static TOAuthBuilder PostLogin<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string, IDictionary<string, string>, IDictionary<string, string>?> postLogin)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with { PostLoginCallback = postLogin });

    // Refresh overloads
    public static TOAuthBuilder Refresh<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IDictionary<string, string>, IDictionary<string, string>?> refresh)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with
            {
                RefreshCallback = (services, cache, tokens, ct) => refresh(tokens, ct)
            });

    public static TOAuthBuilder Refresh<TOAuthBuilder>(this TOAuthBuilder builder, AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?> refresh)
         where TOAuthBuilder : IOAuthBuilder
        => builder
            .Property((OAuthSettings s) => s with { RefreshCallback = refresh });
}
