namespace DevTKSS.MyManufacturerERP.Presentation.Dialogs;

internal partial record AuthDialogModel
{
    private readonly INavigator _navigator;
    private readonly IDispatcher _dispatcher;
    private readonly WebAuthRequest _request;
    public AuthDialogModel(
        
        INavigator navigator,
        IDispatcher dispatcher,
        WebAuthRequest request)
    {
        _dispatcher = dispatcher;
        _navigator = navigator;
        _request = request;
    }
    
    public IState<Uri> CurrentUri => State<Uri>.Value(this, () => BuildStartUri())
                                               .ForEach(CurrentUriChanged);

    public async Task ExecutePrimaryCommandAsync()
    {
        var currentUri = await CurrentUri.Value();
        if (currentUri is not null && IsRedirectMatch(currentUri))
        {
            await _navigator.NavigateBackWithResultAsync(this, data: currentUri.OriginalString);
        }
    }

    public async Task FinishAuthentication()
    {
        await _navigator.NavigateBackWithResultAsync(this, data: new { Success = true });
    }

    internal async ValueTask CurrentUriChanged(Uri? destination, CancellationToken cancellationToken)
    {
        if (destination is null)
        {
            return;
        }

        if (!IsRedirectMatch(destination))
        {
            return;
        }

        var queryParameters = OAuth2Utilitys.GetQueryParameters(destination);
        if (queryParameters.TryGetErrorCode(out var error) && !string.IsNullOrWhiteSpace(error))
        {
            return;
        }

        queryParameters.TryGetCode(out var code);
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        await _navigator.NavigateBackWithResultAsync(this, data: destination.OriginalString, cancellation: cancellationToken);
    }

    private bool IsRedirectMatch(Uri? destination) => _request.IsRedirectMatch(destination);

    private Uri BuildStartUri()
    {
        if (Uri.TryCreate(_request.StartUrl, UriKind.Absolute, out var startUri))
        {
            return startUri;
        }

        return new Uri("about:blank");
    }
}
