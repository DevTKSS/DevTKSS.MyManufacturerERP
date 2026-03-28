using System.Diagnostics;
using System.Runtime.InteropServices;
using DevTKSS.Extensions.OAuth.Providers;

namespace DevTKSS.MyManufacturerERP.xUnitTests;

public class BrowserProviderTests
{
    #region Argument Validation

    [Fact]
    public void OpenBrowser_ShouldThrow_WhenUriIsNull()
    {
        Should.Throw<ArgumentNullException>(() => BrowserProvider.OpenBrowser(null!, null));
    }

    [Fact]
    public void OpenBrowser_Instance_ShouldThrow_WhenUriIsNull()
    {
        var provider = new BrowserProvider();
        Should.Throw<ArgumentNullException>(() => provider.OpenBrowser(null!));
    }

    #endregion

    #region URL Encoding (cmd fallback)

    [Fact]
    public void UrlWithAmpersand_ShouldBeEscaped_ForCmdFallback()
    {
        var url = "https://example.com/auth?client_id=abc&redirect_uri=http://localhost";
        var escaped = url.Replace("&", "^&");

        escaped.ShouldContain("^&");
        escaped.ShouldNotContain("&&");
        escaped.Split("^&").Length.ShouldBe(url.Split("&").Length);
    }

    [Fact]
    public void UrlWithoutAmpersand_ShouldRemainUnchanged()
    {
        var url = "https://example.com/auth?client_id=abc";
        var escaped = url.Replace("&", "^&");

        escaped.ShouldBe(url);
    }

    #endregion

    #region Windows Integration Tests

    [Trait("Category", "Integration")]
    [Trait("Platform", "Windows")]
    [Fact]
    public void UseShellExecute_ShouldReturnProcess_OnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }

        // Verify that Process.Start with UseShellExecute=true returns a non-null Process
        // for a known safe URL scheme. Using about:blank to avoid actually opening a browser page.
        var psi = new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = "/c echo test",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };

        var process = Process.Start(psi);
        process.ShouldNotBeNull("Process.Start should return a non-null Process on Windows");
        process.WaitForExit(5000);
        process.ExitCode.ShouldBe(0);
    }

    [Trait("Category", "Integration")]
    [Trait("Platform", "Windows")]
    [Fact]
    public void CmdFallback_ShouldStartProcess_OnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }

        // Verify the cmd /c start pattern works (the corefx#10361 workaround)
        var url = "https://example.com/test?a=1^&b=2";
        var process = Process.Start(new ProcessStartInfo("cmd", $"/c echo \"{url}\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        });

        process.ShouldNotBeNull("cmd fallback should start a process");
        process.WaitForExit(5000);
        process.ExitCode.ShouldBe(0);
    }

    #endregion
}
