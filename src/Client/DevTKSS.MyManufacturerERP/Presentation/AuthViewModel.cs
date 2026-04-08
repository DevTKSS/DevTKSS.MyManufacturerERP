namespace DevTKSS.MyManufacturerERP.Presentation;

[ReactiveBindable(false)]
public partial class AuthViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;
    private readonly INavigator _navigator;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger _logger;

    public AuthViewModel(
        IDispatcher dispatcher, 
        INavigator navigator, 
        IAuthenticationService authenticationService,
        ILogger logger)
    {
        _dispatcher = dispatcher;
        _navigator = navigator;
        _authenticationService = authenticationService;
        _logger = logger.ForContext<AuthViewModel>();
    }

    public string Title { get; } = "Login";

    [ObservableProperty]
    private string? _currentUri;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Initiates OAuthDefaults login flow with Etsy via WebAPI.
    /// This method:
    /// 1. Calls <see cref="IAuthenticationService.LoginAsync"/> with the Custom provider which triggers HandleLoginCallbackAsync
    /// 2. HandleLoginCallbackAsync calls WebAPI /auth/login endpoint
    /// 3. WebAPI redirects to Etsy OAuthDefaults login page
    /// 4. User authenticates with Etsy
    /// 5. Etsy redirects back to WebAPI /auth/callback/etsy
    /// 6. WebAPI sets authentication cookie and redirects back to client
    /// 7. User is navigated to MainModel upon success
    /// </summary>
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ConnectToEtsyAsync(CancellationToken token)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            _logger.Information("Starting OAuth login flow with Etsy");

            var success = await _authenticationService.LoginAsync(_dispatcher, null, "Custom", token);

            if (success)
            {
                _logger.Information("OAuth login successful, navigating to main page");
                await _navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.ClearBackStack);
            }
            else
            {
                _logger.Warning("OAuth login failed or was cancelled");
                ErrorMessage = "Login failed or was cancelled. Please try again.";
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("OAuth login was cancelled by user");
            ErrorMessage = "Login was cancelled.";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception during OAuth login flow");
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
