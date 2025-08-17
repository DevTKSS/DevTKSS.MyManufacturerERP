namespace OpenIddict.Sandbox.UnoClient;
// https://github.com/Ecierge/openiddict-core/blob/apple-cryptokit/sandbox/OpenIddict.Sandbox.Uno.Client/Worker.cs
public class Worker : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly IHostApplicationLifetime _lifetime;

    public Worker(IServiceProvider provider, IHostApplicationLifetime lifetime)

    {
        _provider = provider;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
