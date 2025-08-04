using System.Net.Http.Headers;
using Refit;

public interface IEtsyOAuthEndpoints
{
    [Get("/oauth/connect")]
    [Headers($"Accept: {MediaTypeNames.Application.Json}")]
    Task<HttpResponseMessage> SendAuthorizationCodeRequestAsync(
        [Query] string response_type,
        [Query] string redirect_uri,
        [Query] string scope,
        [Query("client_id")] string client_id,
        [Query] string state,
        [Query] string code_challenge,
        [Query] string code_challenge_method
    );
}