namespace DevTKSS.Extensions.OAuth.Options;

public record ServerOptions
{
    public string Protocol { get; init; } = "http";
    public string RootUri { get; init; } = "localhost";
    public ushort Port { get; init; } = 0; // 0 means auto-assign for loopback flows
    public string CallbackUri { get; init; } = "/callback";

    public override string ToString()
    {
        return $"{Protocol}://{RootUri}{(Port > 0 ? $":{Port}" : "")}{CallbackUri}";
    }
}
