namespace Uno.Extensions.Authentication;

internal interface IProviderFactory
{
    IAuthenticationProvider AuthenticationProvider { get; }
    string Name { get; }
}
