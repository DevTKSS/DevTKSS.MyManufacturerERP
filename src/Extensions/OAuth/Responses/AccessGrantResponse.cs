namespace DevTKSS.Extensions.OAuth.Responses;

public record AccessGrantResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }
    [JsonPropertyName("state")]
    public string? State { get; init; }
}
