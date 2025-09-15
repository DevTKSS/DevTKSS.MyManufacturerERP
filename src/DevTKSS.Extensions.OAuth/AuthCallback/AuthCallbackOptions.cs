namespace DevTKSS.Extensions.OAuth.AuthCallback;

public interface IAuthCallbackOptions
{
	string? CallbackUri { get; init; }
}

public record AuthCallbackOptions : IAuthCallbackOptions
{
	public const string DefaultName = "AuthCallback";
	public string? CallbackUri { get; init; }


}
