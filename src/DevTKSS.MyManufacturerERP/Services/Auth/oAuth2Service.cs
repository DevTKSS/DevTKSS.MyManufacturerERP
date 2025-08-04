using DevTKSS.MyManufacturerERP.Extensions;

namespace DevTKSS.MyManufacturerERP.Services.Auth;

internal static class AuthenticationBuilderExtensions
{
    public static IAuthenticationBuilder AddOAuth(
        this IAuthenticationBuilder builder,
        Action<IOAuthAuthenticationBuilder>? configure = default,
        string name = OAuthAuthenticationProvider.DefaultName)
    {
        return builder.AddProvider<OAuthAuthenticationProvider>();
    }
}
internal class oAuth2Service : IAuthenticationService
{
    public string[] Providers => [nameof(OAuthAuthenticationProvider)];

    public event EventHandler LoggedOut;

    public ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> LoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials = null, string? provider = null, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, CancellationToken? cancellationToken = null)
    {
        throw new NotSupportedException("OAuth does not support actual Logout operation");
    }

    public ValueTask<bool> RefreshAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
}
