using Uno.Extensions;

namespace DevTKSS.Extensions.OAuth.Services;

public record OAuthSettings
{
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, string>? PrepareLoginStartUri { get; init; }

    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>?, string, IDictionary<string, string>, IDictionary<string, string>?>? PostLoginCallback { get; init; }

    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
}
public record OAuthSettings<TService> : OAuthSettings where TService : notnull
{
    public new AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>?, string?, string>? PrepareLoginStartUri { get; init; }

    public new AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>?, string, IDictionary<string, string>, IDictionary<string, string>?>? PostLoginCallback { get; init; }

    public new AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
}