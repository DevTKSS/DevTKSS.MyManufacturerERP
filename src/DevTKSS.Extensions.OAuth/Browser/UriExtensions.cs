namespace DevTKSS.Extensions.OAuth.Browser;

public static class UriExtensions
{
    /// <summary>
    /// Returns a string representation of the URI without query string or fragment,
    /// suitable for safe logging.
    /// </summary>
    public static string ToSafeDisplay(this Uri uri)
    {
        if (uri is null) return string.Empty;
        var b = new UriBuilder(uri) { Query = string.Empty, Fragment = string.Empty };
        return b.Uri.ToString();
    }
}
