namespace DevTKSS.Extensions.OAuth.OAuthServices;

public record OAuthSettings : WebAuthenticationSettings
{
    public OAuthClientOptions? ClientOptions { get; init; }
    private string? _loginStartUri;
    public new string? LoginStartUri { get => _loginStartUri ?? ClientOptions?.CallbackUri; init => _loginStartUri = value; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, IDictionary<string,string>?>? ExchangeCodeForTokensAsync { get; init; }
}

public record OAuthSettings<TService> : OAuthSettings
	where TService : notnull
{
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, IDictionary<string, string>?>? ExchangeCodeForTokensAsync { get; init; }
}
