namespace DevTKSS.Extensions.OAuth.UI;
/// <summary>
/// Service for handling OAuth authentication flows (desktop loopback via system browser).
/// </summary>
public record OAuthProvider : BaseAuthenticationProvider
{
    public const string DefaultName = "OAuth";

    private readonly IOptionsMonitor<OAuthOptions> _optionsMonitor;
    private readonly ILogger _logger;
    private readonly IOAuthTokenClient _client;
    private readonly ISystemBrowserAuthBrokerProvider _systemBrowser; // TODO: Abstract this to similar approach like IBrowser, but without Duende Reference, IAuthNavigationService should potentially become the handler of doing the UI parts. But across Uno, the (static) WebAuthenticationBrowserProvider.AuthenticateAsync coming from Microsoft is the regular go to.
    private readonly ITokenCache _tokenCache;
    private readonly IServiceProvider _serviceProvider;
    public OAuthSettings? AuthSettings { get; init; }
    // NOTE: Replicates WebAuthenticationProvider from Uno, but extending / fitting to oAuth2 interactive flow capabilities. https://github.com/unoplatform/uno.extensions/blob/main/src/Uno.Extensions.Authentication.UI/Web/WebAuthenticationProvider.cs
    public OAuthProvider(
        ILogger<OAuthProvider> logger, 
        ITokenCache tokens,
        IServiceProvider serviceProvider,
        IOAuthTokenClient tokenClient,
        ISystemBrowserAuthBrokerProvider systemBrowser,
        IOptionsMonitor<OAuthOptions> optionsMonitor,
        [ServiceKey] string name = DefaultName)
        : base(logger,name,tokens)
    {
        _client = tokenClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _systemBrowser = systemBrowser;
        _tokenCache = tokens;
        Name = name;
        
        // Validation handled on init and change, so we don't need to do this again on each function
        _optionsMonitor = optionsMonitor;
        _optionsMonitor.OnChange(Options_OnChange);

        new OAuthOptionsValidator().ValidateAndThrow(AuthOptions);
    }

    private OAuthOptions AuthOptions => _optionsMonitor.Get(Name);

    #region Options handling
    private void Options_OnChange(OAuthOptions options, string? sectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName) || sectionName != Name)
        {
            return;
        }
        new OAuthOptionsValidator().ValidateAndThrow(options);
    }
    private OAuthSettings? _internalSettings;
    private OAuthSettings InternalSettings
    {
        get
        {
            if (_internalSettings is null)
            {
                _internalSettings = AuthSettings ?? new OAuthSettings();
                var config = AuthOptions;
                if (config is not null)
                {
                    _internalSettings = _internalSettings with
                    {
                        LoginStartUri = !string.IsNullOrWhiteSpace(config.LoginStartUri) ? config.LoginStartUri : _internalSettings.LoginStartUri,
                        LoginCallbackUri = !string.IsNullOrWhiteSpace(config.LoginCallbackUri) ? config.LoginCallbackUri : _internalSettings.LoginCallbackUri,
                        Options = config.Options ?? _internalSettings.Options
                    };
                }
            }
            return _internalSettings;
        }
    }
    #endregion

    protected async override ValueTask<IDictionary<string,string>?> InternalLoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials, CancellationToken cancellationToken)
    {
        var loginStartUri = InternalSettings!.LoginStartUri;
        loginStartUri = await PrepareLoginStartUriAsync(credentials, loginStartUri, cancellationToken);
        if (loginStartUri is null ||
            string.IsNullOrWhiteSpace(loginStartUri))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning($"{nameof(OAuthSettings.LoginStartUri)} not specified, unable to start login flow");
            }
            return default;
        }
        var loginCallbackUri = InternalSettings.LoginCallbackUri;

        if (string.IsNullOrWhiteSpace(loginCallbackUri) &&
            loginStartUri.Contains(OAuthDefaults.Keys.RedirectUri))
        {
            var args = OAuth2Utilitys.GetQueryParameters(loginStartUri);
            loginCallbackUri = args[OAuthDefaults.Keys.RedirectUri];
        }

        loginCallbackUri = await PrepareLoginCallbackUriAsync(credentials, loginCallbackUri, cancellationToken);

        if (string.IsNullOrWhiteSpace(loginCallbackUri))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning($"{nameof(InternalSettings.LoginCallbackUri)} not specified and {OAuthDefaults.Keys.RedirectUri} not set in {nameof(InternalSettings.LoginStartUri)}, unable to start login flow");
            }
            return default;
        }
        var userResult = await _systemBrowser.AuthenticateAsync(WebAuthenticationOptions.None, new Uri(loginStartUri), new Uri(loginCallbackUri),cancellationToken);
        var authData = userResult?.ResponseData ?? string.Empty;
        
        var query = OAuth2Utilitys.GetQueryParameters(authData);

        var tokens = new Dictionary<string, string>();
        if (query is null)
        {
            return tokens;
        }

        if (InternalSettings!.Options!.UsePkce)
        {
            query = await InternalCodeExchangeAsync(query, credentials, authData, cancellationToken);
            if (query is null)
            {
                return tokens;
            }
        }

        if (query.TryGetAccessToken(out var accessToken) && !string.IsNullOrWhiteSpace(accessToken))
        {
            tokens.AddOrReplace(InternalSettings.Options.TokenKeys.AccessTokenKey, accessToken);
        }

        if (query.TryGetRefreshToken(out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            tokens.AddOrReplace(InternalSettings.Options.TokenKeys.RefreshTokenKey, refreshToken);
        }
        return await PostLoginAsync(credentials, authData, tokens, cancellationToken);
    }
    protected async virtual ValueTask<string?> InternalPrepareLoginStartUriAsync(IDictionary<string, string>? credentials, string? loginStartUri, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(loginStartUri))
        {
            return default;
        }
        if (InternalSettings is not { Options: not null })
        {
            return default;
        }
        string state;
        string codeVerifier;
        credentials ??= new Dictionary<string, string>();
        // Generate PKCE values
        state = OAuth2Utilitys.GenerateState();
        codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);

      
        IDictionary<string, string>? authCodeParams;
        if (InternalSettings.Options.UsePkce)
        {
            authCodeParams = new AuthorizationCodeRequest()
            {
                ClientId = InternalSettings.Options!.ClientId!,
                RedirectUri = InternalSettings.Options.RedirectUri ?? InternalSettings.LoginCallbackUri ?? loginStartUri,
                Scope = InternalSettings.Options.Scopes.JoinBy(" "),
                State = state,
                CodeChallenge = codeChallenge
            }.ToDictionary();
        }
        else
        {
            authCodeParams = new Dictionary<string, string>
            {
                { OAuthDefaults.Keys.ResponseType, OAuthDefaults.Values.Code },
                { OAuthDefaults.Keys.ClientId, InternalSettings.Options.ClientId! },
                { OAuthDefaults.Keys.RedirectUri, loginStartUri },
                { OAuthDefaults.Keys.Scope, InternalSettings.Options.Scopes.JoinBy(" ") },
                { OAuthDefaults.Keys.State, state }
            };
        }
        var builder = new UriBuilder(InternalSettings.Options.AuthorizationEndpoint!).AppendQueryParameters(authCodeParams);

        var configuredUri = builder.Uri.ToString();

        credentials.AddOrReplace(OAuthDefaults.Keys.Pkce.CodeVerifier, codeVerifier);
        credentials.AddOrReplace(OAuthDefaults.Keys.State, state);
        return configuredUri;

    }

    protected async virtual ValueTask<IDictionary<string, string>?> InternalCodeExchangeAsync(IDictionary<string,string> tokens, IDictionary<string, string>? credentials, string? redirectUri, CancellationToken cancellationToken)
    {   
        if (string.IsNullOrWhiteSpace(redirectUri)
             || !Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Provided redirect Uri is invalid!");
            }
            return default;
        }
        // If the credentials or tokens are missing, log and return early
        if (credentials is not { Count: > 0 })
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No credentials provided for exchanging authorization code");
            }
            return default;
        }
      
        var queryParams = OAuth2Utilitys.GetQueryParameters(redirectUri);

        if (queryParams is not { Count: > 0 })
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No query parameters found in authentication result redirectUri");
            }
            return default;
        }

        if (queryParams.TryGetErrorCode(out var error) && !string.IsNullOrWhiteSpace(error))
        {
            queryParams.TryGetErrorDescription(out var errorDescription);
            queryParams.TryGetErrorUri(out var errorUri);

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("OAuth error received: {Error}, Description: {Description}, Uri: {Uri}", error, errorDescription, errorUri);
            }

            return default;
        }

        if (!queryParams.TryGetCode(out var authCode) || string.IsNullOrWhiteSpace(authCode))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No authorization code received from authentication result");
            }
            return default;
        }

        // Extract PKCE values from credentials
        if (!credentials.TryGetState(out var state) || string.IsNullOrWhiteSpace(state))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("PKCE codeVerifier or state have not been preserved!");
            }
            return default;
        }
        if (!credentials.TryGetCodeVerifier(out var codeVerifier) || string.IsNullOrWhiteSpace(codeVerifier))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("PKCE codeVerifier or state have not been preserved!");
            }
            return default;
        }
        // Extract and validate the returned state
        if (!queryParams.TryGetState(out var returnedState) || string.IsNullOrWhiteSpace(returnedState) || state != returnedState)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("State parameter mismatch. Potential CSRF attack.");
            }
            return default;
        }

        var tokenResponse = await _client.ExchangeCodeAsync(new AccessTokenRequest
        {
            ClientId = InternalSettings!.Options!.ClientId!,
            RedirectUri = InternalSettings.Options.RedirectUri ?? redirectUri!,
            Code = authCode,
            CodeVerifier = codeVerifier
        }, cancellationToken);

        if (tokenResponse is not TokenResponse { AccessToken: not null, RefreshToken: not null, ExpiresIn: > 0, TokenType: OAuthDefaults.Values.Bearer } response)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Token response is null");
            }
            return default;
        }

        tokens.AddOrReplace(response.ToDictionary(false));

        return await ExchangeCodeCallbackAsync(credentials, await _tokenCache.GetAsync(cancellationToken), redirectUri, cancellationToken);

    }
   
    protected async override ValueTask<IDictionary<string, string>?> InternalRefreshAsync(CancellationToken cancellationToken)
    {
        var tokens = await _tokenCache.GetAsync(cancellationToken);
        if ((!tokens.TryGetRefreshToken(out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken)) ||
            (!tokens.TryGetAccessToken(out var accessToken) || string.IsNullOrWhiteSpace(accessToken)))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No credentials available to refresh");
            }
            return default;
        }

        var tokenResponse = await _client.RefreshTokenAsync(new RefreshTokenRequest
        {
            ClientId = InternalSettings!.Options!.ClientId!,
            RefreshToken = refreshToken ?? accessToken,
        }, cancellationToken);

        if (tokenResponse is not TokenResponse { AccessToken: not null, RefreshToken: not null, ExpiresIn: > 0, TokenType: OAuthDefaults.Values.Bearer } response)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Token refresh response missing required tokens");
            }
            return default;
        }
        var keys = InternalSettings.Options.TokenKeys;

        tokens.AddOrReplace(keys.AccessTokenKey, response.AccessToken);
        tokens.AddOrReplace(keys.RefreshTokenKey, response.RefreshToken);
        tokens.AddOrReplace(keys.ExpiresInKey, DateTime.Now.AddSeconds(response.ExpiresIn).ToString("g"));

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Tokens refreshed successfully");
        }

        return tokens;
    }

    #region Callbacks from Config
 
    protected async virtual Task<string?> PrepareLoginStartUriAsync(IDictionary<string, string>? credentials, string? loginStartUri, CancellationToken cancellationToken)
    {
        if (InternalSettings!.PrepareLoginStartUri is not null)
        {
            return await InternalSettings.PrepareLoginStartUri(_serviceProvider, Tokens, credentials, loginStartUri, cancellationToken);
        }
        return loginStartUri;
    }

    protected async virtual Task<string?> PrepareLoginCallbackUriAsync(IDictionary<string, string>? credentials, string? loginCallbackUri, CancellationToken cancellationToken)
    {
        if (InternalSettings!.PrepareLoginCallbackUri is not null)
        {
            return await InternalSettings.PrepareLoginCallbackUri(_serviceProvider, Tokens, credentials, loginCallbackUri, cancellationToken);
        }
        return loginCallbackUri;
    }

    protected async virtual Task<IDictionary<string, string>?> ExchangeCodeCallbackAsync(IDictionary<string,string>? credentials, IDictionary<string, string> tokens, string? redirectUri, CancellationToken cancellationToken)
    {
        if (InternalSettings!.ExchangeCodeCallback is not null)
        {
            return await InternalSettings.ExchangeCodeCallback(_serviceProvider, _tokenCache, await _tokenCache.GetAsync(cancellationToken), credentials,  redirectUri, cancellationToken);
        }
        return tokens;
    }

    protected async virtual ValueTask<IDictionary<string, string>?> PostLoginAsync(IDictionary<string, string>? credentials, string redirectUri, IDictionary<string, string> tokens, CancellationToken cancellationToken)
    {
        if (InternalSettings!.PostLoginCallback is not null)
        {
            return await InternalSettings.PostLoginCallback(_serviceProvider, Tokens, credentials, redirectUri, tokens, cancellationToken);
        }
        return tokens;
    }
    protected async virtual ValueTask<IDictionary<string, string>?> PostRefreshAsync(CancellationToken cancellationToken)
    {
        if (InternalSettings!.RefreshCallback is not null)
        {
            return await InternalSettings.RefreshCallback(_serviceProvider, Tokens, await Tokens.GetAsync(cancellationToken), cancellationToken);
        }
        return await base.InternalRefreshAsync(cancellationToken);
    }

    #endregion
}