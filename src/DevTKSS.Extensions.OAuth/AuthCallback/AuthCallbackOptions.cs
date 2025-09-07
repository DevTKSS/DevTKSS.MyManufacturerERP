namespace DevTKSS.Extensions.OAuth.AuthCallback;

public interface IAuthCallbackOptions
{
    Uri? CallbackUri { get; init; }
}

public record AuthCallbackOptions : IAuthCallbackOptions
{
    public const string DefaultName = "AuthCallback";
    public Uri? CallbackUri { get; init; }


}
