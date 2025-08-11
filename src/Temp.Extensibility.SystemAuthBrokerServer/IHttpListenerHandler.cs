using System.Net;
// references used:
// - https://github.com/hyun4545/OAuthDesktopApp/blob/master/OAuthDesktopApp/KeycloakService.cs
// - https://github.com/carldebilly/Yllibed.HttpServer/blob/master/Yllibed.HttpServer/Server.cs
namespace Temp.Extensibility.DesktopAuthBroker;

public interface IHttpListenerHandler
{
    Task HandleRequest(HttpListenerContext context);
}
