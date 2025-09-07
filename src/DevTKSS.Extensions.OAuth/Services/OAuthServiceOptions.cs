namespace DevTKSS.Extensions.OAuth.Services;

/// <summary>
/// Options for the AuthenticationService wrapper.
/// </summary>
public sealed class OAuthServiceOptions
{
    public const string DefaultSectionName = "AuthenticationServiceOptions";
    public string DefaultProviderName { get; init; } = OAuthProvider.DefaultName;
    public string[] ProviderNames { get; set; } = [OAuthProvider.DefaultName];
}
