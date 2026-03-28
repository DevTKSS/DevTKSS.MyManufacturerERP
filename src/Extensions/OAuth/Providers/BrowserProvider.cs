namespace DevTKSS.Extensions.OAuth.Providers;

public interface IBrowserProvider
{
	void OpenBrowser(Uri uri);
}

public class BrowserProvider : IBrowserProvider
{

    public void OpenBrowser(Uri uri)
    {
        OpenBrowser(uri, null);
    }

    /// <summary>
    /// Helper method to open the System browser with the provided Uri
    /// </summary>
    /// <param name="uri">The Uri to open</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="uri"/> is <see langword="null"/></exception>"
    public static void OpenBrowser(Uri uri, ILogger? logger = null)
	{
        ArgumentNullException.ThrowIfNull(uri);

        var url = uri.AbsoluteUri;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to open URL via UseShellExecute, trying cmd fallback");
            }

            // Workaround for https://github.com/dotnet/corefx/issues/10361
            try
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{url}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                return;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "cmd fallback also failed to open browser");
                throw;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Process.Start("open", url);
        }
        else
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to open browser on this platform");
                throw;
            }
        }
    }

}
