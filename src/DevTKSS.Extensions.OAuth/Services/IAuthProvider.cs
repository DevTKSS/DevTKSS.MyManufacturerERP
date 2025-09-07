namespace DevTKSS.Extensions.OAuth.Services;

internal interface IAuthProvider : IAuthenticationProvider
{
    public OAuthSettings Settings { get; }
    public void Configure(OAuthOptions options, OAuthSettings settings);

}
