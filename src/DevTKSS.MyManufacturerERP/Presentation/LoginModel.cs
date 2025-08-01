using DevTKSS.MyManufacturerERP.Infrastructure.Services;

namespace DevTKSS.MyManufacturerERP.Presentation;

public partial record LoginModel(IDispatcher Dispatcher, INavigator Navigator, OAuth2AuthenticationProvider Authentication)
{
    public string Title { get; } = "Login";


    public async ValueTask Login(CancellationToken token)
    {
        var success = await Authentication.LoginAsync(Dispatcher);
        if (success)
        {
            await Navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.ClearBackStack);
        }
    }

}
