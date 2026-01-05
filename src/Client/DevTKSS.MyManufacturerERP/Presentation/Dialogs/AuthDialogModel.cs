using Windows.System;

namespace DevTKSS.MyManufacturerERP.Presentation.Dialogs;

internal partial record AuthDialogModel
{
    private readonly INavigator _navigator;
    private readonly IDispatcher _dispatcher;
    private readonly AuthenticationRequest _request;
    public AuthDialogModel(
        INavigator navigator,
        IDispatcher dispatcher,
        IOAuthEndpoints refitAuthEndpoint,
        IOptions<OAuthEndpointOptions> options,
        AuthenticationRequest request)
    {
        _dispatcher = dispatcher;
        _navigator = navigator;
        _request = request;
    }
    
    public IState<Uri> CurrentUri => State<Uri>.Value(this, () => _request.RequestUri);

    public async Task ExecutePrimaryCommandAsync()
    {
        
    }

    public async Task FinishAuthentication()
    {
        await _navigator.NavigateBackWithResultAsync(this, data: new { Success = true });
    }
}
