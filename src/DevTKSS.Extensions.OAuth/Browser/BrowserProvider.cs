using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DevTKSS.Extensions.OAuth.Browser;

public interface IBrowserProvider
{
	void OpenBrowser(Uri uri);
}

public class BrowserProvider : IBrowserProvider
{
	private readonly ILogger _logger;
	public BrowserProvider(ILogger logger)
	{
		_logger = logger;
	}
	/// <summary>
	/// Helper method to open the browser through the url.dll.
	/// </summary>
	/// <param name="uri">The Uri to open</param>
	public void OpenBrowser(Uri uri)
	{
		var url = uri.AbsoluteUri;
		try
		{
			Process.Start(url);
		}
		catch(Exception ex)
		{
			if (_logger.IsEnabled(LogLevel.Error))
			{
				_logger.LogError(ex, "Failed to open URL in default browser using Process.Start. Falling back to platform specific handling.");
			}

			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				try
				{
					ProcessStartInfo psi = new ProcessStartInfo
					{
						FileName = uri.ToString(),
						UseShellExecute = true
					};
					Process.Start(psi);
				}
				catch(Exception ex1)
				{
					if (_logger.IsEnabled(LogLevel.Error))
					{
						_logger.LogError(ex1, "Failed to open URL in default browser using ProcessStartInfo. Falling back to cmd.");
					}
					url = url.Replace("&", "^&");
					var psi = new ProcessStartInfo("cmd", $"/c start {url}") 
					{ 
						CreateNoWindow = true,
						UseShellExecute = false
					};
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
			   
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
				throw;
			}
		}
		

	}

}
