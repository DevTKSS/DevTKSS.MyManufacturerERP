using Uno.UI.Extensions;

namespace DevTKSS.Extensions.OAuth.UI.Http;

public sealed class OAuthTokenHttpClient
    : IOAuthTokenClient,
     IAuthenticationTokenProvider // Interface from Uno.Extensions.Http called in their Hostbuilder providing integration into the HttpClient pipeline for automatic token injection into outgoing requests
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly ITokenCache _tokenCache;
    private OAuthClientOptions? _options;
    private AuthorizationState? _state;
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
    public ValueTask<AuthorizationState> GetAuthorizeStateAsync(IDictionary<string, string>? extraParameters = null)
    {
        _state = new AuthorizationState();
        return ValueTask.FromResult(_state);
    }

    public ValueTask<WebAuthRequest?> GetWebAuthRequestAsync(AuthorizationState state, IDictionary<string, string>? extraParameters = null)
    {
        if (_options is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options are not configured; cannot build web auth request.");
            }
            return ValueTask.FromResult<WebAuthRequest?>(default);
        }

        if (string.IsNullOrWhiteSpace(_options.AuthorizationEndpoint) || string.IsNullOrWhiteSpace(_options.RedirectUri))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("AuthorizationEndpoint or RedirectUri is not configured; cannot build web auth request.");
            }
            return ValueTask.FromResult<WebAuthRequest?>(default);
        }

        var request = _options.ToWebAuthRequest(state);
        return ValueTask.FromResult<WebAuthRequest?>(request);
    }

    public async ValueTask<TokenResponse?> ExchangeCodeAsync(AuthorizationState state, string callbackResult, CancellationToken cancellationToken = default)
    {
        if (_options is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth client options are not configured; cannot exchange authorization code.");
            }
            return default;
        }

        if (!TryValidateCallback(callbackResult, out var parameters))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth callback returned an error response.");
            }
            return default;
        }

        if (!parameters.TryGetState(out var returnedState) || !string.Equals(returnedState, state.State, StringComparison.Ordinal))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth callback state mismatch (possible CSRF attack).");
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
            RedirectUri = _options.RedirectUri!,
            Code = code,
            CodeVerifier = state.CodeVerifier
        }, cancellationToken);
    }

    public async ValueTask<TokenResponse?> ExchangeCodeAsync(AccessTokenRequest request, CancellationToken cancellationToken = default)
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



    #region Callback Validation
    /// <summary>
    /// Validates OAuth callback URI and returns query parameters if valid.
    /// </summary>
    /// <param name="callbackUri">The callback URI to validate.</param>
    /// <param name="parameters">Query parameters if validation succeeds; null otherwise.</param>
    /// <returns>True if callback is valid; false otherwise.</returns>
    private bool TryValidateCallback(
        string? callbackUri,
        [NotNullWhen(true)] out IDictionary<string, string>? parameters)
    {
        parameters = null;

        if (string.IsNullOrWhiteSpace(callbackUri) || !Uri.TryCreate(callbackUri, UriKind.Absolute, out var uri))
        {
            return false;
        }

        parameters = OAuth2Utilitys.GetQueryParameters(uri);
        if (parameters is not { Count: > 0 })
        {
            parameters = null;
            return false;
        }

        return !parameters.TryGetErrorCode(out _);
    }
    #endregion
}
