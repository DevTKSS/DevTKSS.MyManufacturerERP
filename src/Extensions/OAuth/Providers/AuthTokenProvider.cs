namespace DevTKSS.Extensions.OAuth.Provider;

public class AuthTokenProvider : IAuthenticationTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly EndpointOptions _options;

    public AuthTokenProvider(
        ILogger<AuthTokenProvider> logger,
        IOptions<EndpointOptions> options,
        HttpClient httpClient)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public Task<string> GetAccessToken(CancellationToken cancellationToken)
        => throw new NotSupportedException("AuthTokenProvider is not wired yet.");
}
