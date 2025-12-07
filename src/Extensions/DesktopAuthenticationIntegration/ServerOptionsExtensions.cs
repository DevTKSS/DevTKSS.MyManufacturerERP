namespace DevTKSS.Extensions.Uno.DesktopAuthenticationIntegration;

internal static class ServerOptionsExtensions
{
    public static Uri ToUri4(this ServerOptions serverOptions)
    {
        var builder = new UriBuilder("http", serverOptions.Hostname4, serverOptions.Port);
        return new Uri(builder.ToString());
    }
}
