using System.Text.Json.Serialization;
using DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Serialization;

[JsonSerializable(typeof(HttpServerOptions))]
public partial class HttpConfigContext : JsonSerializerContext
{ }
