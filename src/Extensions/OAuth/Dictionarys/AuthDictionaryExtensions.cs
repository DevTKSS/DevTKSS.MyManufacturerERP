using System.Diagnostics.CodeAnalysis;

namespace DevTKSS.Extensions.OAuth.Dictionarys;

public static class AuthDictionaryExtensions
{
    #region Error Responses
    public static bool TryGetErrorDescription(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? errorDescription)
    {
        if (credentials.TryGetValue(OAuthDefaults.Keys.Error.Description, out var errorDescriptionValue))
        {
            errorDescription = errorDescriptionValue;
            return true;
        }
        errorDescription = null;
        return false;
    }
    public static bool TryGetErrorUri(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? errorUri)
    {
        if (credentials.TryGetValue(OAuthDefaults.Keys.Error.Uri, out var errorUriValue))
        {
            errorUri = errorUriValue;
            return true;
        }
        errorUri = null;
        return false;
    }
    public static bool TryGetErrorCode(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? errorCode)
    {
        if (credentials.TryGetValue(OAuthDefaults.Keys.Error.Key, out var errorVal))
        {
            errorCode = errorVal;
            return true;
        }
        errorCode = null;
        return false;
    }

    public static bool IsErrorResponse(this IDictionary<string, string> credentials)
    {
        return credentials.ContainsKey(OAuthDefaults.Keys.Error.Key);
    }
    #endregion

    #region Auth Requests
    [Obsolete("Use TryGetCode instead. Both look up the 'code' query parameter (RFC 6749 §4.1.2).")]
    public static bool TryGetAuthorizationCode(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? authorizationCode)
    {
        return credentials.TryGetCode(out authorizationCode);
    }
    public static bool TryGetState(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? state)
    {
        if (credentials.TryGetValue(OAuthDefaults.Keys.State, out var stateValue))
        {
            state = stateValue;
            return true;
        }
        state = null;
        return false;
    }

    public static bool TryGetCode(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? code)
    {
        if (credentials.TryGetValue(OAuthDefaults.Keys.Code, out var codeValue))
        {
            code = codeValue;
            return true;
        }
        code = null;
        return false;
    }
    public static bool TryGetCodeVerifier(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? codeVerifier)
    {
        if (credentials.TryGetValue(OAuthDefaults.Keys.Pkce.CodeVerifier, out var codeVerifierValue))
        {
            codeVerifier = codeVerifierValue;
            return true;
        }
        codeVerifier = null;
        return false;
    }
    #endregion
}
