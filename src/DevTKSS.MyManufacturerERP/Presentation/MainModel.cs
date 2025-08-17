namespace DevTKSS.MyManufacturerERP.Presentation;

public partial record MainModel
{
    private INavigator _navigator;

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        IAuthenticationService authenticationService,
        INavigator navigator)
    {
        _navigator = navigator;
        _authenticationService = authenticationService;
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task GoToSecond()
    {
        var name = await Name;
        await _navigator.NavigateViewModelAsync<SecondModel>(this, data: new Entity(name!));
    }

    public async ValueTask ConnectToEtsy(CancellationToken token)
    {
        await _authenticationService.LogoutAsync(token);
    }

    private IAuthenticationService _authenticationService;
}
