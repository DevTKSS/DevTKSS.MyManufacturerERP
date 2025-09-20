namespace DevTKSS.Extensions.OAuth.AuthCallback;

public record AuthCallbackOptions
{
	public const string DefaultName = "AuthCallback";
	public string? CallbackUri { get; init; }


}
