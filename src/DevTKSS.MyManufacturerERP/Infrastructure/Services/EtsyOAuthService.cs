using System.Threading;
using DevTKSS.Extensions.OAuth.Endpoints;
using DevTKSS.Extensions.OAuth.Services;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

public sealed partial record EtsyOAuthService : OAuthProvider
{

    public const string ProviderName = "EtsyOAuth";
    private readonly IServiceProvider _serviceProvider;
    private readonly OAuthOptions _options;
    public EtsyOAuthService(
        ILogger<OAuthProvider> providerLogger,
        IOptionsSnapshot<OAuthOptions> options,
        IServiceProvider serviceProvider,
        IOAuthEndpoints authEndpoints,
        ISystemBrowserAuthBrokerProvider systemBrowser,
        ITokenCache tokenCache,
        string name = ProviderName) 
        : base(providerLogger, serviceProvider, authEndpoints,systemBrowser, tokenCache, name)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async ValueTask<IDictionary<string, string>?> ExchangeCodeForTokensAsync(
        IServiceProvider serviceProvider,
        IDictionary<string, string> tokens,
        string authCode,
        CancellationToken cancellationToken)
    {
        // First perform the standard code exchange
        var exchanged = await base.ExchangeCodeForTokensAsync(serviceProvider, tokens, authCode, cancellationToken);
        if (exchanged is null)
        {
            return null;
        }

        // Then enrich with Etsy specific "me" information if available
        try
        {
            if (await GetMeAsync(serviceProvider, exchanged, cancellationToken) is { } meTokens)
            {
                foreach (var kvp in meTokens)
                {
                    exchanged[kvp.Key] = kvp.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error enriching tokens with Etsy user info");
            // Do not fail login if enrichment fails; just return the exchanged tokens
        }

        return exchanged;
    }

    public async ValueTask<IDictionary<string, string>?> GetMeAsync(
      IServiceProvider serviceProvider,
      IDictionary<string, string> tokens,
      CancellationToken? cancellationToken = default)
    {
        var userEndpointsClient = serviceProvider.GetRequiredService<IEtsyUserEndpoints>();

        if (tokens == null || !tokens.TryGetAccessToken(out var accessToken) || string.IsNullOrWhiteSpace(accessToken))
        {
            base.Logger.LogWarning("No access token available");
            return default;
        }
        if (string.IsNullOrWhiteSpace(_options.ClientID))
        {
            base.Logger.LogError("Client ID is not configured.");
            return default;
        }
        try
        {
            var meResponse = await userEndpointsClient.GetMeAsync(accessToken, _options.ClientID, cancellationToken ?? CancellationToken.None);
            if (meResponse.ShopId == 0)
            {
                base.Logger.LogWarning("No shop associated with this user.");
                return default;
            }
            var result = new Dictionary<string, string>
            {
                [$"{_options.TokenCacheKeys.IdTokenKey}_{EtsyOAuthMeRequestDefaults.ShopIdKey}"] = meResponse.ShopId.ToString()
            };
            return result;
        }
        catch (ApiException apiEx)
        {
            base.Logger.LogError(apiEx, "API error during GetMeAsync: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return default;
        }
        catch (Exception ex)
        {
            base.Logger.LogError(ex, "Error fetching user info");
            return default;
        }
    }
}