namespace DevTKSS.MyManufacturerERP.Extensions;
public interface IOAuthAuthenticationBuilder : IBuilder
{
}
/// <summary>
/// Implemented by classes that are builders for the oAuth authentication provider feature.
/// </summary>
/// <typeparam name="TService">
/// A service type that is used by the oAuth authentication provider.
/// </typeparam>
public interface IOAuthAuthenticationBuilder<TService> : IOAuthAuthenticationBuilder
{
}
