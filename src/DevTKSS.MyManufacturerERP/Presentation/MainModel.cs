namespace DevTKSS.MyManufacturerERP.Presentation;

public partial record MainModel
{
    private readonly INavigator _navigator;
    private readonly IAuthenticationService _authenticationService;
    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        IAuthenticationService authenticationService,
        INavigator navigator)
    {
        _navigator = navigator;
        _authenticationService = authenticationService;
    }

    public async Task GoToWebView()
    {
        await _navigator.NavigateViewModelAsync<WebViewBrowserModel>(this, data: "https://www.google.com/");
    }

    public async ValueTask LogoutFromEtsy(CancellationToken token)
    {
        await _authenticationService.LogoutAsync(token);
    }

}
