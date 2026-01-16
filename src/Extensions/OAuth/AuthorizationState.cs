namespace DevTKSS.Extensions.OAuth;

public class AuthorizationState
{
    public string? State { get; set; }
    public string? CodeVerifier { get; set; }
    public string? StartUrl { get; set; }
    public string? RedirectUri { get; set; }
}