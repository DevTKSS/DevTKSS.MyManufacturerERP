using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTKSS.Extensions.OAuth.Utils;

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

    partial void OnCurrentUriChanged(Uri? value)
    {
        if (value is not null && IsRedirectMatch(value))
        {
            _ = HandleRedirectAsync(value);
        }
    }

    private async Task HandleRedirectAsync(Uri destination)
    {
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

        await _navigator.NavigateBackWithResultAsync(this, data: destination.OriginalString);
    }

    [RelayCommand]
    private async Task ExecutePrimaryAsync()
    {
        if (CurrentUri is not null && IsRedirectMatch(CurrentUri))
        {
            await _navigator.NavigateBackWithResultAsync(this, data: CurrentUri.OriginalString);
        }
    }

    [RelayCommand]
    private async Task FinishAuthenticationAsync()
    {
        await _navigator.NavigateBackWithResultAsync(this, data: new { Success = true });
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
