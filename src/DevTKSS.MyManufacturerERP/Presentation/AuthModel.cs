namespace DevTKSS.MyManufacturerERP.Presentation;
// Uncomment as soon as WebAuthentication is working in Uno on desktop target
//public partial record AuthModel(IDispatcher Dispatcher, INavigator Navigator, IAuthenticationService Authentication)
//{
//    public string Title { get; } = "Login";


//    public async ValueTask Login(CancellationToken token)
//    {
//        var success = await Authentication.LoginAsync(Dispatcher);
//        if (success)
//        {
//            await Navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.ClearBackStack);
//        }
//    }

//}
public partial record AuthModel(IDispatcher Dispatcher, INavigator Navigator, IAuthenticationService Authentication)
{
    public string Title { get; } = "Connect to your external Data";

    public IState<string> Username => State<string>.Value(this, () => string.Empty);

    public IState<string> Password => State<string>.Value(this, () => string.Empty);

    public async ValueTask ConnectToEtsy(CancellationToken ct)
    {
        var username = await Username ?? string.Empty;
        var password = await Password ?? string.Empty;

        var success = await Authentication.LoginAsync(Dispatcher, provider: "EtsyOAuth",cancellationToken: ct);
        if (success)
        {
            await Navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.ClearBackStack);
        }
    }

}
