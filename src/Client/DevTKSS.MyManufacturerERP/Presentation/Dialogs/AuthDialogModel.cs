namespace DevTKSS.MyManufacturerERP.Presentation.Dialogs;

internal partial class AuthDialogModel : ObservableObject
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
        _currentUri = BuildStartUri();
    }

    [ObservableProperty]
    private Uri? _currentUri;

    [RelayCommand]
    private async Task HandleRedirectAsync(Uri? destination, CancellationToken ct)
    {
        if (destination is null || _request.IsRedirectMatch(destination)) return;
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

        await _navigator.NavigateBackWithResultAsync(this, data: destination.OriginalString,cancellation:  ct); // TODO: This Task should have a Return value not just a Task itself...
    }

    [RelayCommand]
    private async Task ExecutePrimaryAsync()
    {
        if (CurrentUri is not null && _request.IsRedirectMatch(CurrentUri))
        {
            await _navigator.NavigateBackWithResultAsync(this, data: CurrentUri.OriginalString);
        }
    }

    [RelayCommand]
    private async Task FinishAuthenticationAsync()
    {
        await _navigator.NavigateBackWithResultAsync(this, data: new { Success = true });
    }

    private Uri BuildStartUri()
    {
        if (Uri.TryCreate(_request.StartUrl, UriKind.Absolute, out var startUri))
        {
            return startUri;
        }

        return new Uri("about:blank");
    }
}
