namespace DevTKSS.MyManufacturerERP.DataContracts;

[AdaptTo("[name]Dto"), GenerateMapper]
public class TodoItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("is_complete")]
    public bool IsComplete { get; set; } = false;
}
