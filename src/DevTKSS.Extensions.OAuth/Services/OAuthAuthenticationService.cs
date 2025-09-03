namespace DevTKSS.Extensions.OAuth.Services;

/// <summary>
/// Production-ready wrapper over one or more IAuthenticationProvider instances.
/// Implements IAuthenticationService and delegates to the chosen provider.
/// </summary>
public sealed class OAuthAuthenticationService : IAuthenticationService
{
    private readonly ILogger<OAuthAuthenticationService> _logger;
    private readonly ITokenCache _tokenCache;
    private readonly string _defaultProviderName;
    private readonly IOptionsSnapshot<OAuthOptions> _oAuthOptions;
    private readonly IServiceProvider _serviceProvider;
    public OAuthAuthenticationService(
        ILogger<OAuthAuthenticationService> logger,
        ITokenCache tokenCache,
        IServiceProvider serviceProvider,
        IOptionsSnapshot<OAuthOptions> oauthOptions,
        IOptions<AuthenticationServiceOptions> options)
    {
        _logger = logger;
        _tokenCache = tokenCache;
        _defaultProviderName = options.Value.DefaultProviderName;
        _oAuthOptions = oauthOptions;
        Providers = options.Value.ProviderNames;
        _serviceProvider = serviceProvider;
    }

    public string[] Providers { get; }


    public event EventHandler? LoggedOut;
    
    public async ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = default)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var isAuthenticated = await _tokenCache.HasTokenAsync(ct);
        if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTraceMessage($"Is authenticated - {isAuthenticated}");
        return isAuthenticated;

    }

    public async ValueTask<bool> LoginAsync(
        IDispatcher? dispatcher,
        IDictionary<string, string>? credentials = null,
        string? provider = null,
        CancellationToken? cancellationToken = default)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        IOAuthProvider authProvider = await GetOAuthProviderAsync(provider, ct);

        _logger.LogInformation("Attemting to Login via provider '{Provider}' starting...", authProvider.Name);

        var resultTokens = await authProvider.LoginAsync(dispatcher, credentials, ct);
        if (resultTokens is null)
        {
            _logger.LogWarning("Login via provider '{Provider}' returned no tokens.", authProvider.Name);
            return false;
        }

        await _tokenCache.SaveAsync(provider ?? _defaultProviderName, resultTokens, ct);
        _logger.LogInformation("Login via provider '{Provider}' succeeded.", authProvider.Name);
        return true;
    }

  

    public async ValueTask<bool> RefreshAsync(CancellationToken? cancellationToken = default)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var authProvider = await GetOAuthProviderAsync(default,ct);

        _logger.LogInformation("Refreshing tokens via provider '{Provider}'...", authProvider.Name);

        var refreshed = await authProvider.RefreshAsync(ct);
        if (refreshed is null)
        {
            _logger.LogWarning("Token refresh via provider '{Provider}' failed (no tokens).", authProvider.Name);
            return false;
        }

        await _tokenCache.SaveAsync(authProvider.Name,refreshed, ct);
        _logger.LogInformation("Token refresh via provider '{Provider}' succeeded.",authProvider.Name);
        return true;
    }

    public async ValueTask<bool> LogoutAsync(
        IDispatcher? dispatcher,
        CancellationToken? cancellationToken = default)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var authProvider = await GetOAuthProviderAsync(default,ct);

        _logger.LogInformation("Logout via provider '{Provider}' starting...", authProvider.Name);

        var ok = await authProvider.LogoutAsync(dispatcher, ct);
        // Default provider behavior returns true and expects the token cache to be cleared by the service:
        if (ok)
        {
            await _tokenCache.ClearAsync(ct);
            LoggedOut?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Logout via provider '{Provider}' completed.", authProvider.Name);
        }
        else
        {
            _logger.LogWarning("Logout via provider '{Provider}' returned false.", authProvider.Name);
        }
        return ok;
    }
  private async Task<IOAuthProvider> GetOAuthProviderAsync(string? provider, CancellationToken ct)
    {
        IOAuthProvider? authProvider = null;
        if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTraceMessage($"Retrieving oAuth provider '{provider}'");
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = await _tokenCache.GetCurrentProviderAsync(ct);
            if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTraceMessage($"No provider specified, so retrieving current provider from token cache '{provider}'");
        }
        try
        {
            authProvider = _serviceProvider.GetRequiredService<IOAuthProvider>();
            
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve oAuth provider '{Provider}'", provider ?? _defaultProviderName);
            throw;
        }
        return  authProvider;
    }
}
