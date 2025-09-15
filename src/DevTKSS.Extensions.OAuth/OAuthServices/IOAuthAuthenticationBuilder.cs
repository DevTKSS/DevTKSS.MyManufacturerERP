namespace Uno.Extensions.Authentication.Web;

/// <summary>
/// Implemented by classes that are builders for the OAuth authentication provider feature.
/// </summary>
public interface IOAuthAuthenticationBuilder : IBuilder
{
}

/// <summary>
/// Implemented by classes that are builders for the OAuth authentication provider feature.
/// </summary>
/// <typeparam name="TService">
/// A service type that is used by the web authentication provider.
/// </typeparam>
public interface IOAuthAuthenticationBuilder<TService> : IOAuthAuthenticationBuilder
{
}
