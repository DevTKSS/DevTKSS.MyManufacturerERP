namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;

[Headers($"Content-Type: application/json")]
public interface IEtsyUserEndpoints
{
    // Get current authenticated user (minimal info, requires shops_r scope)
    [Get("/v3/application/users/me")]
    Task<UserMeResponse> GetMeAsync(
        [Authorize("Bearer")] string bearerToken,
        [Header("x-api-key")] string apiKey,
        CancellationToken cancellationToken = default);

    // Get user details (requires email_r scope, user_id must be int64 >= 1)
    [Get("/v3/application/users/{user_id}")]
    Task<UserDetailsResponse> GetUserAsync(
        long user_id,
        [Authorize(scheme: "Bearer")] string bearerToken,
        [Header("x-api-key")] string apiKey,
        CancellationToken cancellationToken = default);
}
