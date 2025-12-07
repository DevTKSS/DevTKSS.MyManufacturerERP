namespace DevTKSS.MyManufacturerERP.Presentation;

public partial record AuthModel
{
    private readonly IDispatcher _dispatcher;
    private readonly INavigator _navigator;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthModel> _logger;

    public AuthModel(
        IDispatcher Dispatcher, 
        INavigator Navigator, 
        IAuthenticationService Authentication,
        ILogger<AuthModel> Logger)
    {
        _dispatcher = Dispatcher;
        _navigator = Navigator;
        _authenticationService = Authentication;
        _logger = Logger;
    }

    public string Title { get; } = "Login";
    public IState<string> CurrentUri => State<string>.Value(this, () => "https://example.com/login");

    public IState<bool> IsLoading => State<bool>.Value(this, () => false);

    public IState<string?> ErrorMessage => State<string?>.Value(this, () => null);

    /// <summary>
    /// Initiates OAuth login flow with Etsy via WebAPI.
    /// This method:
    /// 1. Calls IAuthenticationService.LoginAsync() which triggers HandleLoginAsync
    /// 2. HandleLoginAsync calls WebAPI /auth/login endpoint
    /// 3. WebAPI redirects to Etsy OAuth login page
    /// 4. User authenticates with Etsy
    /// 5. Etsy redirects back to WebAPI /auth/callback/etsy
    /// 6. WebAPI sets authentication cookie and redirects back to client
    /// 7. User is navigated to MainModel upon success
    /// </summary>
    public async ValueTask ConnectToEtsy(CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation("Starting OAuth login flow with Etsy");
            
            var success = await _authenticationService.LoginAsync(_dispatcher);
            
            if (success)
            {
                _logger.LogInformation("OAuth login successful, navigating to main page");
                await _navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.ClearBackStack);
            }
            else
            {
                _logger.LogWarning("OAuth login failed or was cancelled");
                // Error will be displayed in the UI via ErrorMessage state
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during OAuth login flow");
            // Set error message for UI display
        }
    }
}