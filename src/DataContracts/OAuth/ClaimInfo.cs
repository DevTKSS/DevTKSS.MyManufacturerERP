namespace DevTKSS.MyManufacturerERP.DataContracts.OAuth;

/// <summary>
/// Represents a claim in the user's identity.
/// </summary>
[AdaptTo(nameof(ClaimInfo), MapToConstructor = true)]
public class ClaimInfo
{
    /// <summary>
    /// Gets or sets the claim type (e.g., "user_id", "email", "name").
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the claim value.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
