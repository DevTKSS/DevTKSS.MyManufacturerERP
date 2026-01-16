using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

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
    /// Returns a https URL including an explicit port only when it is not the default https port (443).
    /// </summary>
    internal static string GetHttpsUriWithOptionalPort(string uri, int port)
    {
        //No need to set port if it equals 443 as it is the default https port
        if (port != DefaultHttpsPort)
        {
            var builder = new UriBuilder(uri)
            {
                Port = port
            };
            return builder.Uri.AbsoluteUri;
        }

        return uri;
    }


}
