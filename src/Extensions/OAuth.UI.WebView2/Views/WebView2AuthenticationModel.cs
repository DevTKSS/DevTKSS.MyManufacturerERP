namespace DevTKSS.Extensions.Uno.Authentication.Desktop.UI;

public partial record WebView2AuthenticationModel
{
    private readonly ILogger _logger;
    private readonly INavigator _navigator;
    private readonly WebAuthRequest _request;
    public WebView2AuthenticationModel(
        ILogger<WebView2AuthenticationModel> logger,
        INavigator navigator,
        WebAuthRequest request)
    {
        _logger = logger;
        _navigator = navigator;
        _request = request;
    }
    public IState<bool> ReadyToClose => State.Value(this, () => false);

    public IState<string> Title => State.Value(this, () => "Authenticating");

    public IState<Uri> CurrentUrl => State<Uri>.Empty(this)
                                              .ForEach(CurrentUrlChanged);
    public async Task AuthenticateAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_request.StartUrl)
            || !Uri.TryCreate(_request.StartUrl, UriKind.Absolute, out var startUri)
            || (startUri.Scheme is not ("http" or "https")))
        {
            _logger.LogWarning("Invalid OAuth start URI (must be absolute http/https)");
            return;
        }

        if (string.IsNullOrWhiteSpace(_request.CallbackUrl)
            || !Uri.TryCreate(_request.CallbackUrl, UriKind.Absolute, out var redirectUri)
            || (redirectUri.Scheme is not ("http" or "https")))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Invalid OAuth start URI (must be absolute http/https)");
            }
            return;
        }
        await CurrentUrl.UpdateAsync(_ => startUri, ct);
    }
    // TODO: Code below this needs to be adapted to benefit from AuthorizationState passed to NavigateAuthenticationAsync
    // (or the INavigator extension method which provides it to the Auth method).
    // Assume that Updating/SetAsync on the CurrentUrl State will have the UI Layer WebView2 Navigate its Source to the provided Value.
    // Use 'CurrentUrl.ForEach' to monitor URL changes and check if we are on the AuthorizationState.RedirectUri.
    // You must not do any checks on it except from maybe Error check, then just return this to the NavigationResponse as Data or similar
    // (check the Uno Docs and sample provided for how to use NavigationResponse type best).
    private async ValueTask CurrentUrlChanged(Uri? arg, CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("{Name}: {Uri}", nameof(CurrentUrlChanged), arg?.OriginalString);
        }
        if (!IsRedirectMatch(arg))
        {
            return;
        }

        if (arg is null) return;

        var queryParameters = OAuth2Utilitys.GetQueryParameters(arg);

        if (queryParameters.TryGetErrorCode(out var error) && !string.IsNullOrWhiteSpace(error))
        {
            queryParameters.TryGetErrorDescription(out var errorDescription);

            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("OAuth callback returned an error: {Error} ({Description})", error, errorDescription);
            }
            return;
        }

        queryParameters.TryGetState(out var callbackState);
        queryParameters.TryGetCode(out var code);

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogInformation("OAuth callback missing authorization code.");
            return;
        }

        // State validation is handled by the OAuthTokenClient/NavigationService after the callback result is returned
        await _navigator.NavigateBackWithResultAsync(this, data: arg.OriginalString, cancellation: ct);
        await ReadyToClose.UpdateAsync(_ => true, ct);
    } 
    private bool IsRedirectMatch(Uri? destination) => _request.IsRedirectMatch(destination); // TODO: Implement this as extension in the Request?

    public async ValueTask NavigationStartedAsync(object? parameter, CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var actualType = parameter?.GetType().FullName;
            _logger.LogDebug(
                "{Handler} triggered. {ParameterName} actual type: {ActualType}",
                nameof(NavigationStartedAsync),
                nameof(parameter),
                actualType);
        }

#if DEBUG
        string? parameterPreview = parameter switch
        {
            Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs nav => nav.Uri,
            string uri => uri,
            _ => null,
        };
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "{Handler} {ParameterName} preview: {ParameterPreview}",
                nameof(NavigationStartedAsync),
                nameof(parameter),
                parameterPreview);
        }
#endif

        if (parameter is null)
        {
            return;
        }

        // Copilot added, because of heavily missing knowledge of SoC, Clean Code and MVVM/MVUX Knowledge, and is wrongly attempting to bring UI/View/ViewModel belongings into the MVUX Model layer
        // Fallback only (disabled for now - And will stay you stupid AI!): MVUX state binding + .ForEach(CurrentUrlChanged) Callbacks are the fucking only source of truth!
        // await IsNavigating.UpdateAsync(_ => true, ct);
        // switch (parameter)
        // {
        //     case Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs nav:
        //         await NavigationStarting(nav.Uri, ct);
        //         break;
        //     case string uri:
        //         await NavigationStarting(uri, ct);
        //         break;
        // }
    }

    public async ValueTask NavigationCompletedAsync(object? parameter, CancellationToken ct) // TODO: Currently not used, check if we need this e.g. if the WebView2.Source -> Navigate() is not triggering satisfyingly.
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var actualType = parameter?.GetType().FullName;
            _logger.LogDebug(
                "{Handler} triggered. {ParameterName} actual type: {ActualType}",
                nameof(NavigationCompletedAsync),
                nameof(parameter),
                actualType);
        }
#if DEBUG
        if (_logger.IsEnabled(LogLevel.Trace))
        { 
            var parameterPreview = parameter?.ToString();
            _logger.LogTrace(
                "{Handler} {ParameterName} preview: {ParameterPreview}",
                nameof(NavigationCompletedAsync),
                nameof(parameter),
                parameterPreview);
        }
#endif

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
    }

    public async ValueTask DocumentTitleChanged(string? title, CancellationToken ct) // TODO: Currently not used, triggered by WebView2Extensions Xaml DP / CommandExtensions
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            await Title.UpdateAsync(_ => title, ct);
        }
    }


   
}
