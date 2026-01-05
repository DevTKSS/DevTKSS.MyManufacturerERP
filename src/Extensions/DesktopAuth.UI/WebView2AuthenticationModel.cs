namespace DevTKSS.Extensions.Uno.Authentication.Desktop.UI;

public partial record WebView2AuthenticationModel
{
    private readonly ILogger _logger;
    private readonly IOAuthEndpoints _oAuthEndpoints;
    private WebView2AuthenticationOptions? _options;

    public WebView2AuthenticationModel(
        ILogger<WebView2AuthenticationModel> logger,
        IOAuthEndpoints oAuthEndpoints,
        IOptions<WebView2AuthenticationOptions>? options = null)
    {
        _logger = logger;
        _oAuthEndpoints = oAuthEndpoints;
        _options = options?.Value;
    }

    public WebView2AuthenticationModel(
        ILogger<WebView2AuthenticationModel> logger,
        Action<WebView2AuthenticationOptions> configuration,
        IOAuthEndpoints oAuthEndpoints)
    {
        _logger = logger;
        _oAuthEndpoints = oAuthEndpoints;
        var options = new WebView2AuthenticationOptions();
        configuration(options);
        _options = options;
    }

    public IState<bool> IsNavigating => State.Value(this, () => false);

    public IState<bool> ReadyToClose => State.Value(this, () => false);

    public IState<string> Title => State.Value(this, () => "Authenticating");

    public IState<Uri> CurrentUrl => State<Uri>.Empty(this)
                                               .ForEach(CurrentUrlChanged);

    public async Task AuthenticateAsync(CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_options?.StartUri, nameof(_options.StartUri));

        var expectedState = Guid.NewGuid().ToString("N");
        var codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier) ?? string.Empty;

        var start = _options.StartUri;
        var ub = new UriBuilder(start);

        var qp = OAuth2Utilitys.GetParameters(start);
        qp[OAuthDefaults.Keys.State] = expectedState;
        qp[OAuthDefaults.Keys.Pkce.CodeChallenge] = codeChallenge;
        qp[OAuthDefaults.Keys.Pkce.CodeChallengeMethod] = OAuthDefaults.Values.S256;

        ub.Query = string.Join("&", qp.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        _options = _options with
        {
            StartUri = ub.Uri,
            ExpectedState = expectedState,
            CodeVerifier = codeVerifier,
        };

        await CurrentUrl.UpdateAsync(_ => _options.StartUri!, ct);
    }

    private async ValueTask CurrentUrlChanged(Uri? arg, CancellationToken ct)
    {
        if (!IsRedirectMatch(arg, _options?.EndUri))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(arg);

        await CallbackUri.UpdateAsync(_ => arg.OriginalString, ct);

        var qp = OAuth2Utilitys.GetParameters(arg);

        if (qp.TryGetValue(OAuthDefaults.Keys.Error.Key, out var error) && !string.IsNullOrWhiteSpace(error))
        {
            var errorDescription = qp.TryGetValue(OAuthDefaults.Keys.Error.Description, out var desc)
                ? desc
                : error;

            await ReadyToClose.UpdateAsync(_ => true, ct);
            return;
        }

        qp.TryGetValue(OAuthDefaults.Keys.State, out var callbackState);
        qp.TryGetValue(OAuthDefaults.Keys.Code, out var code);

        if (!string.IsNullOrWhiteSpace(callbackState))
        {
            await CallbackState.UpdateAsync(_ => callbackState, ct);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            await OAuthError.UpdateAsync(_ => "missing_authorization_code", ct);
            await ReadyToClose.UpdateAsync(_ => true, ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_options?.ExpectedState)
            && !string.Equals(_options.ExpectedState, callbackState, StringComparison.Ordinal))
        {
            await OAuthError.UpdateAsync(_ => "invalid_state", ct);
            await ReadyToClose.UpdateAsync(_ => true, ct);
            return;
        }

        await AuthorizationCode.UpdateAsync(_ => code, ct);
        await ReadyToClose.UpdateAsync(_ => true, ct);
    }

    // Bound from XAML via WebView2Extensions.NavigationStartingCommand
    public async ValueTask NavigationStartedAsync(object? args, CancellationToken ct)
    {
        if (args is null)
        {
            return;
        }


    }

    // Bound from XAML via WebView2Extensions.NavigationCompletedCommand
    public async ValueTask NavigationCompletedAsync(object? args, CancellationToken ct)
    {
        await IsNavigating.UpdateAsync(_ => false, ct);
    }

    public async ValueTask NavigationStarting(string? uri, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return;
        }

        await IsNavigating.UpdateAsync(_ => uri.StartsWith("about:blank", StringComparison.OrdinalIgnoreCase), ct);

        await InterceptIfRedirectUriReached(uri, ct); // TODO: no best practice. use ForEach callback from State<Uri> and compare if we are at destination!
    }

    public async ValueTask DocumentTitleChanged(string? title, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            await Title.UpdateAsync(_ => title, ct);
        }
    }

    private static bool IsRedirectMatch(Uri? destination, Uri? expected)
    {
        if (destination is null || expected is null)
        {
            return false;
        }

        return destination.Scheme.Equals(expected.Scheme, StringComparison.OrdinalIgnoreCase)
               && destination.Authority.Equals(expected.Authority, StringComparison.OrdinalIgnoreCase)
               && destination.AbsolutePath.Equals(expected.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }

    private async ValueTask InterceptIfRedirectUriReached(string destinationUri, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(destinationUri);
        ArgumentNullException.ThrowIfNull(_options?.EndUri);

        if (!Uri.TryCreate(destinationUri, UriKind.Absolute, out var destinationUrl))
        {
            return;
        }

        if (!IsRedirectMatch(destinationUrl, _options.EndUri))
        {
            return;
        }

        _logger.LogInformation("Redirect Uri was reached.");

        await ReadyToClose.UpdateAsync(_ => true, ct);
    }
}