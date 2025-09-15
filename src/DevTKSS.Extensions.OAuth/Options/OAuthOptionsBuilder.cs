using DevTKSS.Extensions.OAuth.Endpoints;

namespace DevTKSS.Extensions.OAuth.Options;

public class OAuthOptionsBuilder
{
    private string? _baseUrl;
    private readonly OAuthOptions? _preConfiguredOptions;
    private OAuthClientOptions? _clientOptions;
    private bool? _useNativeHandler;
    private UriTokenOptions? _uriTokenOptions;
    private TokenCacheOptions? _tokenCacheOptions;

    private OAuthOptionsBuilder(
        OAuthOptions? options = null,
        string? providerName = null)
    {
        _preConfiguredOptions = options;
    }
    public static OAuthOptionsBuilder Create(
        OAuthOptions? options = null)
    => new (options);
    public OAuthOptionsBuilder UseNativeHandler(bool useNativeHandler)
    {
        _useNativeHandler = useNativeHandler;
        return this;
    }
    public OAuthOptionsBuilder ConfigureTokenCacheOptions(Action<TokenCacheOptions> configure)
    {
        var tokenCacheKeyOptions = _preConfiguredOptions?.TokenCacheOptions ?? new ();
        configure?.Invoke(tokenCacheKeyOptions);
        _tokenCacheOptions = tokenCacheKeyOptions;
        return this;
    }
    public OAuthOptionsBuilder ConfigureUriTokenOptions(Action<UriTokenOptions> configure)
    {
        var uriTokenOptions = _preConfiguredOptions?.UriTokenOptions ?? new ();
        configure?.Invoke(uriTokenOptions);
        _uriTokenOptions = uriTokenOptions;
        return this;
    }
    public OAuthOptionsBuilder ConfigureClientOptions(Action<OAuthClientOptions> configure)
    {
        var callbackOptions = _preConfiguredOptions?.ClientOptions ?? new OAuthClientOptions();
        configure?.Invoke(callbackOptions);
        _clientOptions = callbackOptions;
        return this;
    }
    public OAuthOptionsBuilder ConfigureUriTokenOptions(Action<TokenCacheOptions> action)
    {
        var tokenOptions = _preConfiguredOptions?.TokenCacheOptions ?? new TokenCacheOptions();
        action?.Invoke(tokenOptions);
        _tokenCacheOptions = tokenOptions;
        return this;
    }
   
    public OAuthOptionsBuilder WithBaseAddress(string baseAddress)
    {
        if(string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentException("Base address cannot be null or empty.", nameof(baseAddress));
        if (!baseAddress.EndsWith('/'))
            baseAddress += '/';
        if(!Uri.TryCreate(baseAddress,UriKind.Relative,out var relativeUri)) 
            throw new InvalidOperationException("Base address must be a valid relative URI.");
        _baseUrl = baseAddress;

        return this;
    }

    public OAuthOptions Build()
    {
        return new OAuthOptions
        {
            Url = _baseUrl,
            ClientOptions = _clientOptions,
            UriTokenOptions = _uriTokenOptions ?? _preConfiguredOptions?.UriTokenOptions ?? new(), // TODO: check
            TokenCacheOptions = _tokenCacheOptions ?? _preConfiguredOptions?.TokenCacheOptions ?? new(), // TODO: check
            UseNativeHandler = _useNativeHandler ?? _preConfiguredOptions?.UseNativeHandler ?? true
        };

    }
}
