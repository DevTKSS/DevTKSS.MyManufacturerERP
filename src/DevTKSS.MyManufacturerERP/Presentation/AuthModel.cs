namespace DevTKSS.MyManufacturerERP.Presentation;

public partial record AuthModel
{
    private readonly IDispatcher _dispatcher;
    private readonly INavigator _navigator;
    private readonly IAuthenticationService _authenticationService;

    public AuthModel(IDispatcher Dispatcher, INavigator Navigator, IAuthenticationService Authentication)
    {
        _dispatcher = Dispatcher;
        _navigator = Navigator;
        _authenticationService = Authentication;
    }
    public string Title { get; } = "Login";
    public IState<Uri> CurrentUri { get; } = State<Uri>.Value(this,() => new Uri("https://example.com/login"));

    public async ValueTask Login(CancellationToken token)
    {
        var success = await _authenticationService.LoginAsync(_dispatcher);
        if (success)
        {
            await _navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.ClearBackStack);
        }
    }
}