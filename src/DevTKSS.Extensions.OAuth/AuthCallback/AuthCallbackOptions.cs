namespace DevTKSS.Extensions.OAuth.AuthCallback;

public record AuthCallbackOptions
{
    //public string Protocol { get; init; } = "http";
    //public string RootUri { get; init; } = "localhost";
    //public ushort Port { get; init; } = 0; // 0 means auto-assign for loopback flows
    
    public Uri? CallbackUri { get; init; }

    //public override string ToString()
    //{
    //    return $"{Protocol}://{RootUri}{(Port > 0 ? $":{Port}" : "")}{CallbackUri}";
    //}
}
