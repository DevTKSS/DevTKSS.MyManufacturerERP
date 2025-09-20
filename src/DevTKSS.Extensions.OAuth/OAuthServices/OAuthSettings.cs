namespace DevTKSS.Extensions.OAuth.OAuthServices;

public record OAuthSettings
{
	public OAuthClientOptions? ClientOptions { get; init; }
	private string? _loginStartUri;
	public string? LoginStartUri { get => _loginStartUri ?? ClientOptions?.CallbackUri; init => _loginStartUri = value; }
    private string? _loginCallbackUri;
    public string? LoginCallbackUri { get => _loginCallbackUri ?? ClientOptions?.CallbackUri; init => _loginCallbackUri = value; }
    public TokenCacheOptions TokenCacheOptions { get; init; } = new();
    public UriTokenOptions UriTokenOptions { get; init; } = new();
    public AsyncFunc<IServiceProvider, ITokenCache, string?, string>? PrepareLoginStartUri { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, string?, string>? PrepareLoginCallbackUri { get; init; } 
    public AsyncFunc<IServiceProvider, ITokenCache, string, IDictionary<string, string>, IDictionary<string, string>?>? PostLoginCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, IDictionary<string,string>?>? CodeExchangeCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }


}

public record OAuthSettings<TService> : OAuthSettings
	where TService : notnull
{
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, string?, string>? PrepareLoginStartUri { get; init; }
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, string?, string>? PrepareLoginCallbackUri { get; init; }
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, string, IDictionary<string, string>, IDictionary<string, string>?>? PostLoginCallback { get; init; }
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, IDictionary<string, string>?>? CodeExchangeCallback { get; init; }
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
    
}
