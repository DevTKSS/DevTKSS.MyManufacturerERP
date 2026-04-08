namespace DevTKSS.Extensions.OAuth.UI.SystemBrowser;

internal static class ServerOptionsExtensions
{
    /// <summary>
    /// Creates a URI using the specified server options and callback path for IPv4 connections.
    /// </summary>
    /// <remarks>This method always uses the HTTP scheme because the underlying server implementation does not
    /// support HTTPS.</remarks>
    /// <param name="serverOptions">The server options containing the hostname and port to use for the URI.</param>
    /// <param name="callbackPath">The callback path to append to the URI. This should be a relative path.</param>
    /// <returns>A new URI constructed with the specified hostname, port, and callback path using the HTTP scheme.</returns>
    public static Uri ToUri4(this ServerOptions serverOptions, string callbackPath)
    {
        // not implementing http, as the Yllibed.HttpServer Project is using TCPListener which doesn't support https.
        var builder = new UriBuilder("http", serverOptions.Hostname4, serverOptions.Port, callbackPath);
        return new Uri(builder.ToString());
    }
}
