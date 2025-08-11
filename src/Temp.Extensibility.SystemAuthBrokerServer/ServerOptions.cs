namespace Temp.Extensibility.DesktopAuthBroker;
public record ServerOptions
{
    public string? RootUri { get; init; }
    public string? RelativeCallbackUri { get; init; }
}
