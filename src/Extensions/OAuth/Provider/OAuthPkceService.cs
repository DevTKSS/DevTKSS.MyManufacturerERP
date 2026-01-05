

namespace DevTKSS.Extensions.OAuth.Provider;
public sealed record AuthorizationRequest(
        Uri StartUri,
        string ExpectedState,
        string CodeVerifier);

public record AuthorizationResponse(
        string Code,
        string State);
public record AuthorizationErrorResponse(
    string Code,
    string State,
    string Error,
    string ErrorDescription,
    string? ErrorUri)
    : AuthorizationResponse(Code,State);
public sealed class OAuthPkceService
{

    private readonly IOAuthEndpoints _oauth;
    private readonly OAuthEndpointOptions _options;
    private readonly ILogger<OAuthPkceService> _logger;

    private string _codeVerifier = string.Empty;
    private string _state = string.Empty;
    public OAuthPkceService(
        IOAuthEndpoints oauth,
        IOptions<OAuthEndpointOptions> options,
        ILogger<OAuthPkceService> logger)
    {
        _oauth = oauth;
        _options = options.Value;
        _logger = logger;

        _options.Valdiate();
    }

    public async Task<AuthorizationRequest> CreateAuthorizationRequestAsync(CancellationToken ct) // TODO: Change return Type!
    {
        _options.Valdiate();
        _state = OAuth2Utilitys.GenerateState();
        _codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(_codeVerifier) ?? string.Empty;

        var request = new AuthorizationCodeRequest
        {
            ClientId = _options.ClientId,
            RedirectUri = _options.RedirectUri,
            Scope = string.Join(" ", _options.Scopes),
            State = _state,
            CodeChallenge = _options.UsePkce ? codeChallenge : string.Empty,
        };

        var response = await _oauth.AuthenticateAsync(request, ct);

        var startUri = response.RequestMessage?.RequestUri
            ?? throw new InvalidOperationException("Refit did not provide the request URI for the authorization request.");

        return new AuthorizationRequest(startUri, _state, _codeVerifier); // TODO: We should not return the code verifier or state! Keep this internal to the service.
    }

    // Keep old method for compatibility; prefer async version.
    public AuthorizationRequest CreateAuthorizationRequest() // TODO: Change return Type!
        => CreateAuthorizationRequestAsync(CancellationToken.None).GetAwaiter().GetResult(); // TODO: NEVER EVER use .GetAwaiter().GetResult() !!!

    public AuthorizationResponse ParseCallback(Uri callbackUri) // TODO: DO NOT return code and state! Instead Proceed with auth flow!
    {
        var qp = callbackUri.GetParameters();

        if (qp.TryGetErrorCode(out var errorCode))
        {
            qp.TryGetErrorDescription(out var errorDescription);
            qp.TryGetErrorUri(out var errorUri);

            return new AuthorizationErrorResponse(
                Code: string.Empty,
                State: string.Empty,
                Error: errorCode,
                ErrorDescription: errorDescription ?? string.Empty,
                ErrorUri: errorUri);
        }

        qp.TryGetState(out var state);
        qp.TryGetCode(out var code);

        return new AuthorizationResponse(
            Code: code ?? string.Empty,
            State: state ?? string.Empty);
    }

    public async Task<TokenResponse> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken ct)
    {
        var request = new AccessTokenRequest
        {
            ClientId = _options.ClientId,
            RedirectUri = _options.RedirectUri!,
            Code = code,
            CodeVerifier = _options.UsePkce ? codeVerifier : string.Empty,
        };

        _logger.LogInformation("Exchanging authorization code for tokens");
        return await _oauth.ExchangeCodeAsync(request, ct);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var request = new RefreshTokenRequest
        {
            ClientId = _options.ClientId!,
            RefreshToken = refreshToken,
        };

        _logger.LogInformation("Refreshing OAuthDefaults tokens");
        return await _oauth.RefreshTokenAsync(request, ct);
    }
}
