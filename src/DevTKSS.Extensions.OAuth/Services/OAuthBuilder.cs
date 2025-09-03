using Uno.Extensions;

namespace DevTKSS.Extensions.OAuth.Services;
internal record OAuthBuilder : BaseBuilder<OAuthSettings>, IOAuthBuilder
{
}
internal record OAuthBuilder<TService> : BaseBuilder<OAuthSettings<TService>>, IBuilder<OAuthSettings>, IOAuthBuilder<TService>
    where TService : notnull
{
    OAuthSettings IBuilder<OAuthSettings>.Settings { get => Settings; set => Settings = value as OAuthSettings<TService> ?? Settings; }
}


