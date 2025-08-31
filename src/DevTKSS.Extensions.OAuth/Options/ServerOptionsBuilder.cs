namespace DevTKSS.Extensions.OAuth.Options;

public class ServerOptionsBuilder
{
    private bool _isLoopback = true;
    private string _protocol = "http";
    private string _rootUri = "localhost";
    private ushort _port = 0;
    private string _callbackUri = "/callback";
    private UriFormatMode _uriFormat = UriFormatMode.Standard;
    private string? _customUri = null;

    public ServerOptionsBuilder SetIsLoopback(bool isLoopback)
    {
        _isLoopback = isLoopback;
        return this;
    }

    public ServerOptionsBuilder SetProtocol(string protocol)
    {
        _protocol = protocol;
        return this;
    }

    public ServerOptionsBuilder SetRootUri(string rootUri)
    {
        _rootUri = rootUri;
        return this;
    }

    public ServerOptionsBuilder SetPort(ushort port)
    {
        _port = port;
        return this;
    }

    public ServerOptionsBuilder SetCallbackUri(string callbackUri)
    {
        _callbackUri = callbackUri;
        return this;
    }

    public ServerOptionsBuilder SetUriFormat(UriFormatMode uriFormat)
    {
        _uriFormat = uriFormat;
        return this;
    }

    public ServerOptionsBuilder SetCustomUri(string? customUri)
    {
        _customUri = customUri;
        return this;
    }

    public ServerOptions Build()
    {
        return new ServerOptions
        {
            IsLoopback = _isLoopback,
            Protocol = _protocol,
            RootUri = _rootUri,
            Port = _port,
            CallbackUri = _callbackUri,
            UriFormat = _uriFormat,
            CustomUri = _customUri
        };
    }
}