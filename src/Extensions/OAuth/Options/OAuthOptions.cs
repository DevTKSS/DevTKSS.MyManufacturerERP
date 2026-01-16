namespace DevTKSS.Extensions.OAuth.Options;

public record OAuthOptions // TODO: Check if this is needed or we should better inherit and make the record a class
{
    public string? LoginStartUri { get; init; }
    public string? LoginCallbackUri { get; init; }
    public OAuthClientOptions? Options { get; init; }
}
