using System.Collections.Specialized;

namespace DevTKSS.Extensions.OAuth;

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
    public static NameValueCollection GetQuery(this Uri? redirectUri, Uri callbackUri)
    {
        if (redirectUri is null)
            return [];
        return redirectUri.IsBaseOf(callbackUri)
             ? AuthHttpUtility.ExtractArguments(redirectUri.ToString())  // it's a fully qualified url, so need to extract query or fragment
             : AuthHttpUtility.ParseQueryString(redirectUri.ToString().TrimStart('#').TrimStart('?')); // it isn't a full url, so just process as query or fragment

    }
}
