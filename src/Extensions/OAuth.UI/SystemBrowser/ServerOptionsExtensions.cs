namespace DevTKSS.Extensions.OAuth.UI.SystemBrowser;

internal static class ServerOptionsExtensions
{
    public static Uri ToUri4(this ServerOptions serverOptions)
    {
        // not implementing http on purpose, as the Yllibed.HttpServer Project is using TCPListener which doesn't support https.
        var builder = new UriBuilder("http", serverOptions.Hostname4, serverOptions.Port);
        return new Uri(builder.ToString());
    }
}
