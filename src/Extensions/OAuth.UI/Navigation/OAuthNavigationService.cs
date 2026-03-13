namespace DevTKSS.Extensions.OAuth.UI.Navigation;

public sealed class OAuthNavigationService : IOAuthNavigationService
{
    private readonly IOAuthTokenClient _tokenClient;
    private readonly ISystemBrowserAuthBrokerProvider? _systemBrowser;
    private readonly ILogger<OAuthNavigationService> _logger;

    public OAuthNavigationService(
        IOAuthTokenClient tokenClient,
        ILogger<OAuthNavigationService> logger,
        ISystemBrowserAuthBrokerProvider? systemBrowser = null)
    {
        _tokenClient = tokenClient;
        _logger = logger;
        _systemBrowser = systemBrowser;
    }

    /// <inheritdoc/>
    public bool IsNavigatingAuthentication { get; private set; }

    /// <inheritdoc/>
    public async ValueTask<TokenResponse?> NavigateAuthenticationAsync(
        INavigator navigator,
        string qualifier,
        object? sender = null,
        IDictionary<string, string>? extraParameters = null,
        CancellationToken cancellationToken = default)
    {
        if (IsNavigatingAuthentication)
        {
            return default;
        }

        IsNavigatingAuthentication = true;
        try
        {
            var state = await _tokenClient.GetAuthorizeStateAsync(extraParameters);
            var request = await _tokenClient.GetWebAuthRequestAsync(state, extraParameters);
            if (request is null)
            {
                return default;
            }

            if (!Uri.TryCreate(request.StartUrl, UriKind.Absolute, out var startUri)
                || !Uri.TryCreate(request.CallbackUrl, UriKind.Absolute, out var callbackUri))
            {
                return default;
            }

            string? callbackResult = qualifier switch
            {
                OAuthNavigationQualifiers.SystemBrowser => await AuthenticateWithSystemBrowser(startUri, callbackUri, cancellationToken),
                _ when sender is not null => await navigator.NavigateDataForResultAsync<WebAuthRequest, string>(
                        sender,
                        request,
                        qualifier: qualifier,
                        cancellation: cancellationToken),
                _ => default
            };

            if (string.IsNullOrWhiteSpace(callbackResult))
            {
                return default;
            }

            return await _tokenClient.ExchangeCodeAsync(state, callbackResult, cancellationToken);
        }
        finally
        {
            IsNavigatingAuthentication = false;
        }
    }

    private async ValueTask<string?> AuthenticateWithSystemBrowser(Uri startUri, Uri callbackUri, CancellationToken cancellationToken)
    {
        if (_systemBrowser is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("System browser authentication is not available.");
            }
            return default;
        }

        var result = await _systemBrowser.AuthenticateAsync(WebAuthenticationOptions.None, startUri, callbackUri, cancellationToken);
        return result?.ResponseData;
    }
}
