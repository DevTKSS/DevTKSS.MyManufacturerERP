namespace DevTKSS.Extensions.OAuth.UI.Uno;

public record OAuthSettings
{
    public OAuthClientOptions? Options { get; init; }
    public string? LoginStartUri { get; init; }
    public string? LoginCallbackUri { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, string?>? PrepareLoginStartUri { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, string?>? PrepareLoginCallbackUri { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>,IDictionary<string,string>?,string?, IDictionary<string, string>?>? ExchangeCodeCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string, IDictionary<string, string>, IDictionary<string, string>?>? PostLoginCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
}
