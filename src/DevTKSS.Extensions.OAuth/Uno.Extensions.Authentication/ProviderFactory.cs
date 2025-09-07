namespace Uno.Extensions.Authentication;

internal record ProviderFactory<TProvider, TSettings>(string Name, TProvider Provider, TSettings Settings, Func<TProvider, TSettings, TProvider> ConfigureProvider) : IProviderFactory
    where TProvider : IAuthenticationProvider
{
    private IAuthenticationProvider? configuredProvider;
    public IAuthenticationProvider AuthenticationProvider => configuredProvider ??= ConfigureProvider(Provider, Settings);
}
