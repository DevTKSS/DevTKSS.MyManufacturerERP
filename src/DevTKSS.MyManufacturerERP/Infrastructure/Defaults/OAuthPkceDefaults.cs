namespace DevTKSS.MyManufacturerERP.Infrastructure.Defaults;

public class OAuthPkceDefaults
{
    public const string CodeChallengeKey = "code_challenge";
    public const string CodeChallengeMethodKey = "code_challenge_method";
    public const string CodeChallengeMethodS256 = "S256";
    /// <summary>
    /// The PKCE code verifier preimage of the code_challenge used in the prior authorization code request.
    /// </summary>
    public const string CodeVerifierKey = "code_verifier";
}