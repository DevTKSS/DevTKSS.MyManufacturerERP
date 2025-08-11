// references used:
// - https://github.com/hyun4545/OAuthDesktopApp/blob/master/OAuthDesktopApp/KeycloakService.cs
// - https://github.com/carldebilly/Yllibed.HttpServer/blob/master/Yllibed.HttpServer/Server.cs
namespace Temp.Extensibility.DesktopAuthBroker;

/// <summary>
/// Defines the contract for an HTTP listener server that can handle incoming HTTP requests and register custom request handlers.
/// </summary>
public interface IHttpListenerServer
{
    /// <summary>
    /// Starts the HTTP listener server using <see cref="ServerOptions.RelativeCallbackUri"/> and <see cref="ServerOptions.RootUri"/> from the options provided via dependency injection.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the options are <see langword="null"/> or required properties are not set.</exception>
    void Start();

    /// <summary>
    /// Starts the HTTP listener server with the specified root URI and relative callback URI, ignoring the options provided via dependency injection.
    /// </summary>
    /// <param name="rootUri">The absolute root URI to listen on (e.g., "http://localhost:5002").</param>
    /// <param name="relativeCallbackUri">The relative callback URI (e.g., "callback").</param>
    void Start(string rootUri, string relativeCallbackUri);

    /// <summary>
    /// Stops the HTTP listener server and releases all resources.
    /// </summary>
    void Stop();

    /// <summary>
    /// Registers a custom request handler that will be invoked for each incoming HTTP request.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unregister the handler.</returns>
    IDisposable RegisterHandler(IHttpListenerHandler handler);
}
