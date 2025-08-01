using DevTKSS.MyManufacturerERP.Infrastructure.Entitys;
using Refit;
using System.Net.Mime;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;

[Headers($"Content-Type: {MediaTypeNames.Application.Json}")]
public interface IEtsyUserEndpoints
{
    // Get current authenticated user (minimal info, requires shops_r scope)
    [Get("/v3/application/users/me")]
    Task<UserMe> GetMeAsync(
        [Header("Authorization")] string bearerToken,
        [Header("x-api-key")] string apiKey);

    // Get user details (requires email_r scope, user_id must be int64 >= 1)
    [Get("/v3/application/users/{user_id}")]
    Task<UserDetails> GetUserAsync(
        long user_id,
        [Header("Authorization")] string bearerToken,
        [Header("x-api-key")] string apiKey);
}
