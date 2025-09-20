using DevTKSS.Extensions.OAuth.Endpoints;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

public sealed partial record EtsyOAuthService : OAuthProvider
{
    [GeneratedRegex(@"^(?<userId>\d+)\.(?<token>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture)]
    public static partial Regex DoesContainUserId();

    public const string ProviderName = "EtsyOAuth";
    private readonly IServiceProvider _serviceProvider;
    private readonly OAuthOptions _options;
    public EtsyOAuthService(
        ILogger<EtsyOAuthService> providerLogger,
        ILogger<OAuthProvider> baseLogger,
        IOptionsSnapshot<OAuthOptions> Configuration,
        IServiceProvider ServiceProvider,
        IOAuthEndpoints AuthEndpoints,
        ITokenCache tokenCache,
        ISystemBrowserAuthBrokerProvider systemBrowserAuthBrokerProvider) 
        : base(baseLogger, ServiceProvider, AuthEndpoints, tokenCache, Configuration, systemBrowserAuthBrokerProvider)
    {
        _serviceProvider = ServiceProvider;
        _options = Configuration.Value;
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
            var token = match.Groups["token"].Value;
            ProviderLogger.LogInformation("Logged in as user ID: {userId}", userId);
            tokens.AddOrReplace(InternalSettings.TokenCacheOptions.IdTokenKey, userId);
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
            ProviderLogger.LogError(ex, "Error enriching tokens with Etsy user info");
            // Do not fail login if enrichment fails; just return the exchanged tokens
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
            base.ProviderLogger.LogWarning("No access token available");
            return default;
        }
        if (InternalSettings.ClientOptions?.ClientID is not string clientID
            || string.IsNullOrWhiteSpace(clientID))
        {
            base.ProviderLogger.LogError("Client ID is not configured.");
            return default;
        }
        try
        {
            var meResponse = await userEndpointsClient.GetMeAsync(accessToken, clientID, cancellationToken ?? CancellationToken.None);
            if (meResponse.ShopId == 0)
            {
                base.ProviderLogger.LogWarning("No shop associated with this user.");
                return default;
            }
            var result = new Dictionary<string, string>
            {
                [$"{_options.TokenCacheOptions.IdTokenKey}_{EtsyOAuthMeRequestDefaults.ShopIdKey}"] = meResponse.ShopId.ToString()
            };
            return result;
        }
        catch (ApiException apiEx)
        {
            base.ProviderLogger.LogError(apiEx, "API error during GetMeAsync: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return default;
        }
        catch (Exception ex)
        {
            base.ProviderLogger.LogError(ex, "Error fetching user info");
            return default;
        }
    }
}