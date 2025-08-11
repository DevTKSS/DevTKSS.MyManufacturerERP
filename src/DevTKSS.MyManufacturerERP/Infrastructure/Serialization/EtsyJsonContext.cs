namespace DevTKSS.MyManufacturerERP.Infrastructure.Serialization;

[JsonSerializable(typeof(OAuthOptions))]
[JsonSerializable(typeof(UserMeResponse))]
[JsonSerializable(typeof(UserDetailsResponse))]
[JsonSerializable(typeof(PingResponse))]
public partial class EtsyJsonContext : JsonSerializerContext
{
}


[JsonSerializable(typeof(AccessGrantResponse))]
[JsonSerializable(typeof(TokenResponse))]
public partial class OAuthFlowContext : JsonSerializerContext
{
}
