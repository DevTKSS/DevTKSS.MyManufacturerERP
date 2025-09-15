namespace DevTKSS.Extensions.OAuth.OAuthServices;

// Inheriting here from Endpoint Options to get the possibility to use this Options directly with Refit client configuration in Uno also, but this isnt a pretty workaround
public class OAuthOptions : EndpointOptions
{
    public const string DefaultName = "OAuth";
    public OAuthClientOptions? ClientOptions { get; init; }
    public TokenCacheOptions TokenCacheOptions { get; init; } = new ();
    public UriTokenOptions UriTokenOptions { get; init; } = new ();
}
