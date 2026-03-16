namespace DevTKSS.MyManufacturerERP.Presentation;

public partial record AuthModel
{
    private readonly IDispatcher _dispatcher;
    private readonly INavigator _navigator;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger _logger;

    public AuthModel(
        IDispatcher dispatcher, 
        INavigator navigator, 
        IAuthenticationService authenticationService,
        ILogger logger)
    {
        _dispatcher = dispatcher;
        _navigator = navigator;
        _authenticationService = authenticationService;
        _logger = logger.ForContext<AuthModel>();
    }

    public string Title { get; } = "Login";
    public IState<string> CurrentUri => State<string>.Empty(this);

    public IState<bool> IsLoading => State<bool>.Value(this, () => false);

    public IState<string?> ErrorMessage => State<string?>.Value(this, () => null);

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
    public async ValueTask ConnectToEtsy(CancellationToken token = default)
    {
        await IsLoading.UpdateAsync(_ => true, token);
        await ErrorMessage.UpdateAsync(_ => null, token);

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
                await ErrorMessage.UpdateAsync(_ => "Login failed or was cancelled. Please try again.", token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("OAuth login was cancelled by user");
            await ErrorMessage.UpdateAsync(_ => "Login was cancelled.", token);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception during OAuth login flow");
            await ErrorMessage.UpdateAsync(_ => $"An error occurred: {ex.Message}", token);
        }
        finally
        {
            await IsLoading.UpdateAsync(_ => false, token);
        }
    }
}