using System.Text.Json.Serialization;
using DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Serialization;
[JsonSerializable(typeof(OAuthConfiguration))]
[JsonSerializable(typeof(TokenRequestPayload))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(UserMe))]
[JsonSerializable(typeof(UserDetails))]
public partial class EtsyJsonContext : JsonSerializerContext
{
}
