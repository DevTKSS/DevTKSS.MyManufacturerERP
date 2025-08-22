namespace DevTKSS.Extensions.OAuth.Dictionarys;

public static class CredentialsDictionaryExtensions
{
    public static bool TryGetState(this IDictionary<string, string> credentials, out string? state)
    {
        if (credentials.TryGetValue(OAuthAuthRequestDefaults.StateKey, out var stateValue))
        {
            state = stateValue;
            return true;
        }
        state = null;
        return false;
    }
    public static bool TryGetCodeVerifier(this IDictionary<string, string> credentials, out string? codeVerifier)
    {
        if (credentials.TryGetValue(OAuthPkceDefaults.CodeVerifierKey, out var codeVerifierValue))
        {
            codeVerifier = codeVerifierValue;
            return true;
        }
        codeVerifier = null;
        return false;
    }
}
