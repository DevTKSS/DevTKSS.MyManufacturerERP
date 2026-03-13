namespace DevTKSS.Extensions.OAuth.Requests;
public record AuthorizationState
{
    public AuthorizationState()
    {
        State = OAuth2Utilitys.GenerateState();
        CodeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        CodeChallenge = OAuth2Utilitys.GenerateCodeChallenge(CodeVerifier);
    }
    // Keep the properties internal to prevent external modification, as they are generated internally and should not be changed by the consumer of the class.

    internal string State { get; }
    internal string CodeVerifier { get; }
    internal string CodeChallenge { get; } 
    // TODO: Implement validation methods here to ensure that the retrieved response values meet the OAuth2 flow requirements.
}

