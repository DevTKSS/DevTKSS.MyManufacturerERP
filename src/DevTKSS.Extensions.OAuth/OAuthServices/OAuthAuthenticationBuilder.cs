namespace DevTKSS.Extensions.OAuth.OAuthServices;

internal record OAuthAuthenticationBuilder : BaseBuilder<OAuthSettings>, IOAuthAuthenticationBuilder
{
}

internal record OAuthAuthenticationBuilder<TService> : BaseBuilder<OAuthSettings<TService>>, IBuilder<OAuthSettings>, IOAuthAuthenticationBuilder<TService>
	where TService : notnull
{
	OAuthSettings IBuilder<OAuthSettings>.Settings { get => Settings; set => Settings = value as OAuthSettings<TService> ?? Settings; }
}
