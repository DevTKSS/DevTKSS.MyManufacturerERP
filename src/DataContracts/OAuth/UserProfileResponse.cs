namespace DevTKSS.MyManufacturerERP.DataContracts.OAuth;

/// <summary>
/// User profile response from WebAPI /auth/profile endpoint.
/// Maps claims from OAuth provider (Etsy) with IncludeDetailedUserInfo enabled.
/// </summary>
[AdaptTo(nameof(UserProfileResponse), MapToConstructor = true)]
public class UserProfileResponse
{
    /// <summary>
    /// Gets or sets the unique user identifier from the OAuth provider.
    /// Claim type: "user_id"
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the shop identifier associated with the user.
    /// Claim type: "shop_id"
    /// </summary>
    [JsonPropertyName("shop_id")]
    public string? ShopId { get; set; }

    /// <summary>
    /// Gets or sets the user's primary email address.
    /// Claim type: "primary_email" or ClaimTypes.Email
    /// </summary>
    [JsonPropertyName("primary_email")]
    public string? PrimaryEmail { get; set; }

    /// <summary>
    /// Gets or sets the user's given (first) name.
    /// Claim type: "given_name" or ClaimTypes.GivenName
    /// </summary>
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    /// <summary>
    /// Gets or sets the user's surname (last name).
    /// Claim type: "family_name" or ClaimTypes.Surname
    /// </summary>
    [JsonPropertyName("family_name")]
    public string? Surname { get; set; }

    /// <summary>
    /// Gets or sets the user's display name (full name).
    /// Claim type: ClaimTypes.Name
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the user's profile image URL (75x75 pixels).
    /// Claim type: "picture" or "image_75x75_url"
    /// Optional claim, only included when available from provider.
    /// </summary>
    [JsonPropertyName("picture")]
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the unique principal identifier claim from the OAuth provider.
    /// This is typically the NameIdentifier claim that uniquely identifies the user.
    /// Claim type: ClaimTypes.NameIdentifier
    /// </summary>
    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the list of all claims associated with the user's identity.
    /// Useful for advanced scenarios where all claim data is needed.
    /// </summary>
    public List<ClaimInfo>? Claims { get; set; }
}
