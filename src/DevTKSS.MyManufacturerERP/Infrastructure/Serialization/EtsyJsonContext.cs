namespace DevTKSS.MyManufacturerERP.Infrastructure.Serialization;
[JsonSerializable(typeof(OAuthConfiguration))]
[JsonSerializable(typeof(AuthorizationCodeResponse))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(UserMeResponse))]
[JsonSerializable(typeof(UserDetailsResponse))]
[JsonSerializable(typeof(PingResponse))]
public partial class EtsyJsonContext : JsonSerializerContext
{
}
