namespace DevTKSS.Extensions.OAuth.Services;
/// <summary>
/// Implemented by classes that are builders for the web authentication provider feature.
/// </summary>
public interface IOAuthBuilder : IBuilder
{
}

/// <summary>
/// Implemented by classes that are builders for the web authentication provider feature.
/// </summary>
/// <typeparam name="TService">
/// A service type that is used by the web authentication provider.
/// </typeparam>
public interface IOAuthBuilder<TService> : IOAuthBuilder
{
}