using System.Diagnostics.CodeAnalysis;

namespace DevTKSS.Extensions.OAuth.Dictionarys;

public static class AuthDictionaryExtensions
{
    #region Error Responses
    public static bool TryGetErrorDescription(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? errorDescription)
    {
        if (credentials.TryGetValue(OAuthErrorResponseDefaults.ErrorDescriptionKey, out var errorDescriptionValue))
        {
            errorDescription = errorDescriptionValue;
            return true;
        }
        errorDescription = null;
        return false;
    }
    public static bool TryGetErrorUri(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? errorUri)
    {
        if (credentials.TryGetValue(OAuthErrorResponseDefaults.ErrorUriKey, out var errorUriValue))
        {
            errorUri = errorUriValue;
            return true;
        }
        errorUri = null;
        return false;
    }
    public static bool TryGetErrorCode(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? errorCode)
    {
        if (credentials.TryGetValue(OAuthErrorResponseDefaults.ErrorKey, out var errorVal))
        {
            errorCode = errorVal;
            return true;
        }
        errorCode = null;
        return false;
    }

    public static bool IsErrorResponse(this IDictionary<string, string> credentials)
    {
        return credentials.ContainsKey(OAuthErrorResponseDefaults.ErrorKey);
    }
    #endregion

    #region Auth Requests
    public static bool TryGetState(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? state)
    {
        if (credentials.TryGetValue(OAuthAuthorizationCodeReqestDefaults.StateKey, out var stateValue))
        {
            state = stateValue;
            return true;
        }
        state = null;
        return false;
    }

    public static bool TryGetCode(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? code)
    {
        if (credentials.TryGetValue(OAuthAuthorizationCodeReqestDefaults.CodeKey, out var codeValue))
        {
            code = codeValue;
            return true;
        }
        code = null;
        return false;
    }
    public static bool TryGetCodeVerifier(this IDictionary<string, string> credentials,[NotNullWhen(true)] out string? codeVerifier)
    {
        if (credentials.TryGetValue(OAuthPkceDefaults.CodeVerifierKey, out var codeVerifierValue))
        {
            codeVerifier = codeVerifierValue;
            return true;
        }
        codeVerifier = null;
        return false;
    }
    #endregion
}
