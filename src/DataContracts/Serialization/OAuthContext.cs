using System.Collections.Immutable;
using System.Text.Json.Serialization;
using DevTKSS.MyManufacturerERP.DataContracts.OAuth;

namespace DevTKSS.MyManufacturerERP.DataContracts.Serialization;

/// <summary>
/// Generated class for System.Text.Json Serialization of OAuth-related types.
/// Attributes are used by the JsonSerializerContext source generator.
/// </summary>
[JsonSerializable(typeof(UserProfileResponse))]
[JsonSerializable(typeof(UserProfileResponse[]))]
[JsonSerializable(typeof(IEnumerable<UserProfileResponse>))]
[JsonSerializable(typeof(ClaimInfo))]
[JsonSerializable(typeof(ClaimInfo[]))]
[JsonSerializable(typeof(List<ClaimInfo>))]
[JsonSerializable(typeof(IEnumerable<ClaimInfo>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public partial class OAuthContext : JsonSerializerContext
{
}
