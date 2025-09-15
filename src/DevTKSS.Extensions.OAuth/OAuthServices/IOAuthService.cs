using DevTKSS.Extensions.OAuth.Endpoints;

namespace DevTKSS.Extensions.OAuth.OAuthServices;
public interface IOAuthService
{
    IOAuthEndpoints AuthEndpoints { get; init; }
    IOptionsSnapshot<OAuthOptions> Configuration { get; init; }
    string Name { get; init; }
    OAuthSettings? Settings { get; init; }
    ITokenCache Tokens { get; init; }

    ValueTask<IDictionary<string, string>?> PostLoginAsync(IDictionary<string, string> tokens, string redirectUri, CancellationToken cancellationToken);
    ValueTask<string> PrepareLoginStartUri(string? loginStartUri, CancellationToken token);
    ValueTask<IDictionary<string, string>?> RefreshAsync(CancellationToken cancellationToken);
}