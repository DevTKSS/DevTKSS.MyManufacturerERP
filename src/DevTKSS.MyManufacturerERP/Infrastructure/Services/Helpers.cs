namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

internal interface IHelpers
{
    void OpenBrowser(Uri uri);
}

internal class Helpers : IHelpers
{
    /// <summary>
    /// Helper method to open the browser through the url.dll.
    /// </summary>
    /// <param name="uri">The Uri to open</param>
    public void OpenBrowser(Uri uri)
    {
        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = uri.ToString(),
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }

}
