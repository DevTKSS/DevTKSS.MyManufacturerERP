namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;
public record OAuthSettings
{
    public AsyncFunc<IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? LoginCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
   
    // there is no logout in OAuth2
    public AsyncFunc<IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, bool>? LogoutCallback { get; init; }
}
