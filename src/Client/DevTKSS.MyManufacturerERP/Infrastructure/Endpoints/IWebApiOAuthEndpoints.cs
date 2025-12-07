namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;

/// <summary>
/// Refit client for WebAPI OAuth endpoints.
/// Replaces direct Etsy API integration - all OAuth is handled by WebAPI.
/// </summary>
[Headers("Content-Type: application/json")]
public interface IWebApiOAuthEndpoints
{
    /// <summary>
    /// Initiate OAuth login flow.
    /// WebAPI will redirect to Etsy, handle callback, and return authenticated response.
    /// </summary>
    [Get("/auth/login")]
    Task<HttpResponseMessage> LoginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current authenticated user profile.
    /// Returns user claims: userId, shopId, email, etc.
    /// </summary>
    [Get("/auth/profile")]
    Task<UserProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout the current user.
    /// </summary>
    [Get("/auth/logout")]
    Task<HttpResponseMessage> LogoutAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// User profile response from WebAPI /auth/profile endpoint.
/// </summary>
public class UserProfileResponse
{
    public string? UserId { get; set; }
    public string? ShopId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public List<ClaimInfo>? Claims { get; set; }
}

/// <summary>
/// Represents a claim in the user's identity.
/// </summary>
public class ClaimInfo
{
    public string? Type { get; set; }
    public string? Value { get; set; }
}
