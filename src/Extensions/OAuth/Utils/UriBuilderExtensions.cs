

namespace DevTKSS.Extensions.OAuth;

internal static partial class UriBuilderExtensions
{
    private const int DefaultHttpsPort = 443;

    [GeneratedRegex(@"^(?!\s*$)(?!.*\p{Cc})(?:[^%]|%(?:[0-9A-Fa-f]{2}))*$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex IsValidQueryComponent();

    private static string? NormalizeAndEncodeQueryParameter(this string? input)
    {
        if (input is null)
        {
            return default;
        }

        input = input.Trim();
        if (input.Length == 0)
        {
            return default;
        }
        
        if (IsValidQueryComponent().IsMatch(input))
        {
            return input;
        }

        try
        {
            input = Uri.UnescapeDataString(input);
        }
        catch
        {
            return default;
        }

        return Uri.EscapeDataString(input);
    }

    /// <summary>
    /// Appends raw query string parameters to the <see cref="UriBuilder.Query"/> of <paramref name="builder"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="queryParams"/> is expected to be a query string without a leading '?'.
    /// </remarks>
    internal static UriBuilder AppendQueryParameters(this UriBuilder builder, string queryParams)
    {
        if (builder.Query.Length > 1)
        {
            builder.Query = builder.Query[1..] + "&" + queryParams;
        }
        else
        {
            builder.Query = queryParams;
        }
        return builder;
    }
    internal static UriBuilder AppendQueryParameters(this UriBuilder builder, IDictionary<string, string> queryParams)
    {
        var list = new List<string>();
        foreach (var kvp in queryParams)
        {
            if (kvp.Key.NormalizeAndEncodeQueryParameter() is not string key 
               || kvp.Value.NormalizeAndEncodeQueryParameter() is not string value)
            {    
                continue; 
            }
            list.Add($"{key}={value}");
        }
        return builder.AppendQueryParameters(string.Join("&", list));
    }

    /// <summary>
    /// Ensures the returned <see cref="UriBuilder"/> uses the HTTPS scheme and only sets an explicit port, when it is not the default HTTPS port (443).
    /// </summary>
    /// <param name="builder">The existing <see cref="UriBuilder"/> instance to update.</param>
    /// <param name="uri">
    /// The URI used to create the <see cref="UriBuilder"/> when <paramref name="builder"/> is <see langword="null"/>.
    /// </param>
    /// <param name="port">The HTTPS port to apply; ignored when it equals 443.</param>
    /// <returns>The updated (or newly created) <see cref="UriBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/> and <paramref name="uri"/> is missing.</exception>
    /// <exception cref="UriFormatException">Thrown when <paramref name="builder"/> is <see langword="null"/> and <paramref name="uri"/> is not a valid URI.</exception>
    internal static UriBuilder SetHttpsUriWithOptionalPort(this UriBuilder? builder, string? uri = default, int port = DefaultHttpsPort)
    {
        if (builder is null)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri), "Either provide a valid Uri or an existing UriBuilder instance.");
            }
            builder = new UriBuilder(uri);
        }

        if (!builder.Uri.AbsoluteUri.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            builder.Scheme = Uri.UriSchemeHttps;
        }
        // No need to set port if it equals 443 as it is the default https port
        if (port != DefaultHttpsPort)
        {
            builder.Port = port;
        }
        return builder;
    }


}
