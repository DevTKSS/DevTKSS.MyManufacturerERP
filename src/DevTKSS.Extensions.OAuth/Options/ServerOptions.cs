namespace DevTKSS.Extensions.OAuth.Options;

public enum UriFormatMode
{
    Standard,       // "://"
    SingleSlash,    // ":/"
    Custom          // completely custom URI
}

public record ServerOptions
{
    public bool IsLoopback { get; init; } = true;
    public string Protocol { get; init; } = "http";
    public string RootUri { get; init; } = "localhost";
    public ushort Port { get; init; } = 0;
    public string CallbackUri { get; init; } = "/callback";
    public UriFormatMode UriFormat { get; init; } = UriFormatMode.Standard;
    public string? CustomUri { get; init; } = null;

    public override string ToString()
    {
        return UriFormat switch
        {
            UriFormatMode.Standard =>
                $"{Protocol}://{RootUri}{(Port > 0 ? $":{Port}" : "")}{CallbackUri}",
            UriFormatMode.SingleSlash =>
                $"{Protocol}:/{RootUri}{(Port > 0 ? $":{Port}" : "")}{CallbackUri}",
            UriFormatMode.Custom when !string.IsNullOrEmpty(CustomUri) =>
                CustomUri!,
            _ =>
                $"{Protocol}://{RootUri}{(Port > 0 ? $":{Port}" : "")}{CallbackUri}"
        };
    }
}
