namespace DevTKSS.Extensions.OAuth.Services;

internal interface IOAuthProvider
{
    public string Name { get; }
    public OAuthSettings Settings { get; }
    public void Configure(OAuthOptions options, OAuthSettings settings);

    public ValueTask<IDictionary<string, string>?> LoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials, CancellationToken cancellationToken);

    public ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, CancellationToken cancellationToken);

    public ValueTask<IDictionary<string, string>?> RefreshAsync(CancellationToken cancellationToken);
}
