using System.Collections.Immutable;

namespace DevTKSS.MyManufacturerERP.DataContracts.Serialization;
[JsonSerializable(typeof(TodoItem))]
[JsonSerializable(typeof(TodoItem[]))]
[JsonSerializable(typeof(IEnumerable<TodoItem>))]
[JsonSerializable(typeof(IImmutableList<TodoItem>))]
[JsonSerializable(typeof(ImmutableList<TodoItem>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class TodoItemContext : JsonSerializerContext
{
}
