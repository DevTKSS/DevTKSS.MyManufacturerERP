using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace OpenIddict.Sandbox.UnoClient;
//https://github.com/Ecierge/openiddict-core/blob/apple-cryptokit/sandbox/OpenIddict.Sandbox.Uno.Client/UnoHostApplicationLifetime.cs
public class UnoHostApplicationLifetime : ApplicationLifetime, IHostApplicationLifetime
{
    public UnoHostApplicationLifetime(ILogger<ApplicationLifetime> logger) : base(logger) { }

    CancellationToken IHostApplicationLifetime.ApplicationStarted => this.ApplicationStarted;

    CancellationToken IHostApplicationLifetime.ApplicationStopping => this.ApplicationStopping;

    CancellationToken IHostApplicationLifetime.ApplicationStopped => this.ApplicationStopped;

    void IHostApplicationLifetime.StopApplication()
    {
        this.StopApplication();
        Environment.Exit(0);
    }
}

public class ActivationHostApplicationLifetime : ApplicationLifetime
{
    public ActivationHostApplicationLifetime(ILogger<ApplicationLifetime> logger) : base(logger)
    {
        this.ApplicationStarted.Register(() => this.StopApplication());
    }
}