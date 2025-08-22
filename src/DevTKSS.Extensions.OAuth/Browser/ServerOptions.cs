namespace DevTKSS.Extensions.OAuth.Browser;
public record ServerOptions
{
    public int Port { get; init; } = 0;
    public string? RootUri { get; init; }
    public string? RelativeCallbackUri { get; init; }
}
