namespace DevTKSS.Extensions.OAuth.UI.Http;
public sealed class OAuthTokenHttpClient
    : IOAuthTokenClient, // TODO: Fit Interface and Implementation after finding a better SOC-Service Split. Other Services should not provide the Request to the Client, they should be build by the client!
     IAuthenticationTokenProvider // Interface from Uno.Extensions.Http called in their Hostbuilder providing integration into the HttpClient pipeline for automatic token injection into outgoing requests
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly ITokenCache _tokenCache;
    private OAuthClientOptions? _options;
    private AuthNavigationRequest? _request;
    public OAuthTokenHttpClient(
        HttpClient httpClient,
        ILogger<OAuthTokenHttpClient> logger,
        ITokenCache tokenCache,
        IOptions<OAuthClientOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _tokenCache = tokenCache;
        ApplyOptions(options.Value);
    }
    #region Options Configuration
    public void Configure(OAuthClientOptions options)
    {
        if (options is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options are missing. Skipping configuration.");
            }
            return;
        }

        ApplyOptions(options);
    }

    public void Configure(Action<OAuthClientOptions> configureOptions)
    {
        if (configureOptions is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options configuration callback is missing.");
            }
            return;
        }
        var options = new OAuthClientOptions();
        configureOptions(options);
        ApplyOptions(options);
    }
    private void ApplyOptions(OAuthClientOptions options)
    {
        new OAuthClientOptionsValidator().ValidateAndThrow(options);
        _options = options;
        _httpClient.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
    }
    #endregion

    public async Task<string> GetAccessToken(CancellationToken cancellationToken) // TODO: This method should trigger GETTING the Auth token if not available yet (if options are there), not just return default!
    {
        if (await _tokenCache.GetAsync(cancellationToken) is not { Count: > 0 } tokens
            || !tokens.TryGetAccessToken(out var accessToken))
        {

            return string.Empty;
        }
        return accessToken;
    }
 
    public async ValueTask<TokenResponse?> HandleCallbackAsync(
        Uri redirectUri,
        CancellationToken cancellationToken = default)
    {
        if (_request?.AuthState is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Authorization Navigation Request is not initialized.");
            }
            return default;
        }

        if (!_request.IsCallbackUri(redirectUri))
        {
            return default;
        }

        return await ExchangeCodeAsync(redirectUri.OriginalString, cancellationToken);
    }

    private async ValueTask<TokenResponse?> ExchangeCodeAsync(string callbackResult, CancellationToken cancellationToken = default)
    {
        if (_options is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options are not configured; cannot exchange authorization code.");
            }
            return default;
        }

        if (_request is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Authorization Navigation Request is not initialized.");
            }
            return default;
        }

        if (!_request.IsValidCallback(callbackResult, out var parameters))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth callback returned an error response.");
            }
            return default;
        }

        if (!parameters.TryGetCode(out var code) || string.IsNullOrWhiteSpace(code))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Authorization code missing from callback.");
            }
            return default;
        }

        return await ExchangeCodeAsync(new AccessTokenRequest
        {
            ClientId = _options.ClientId!,
            RedirectUri = _options.CallbackUri!,
            Code = code,
            CodeVerifier = _request.AuthState.CodeVerifier
        }, cancellationToken);
    }

    public async ValueTask<TokenResponse?> ExchangeCodeAsync(AccessTokenRequest request, CancellationToken cancellationToken = default) // TODO: Find optimal Way to integrate the Interactive Auth part between Auth Start and Exchange here. Prefer setting this to private, but currently collides with OAuthProvider. Eventually SOC Layer Evaluation Problem
    {
        if (request is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Access token request is missing; cannot exchange authorization code.");
            }
            return default;
        }

        if (_options is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options are not configured; cannot exchange authorization code.");
            }
            return default;
        }

        var tokenEndpoint = _options!.TokenEndpoint;
        var requestDict = request.ToDictionary();

        using var content = new FormUrlEncodedContent(requestDict);
        using var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    }

    public async ValueTask<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Refresh token request is missing; cannot refresh tokens.");
            }
            return default;
        }

        if (_options is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options are not configured; cannot refresh tokens.");
            }
            return default;
        }

        var tokenEndpoint = _options!.TokenEndpoint;
        var requestDict = request.ToDictionary();

        using var content = new FormUrlEncodedContent(requestDict);
        using var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    }

}
