using System.Diagnostics.CodeAnalysis;

namespace DevTKSS.Extensions.OAuth;

internal static class Base64UrlHelpers
{
    public static string Encode(byte[] arg)
    {
        string s = Convert.ToBase64String(arg); // Standard base64 encoder
        s = s.Split('=')[0]; // Remove any trailing '='s
        s = s.Replace('+', '-'); // 62nd char of encoding
        s = s.Replace('/', '_'); // 63rd char of encoding
        return s;
    }
    public static string? Encode([NotNullIfNotNull(nameof(arg))]string arg)
    {
        if (arg == null)
            return null;
        return Encode(System.Text.Encoding.UTF8.GetBytes(arg));
    }
}