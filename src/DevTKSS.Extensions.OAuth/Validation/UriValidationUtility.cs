namespace DevTKSS.Extensions.OAuth.Validation;
public class UriValidationUtility
{
    public static bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var _);
    }
    public static bool BeAValidRelativeUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Relative, out var _);
    }
}
