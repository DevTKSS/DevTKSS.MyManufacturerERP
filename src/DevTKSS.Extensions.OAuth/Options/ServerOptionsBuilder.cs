namespace DevTKSS.Extensions.OAuth.Options;

public class ServerOptionsBuilder
{
    private string _protocol = "http";
    private string _rootUri = "localhost";
    private ushort _port = 0;
    private string _callbackUri = "/callback";

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

    public ServerOptions Build()
    {
        return new ServerOptions
        {
            Protocol = _protocol,
            RootUri = _rootUri,
            Port = _port,
            CallbackUri = _callbackUri
        };
    }
}