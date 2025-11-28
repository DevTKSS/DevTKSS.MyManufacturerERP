using DesktopAuthenticationIntegration;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

public sealed partial record EtsyOAuthService
{
    [GeneratedRegex(@"^(?<userId>\d+)\.(?<token>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture)]
    public static partial Regex DoesContainUserId();

    public const string ProviderName = "EtsyOAuth";
    private readonly IServiceProvider _serviceProvider;
    private readonly EtsyOAuthEndpointOptions _options;
    private readonly ILogger _logger;
    public EtsyOAuthService(
        ILogger logger,
        IOptionsSnapshot<EtsyOAuthEndpointOptions> configuration,
        IServiceProvider serviceProvider,
        IOAuthEndpoints authEndpoints,
        ITokenCache tokenCache,
        ISystemBrowserAuthBrokerProvider systemBrowserAuthBrokerProvider)
    {
        _logger = logger.ForContext<EtsyOAuthService>();
        _serviceProvider = serviceProvider;
        _options = configuration.Value;
    }

    public async ValueTask<IDictionary<string, string>?> ExchangeCodeForTokensAsync(
        IServiceProvider serviceProvider,
        ITokenCache tokenCache,
        IDictionary<string, string> tokens,
        CancellationToken cancellationToken)
    {
        tokens.TryGetRefreshToken(out var refreshToken);
        if (DoesContainUserId().Match(refreshToken!) is { Success: true } match)
        {
            var userId = match.Groups["userId"].Value;
            _logger.Information("Logged in as user ID: {userId}", userId);
            // Store user id under configured user id token key
            tokens.AddOrReplace(_options.UserIdTokenKey, userId);
        }
        // Then enrich with Etsy specific "me" information if available
        try
        {
            if (await GetMeAsync(serviceProvider, tokens, cancellationToken) is { } meTokens)
            {
                foreach (var kvp in meTokens)
                {
                    tokens[kvp.Key] = kvp.Value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error enriching tokens with Etsy user info");
        }

        return tokens;
    }

    public async ValueTask<IDictionary<string, string>?> GetMeAsync(
      IServiceProvider serviceProvider,
      IDictionary<string, string> tokens,
      CancellationToken? cancellationToken = default)
    {
        var userEndpointsClient = serviceProvider.GetRequiredService<IEtsyUserEndpoints>();

        if (tokens == null || !tokens.TryGetAccessToken(out var accessToken) || string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.Warning("No access token available");
            return default;
        }
        if (_options.ClientId is not string clientID
            || string.IsNullOrWhiteSpace(clientID))
        {
            _logger.Error("Client ID is not configured.");
            return default;
        }
        try
        {
            var meResponse = await userEndpointsClient.GetMeAsync(accessToken, clientID, cancellationToken ?? CancellationToken.None);
            if (meResponse.ShopId == 0)
            {
                _logger.Warning("No shop associated with this user.");
                return default;
            }
            var result = new Dictionary<string, string>
            {
                [$"{_options.TokenKeys.IdTokenKey}_{_options.ShopIdTokenKey}"] = meResponse.ShopId.ToString(),
                // Store shop id under configured shop id token key
                [_options.ShopIdTokenKey] = meResponse.ShopId.ToString(),
                // Optionally also store user id if available
                [_options.UserIdTokenKey] = meResponse.UserId.ToString() ?? (tokens.ContainsKey(_options.UserIdTokenKey) ? tokens[_options.UserIdTokenKey] : string.Empty)
            };
            return result;
        }
        catch (ApiException apiEx)
        {
            _logger.Error(apiEx, "API error during GetMeAsync: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return default;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching user info");
            return default;
        }
    }
}