using Refit;

public interface IEtsyOAuthEndpoints
{
    [Get("/oauth/connect")]
    [Headers("Accept: application/json")]
    [Pkce]
    Task<HttpResponseMessage> RequestAuthorizationCodeAsync(
        [Query] string response_type,
        [Query] string redirect_uri,
        [Query] string scope,
        [Header("x-api-key")] string client_id,
        [Query] string state,
        [Query] string code_challenge,
        [Query] string code_challenge_method
    );
}