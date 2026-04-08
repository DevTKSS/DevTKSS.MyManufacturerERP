namespace DevTKSS.Extensions.OAuth.UI.WebView.Views;

public partial record WebView2AuthenticationModel
{
    private readonly ILogger _logger;
    private readonly INavigator _navigator;
    private readonly IOAuthTokenClient _tokenClient;
    private readonly WebAuthRequest _authRequest;

    public WebView2AuthenticationModel(
        ILogger<WebView2AuthenticationModel> logger,
        INavigator navigator,
        IOAuthTokenClient tokenClient,
        AuthNavigationRequest authRequest)
    {
        _logger = logger;
        _navigator = navigator;
        _tokenClient = tokenClient;
        _authRequest = authRequest;
    }
    public IState<string> Title => State.Value(this, () => "Authenticating");

    public IState<Uri> CurrentUrl => State<Uri>.Empty(this).ForEach(CurrentUrlChanged);

    private async ValueTask CurrentUrlChanged(Uri? arg, CancellationToken ct)
    {
#if DEBUG
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{Name}: {Uri}", nameof(CurrentUrlChanged), arg?.OriginalString);
        }
#endif
        if (arg is null) return;
        if (!IsBrowserNavigatingToRedirectUri(arg)) return;

        var tokenResponse = await _tokenClient.HandleCallbackAsync(arg, ct);
        if (tokenResponse is null) return;

        await _navigator.NavigateBackWithResultAsync(this, data: tokenResponse, cancellation: ct);
       
    }

    public async ValueTask NavigationStarting(string? uri, CancellationToken ct) // TODO: Currently not used, triggered by WebView2Extensions Xaml DP / CommandExtensions
    {
        if(_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{Handler} triggered. {ParameterName} preview: {ParameterPreview}",
                nameof(NavigationStarting),
                nameof(uri),
                uri);
        }

        if (!string.IsNullOrWhiteSpace(uri) && IsBrowserNavigatingToRedirectUri(new Uri(uri)))
        {

        }
    }

    private bool IsBrowserNavigatingToRedirectUri(Uri uri)
    {
        return uri.AbsoluteUri.StartsWith(_authRequest.CallbackUrl);
    }
}
