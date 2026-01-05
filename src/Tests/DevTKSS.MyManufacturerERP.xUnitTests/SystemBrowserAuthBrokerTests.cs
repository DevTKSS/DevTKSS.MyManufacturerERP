using DevTKSS.Extensions.Uno.Authentication.Desktop;
using System.Net;
using Uno.AuthenticationBroker;
using Windows.Security.Authentication.Web;

namespace DevTKSS.MyManufacturerERP.xUnitTests;

public class SystemBrowserAuthBrokerTests
{
    [Fact]
    public void Constructor_WhenCreated_ShouldNotBeNull()
    {
        // Arrange & Act
        var broker = new SystemBrowserAuthBroker();

        // Assert
        broker.ShouldNotBeNull();
    }

    [Fact]
    public void ServerOptions_WhenNotSet_ShouldReturnDefaultOptions()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var options = broker.ServerOptions;

        // Assert
        options.ShouldNotBeNull();
        options.Hostname4.ShouldBe("localhost");
        ((int)options.Port).ShouldBe(5001);
        options.BindAddress4.ShouldBe(IPAddress.Loopback);
    }

    [Fact]
    public void ServerOptions_WhenAccessedMultipleTimes_ShouldReturnSameInstance()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var options1 = broker.ServerOptions;
        var options2 = broker.ServerOptions;

        // Assert
        options1.ShouldBeSameAs(options2);
    }

    [Fact]
    public void GetCurrentApplicationCallbackUri_WhenCalled_ShouldReturnValidCallbackUri()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.ShouldNotBeNull();
        callbackUri.Scheme.ShouldBe("http");
        callbackUri.Host.ShouldBe("localhost");
        callbackUri.Port.ShouldBe(5001);
        callbackUri.PathAndQuery.ShouldBe("/callback");
    }

    [Fact]
    public void GetCurrentApplicationCallbackUri_WhenCalledMultipleTimes_ShouldReturnConsistentUri()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri1 = broker.GetCurrentApplicationCallbackUri();
        var callbackUri2 = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri1.ToString().ShouldBe(callbackUri2.ToString());
    }

    [Fact]
    public void GetCurrentApplicationCallbackUri_WhenCalled_ShouldStartHttpServer()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.ShouldNotBeNull();
        callbackUri.IsAbsoluteUri.ShouldBeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenOptionsHaveSilentMode_ShouldThrowNotSupportedException()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var options = WebAuthenticationOptions.SilentMode;
        var requestUri = new Uri("https://example.com/auth");
        var callbackUri = new Uri("http://localhost:5001/callback");

        // Act & Assert
        var exception = await Should.ThrowAsync<NotSupportedException>(async () =>
            await broker.AuthenticateAsync(options, requestUri, callbackUri, CancellationToken.None));

        exception.Message.ShouldBe("SilentMode is not supported by this broker.");
    }

    [Fact]
    public async Task AuthenticateAsync_WhenOptionsHaveMultipleUnsupportedFlags_ShouldThrowNotSupportedExceptionForFirst()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var options = WebAuthenticationOptions.SilentMode | WebAuthenticationOptions.None;
        var requestUri = new Uri("https://example.com/auth");
        var callbackUri = new Uri("http://localhost:5001/callback");

        // Act & Assert
        var exception = await Should.ThrowAsync<NotSupportedException>(async () =>
            await broker.AuthenticateAsync(options, requestUri, callbackUri, CancellationToken.None));

        exception.Message.ShouldBe("SilentMode is not supported by this broker.");
    }

    [Fact]
    public void ServerOptions_WhenCustomized_ShouldReturnCustomValues()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var customOptions = new Yllibed.HttpServer.ServerOptions
        {
            Hostname4 = "127.0.0.1",
            Port = 8080,
            BindAddress4 = IPAddress.Any
        };

        // Act
        var property = typeof(SystemBrowserAuthBroker).GetProperty("ServerOptions");
        property?.SetValue(broker, customOptions);

        // Assert
        broker.ServerOptions.Hostname4.ShouldBe("127.0.0.1");
        ((int)broker.ServerOptions.Port).ShouldBe(8080);
        broker.ServerOptions.BindAddress4.ShouldBe(IPAddress.Any);
    }

    [Theory]
    [InlineData("localhost", 3000)]
    [InlineData("127.0.0.1", 8080)]
    [InlineData("0.0.0.0", 9090)]
    public void GetCurrentApplicationCallbackUri_WithDifferentServerOptions_ShouldReturnCorrectUri(string hostname, int port)
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var customOptions = new Yllibed.HttpServer.ServerOptions
        {
            Hostname4 = hostname,
            Port = (ushort)port,
            BindAddress4 = IPAddress.Loopback
        };

        var property = typeof(SystemBrowserAuthBroker).GetProperty("ServerOptions");
        property?.SetValue(broker, customOptions);

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.Host.ShouldBe(hostname);
        callbackUri.Port.ShouldBe(port);
    }

    [Fact]
    public void ServerOptions_WhenSetToNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
        {
            var property = typeof(SystemBrowserAuthBroker).GetProperty("ServerOptions");
            property?.SetValue(broker, null);
        });

        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenOptionsAreNone_ShouldNotThrowForValidationCheck()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var options = WebAuthenticationOptions.None;
        var requestUri = new Uri("https://example.com/auth");
        var callbackUri = new Uri("http://localhost:5001/callback");

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await broker.AuthenticateAsync(options, requestUri, callbackUri, CancellationToken.None));
    }

    [Fact]
    public void ServerOptions_DefaultConfiguration_ShouldUseLoopbackAddress()
    {
        // Arrange & Act
        var broker = new SystemBrowserAuthBroker();

        // Assert
        broker.ServerOptions.BindAddress4.ShouldBe(IPAddress.Loopback);
        broker.ServerOptions.BindAddress4.ToString().ShouldBe("127.0.0.1");
    }

    [Fact]
    public void ServerOptions_DefaultConfiguration_ShouldUseHttpScheme()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.Scheme.ShouldBe("http");
        callbackUri.Scheme.ShouldNotBe("https");
    }

    [Fact]
    public void GetCurrentApplicationCallbackUri_WhenCalled_ShouldHaveCallbackPath()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.AbsolutePath.ShouldBe("/callback");
        callbackUri.PathAndQuery.ShouldContain("callback");
    }

    [Fact]
    public void ServerOptions_DefaultPort_ShouldBe5001()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var port = (int)broker.ServerOptions.Port;

        // Assert
        port.ShouldBe(5001);
        port.ShouldBeGreaterThan(1024);
        port.ShouldBeLessThan(65536);
    }

    [Fact]
    public void GetCurrentApplicationCallbackUri_WhenCalled_ShouldReturnWellFormedUri()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.IsWellFormedOriginalString().ShouldBeTrue();
        callbackUri.ToString().ShouldStartWith("http://");
        callbackUri.ToString().ShouldEndWith("/callback");
    }

    [Fact]
    public async Task AuthenticateAsync_WhenRequestUriIsNull_ShouldThrowException()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var options = WebAuthenticationOptions.None;
        Uri requestUri = null!;
        var callbackUri = new Uri("http://localhost:5001/callback");

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await broker.AuthenticateAsync(options, requestUri, callbackUri, CancellationToken.None));
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(3000)]
    [InlineData(8080)]
    [InlineData(9000)]
    [InlineData(65535)]
    public void ServerOptions_WithVariousPorts_ShouldAcceptValidPortNumbers(int port)
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var customOptions = new Yllibed.HttpServer.ServerOptions
        {
            Hostname4 = "localhost",
            Port = (ushort)port,
            BindAddress4 = IPAddress.Loopback
        };

        // Act
        var property = typeof(SystemBrowserAuthBroker).GetProperty("ServerOptions");
        property?.SetValue(broker, customOptions);

        // Assert
        ((int)broker.ServerOptions.Port).ShouldBe(port);
    }

    [Theory]
    [InlineData("http://localhost:5001/callback")]
    [InlineData("http://127.0.0.1:5001/callback")]
    [InlineData("http://localhost:3000/auth/callback")]
    public void GetCurrentApplicationCallbackUri_WithDifferentPaths_ShouldReturnCorrectFormat(string expectedPattern)
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var callbackUri = broker.GetCurrentApplicationCallbackUri();

        // Assert
        callbackUri.ShouldNotBeNull();
        callbackUri.Scheme.ShouldBe("http");
    }

    [Fact]
    public void ServerOptions_WhenAccessedBeforeAuthentication_ShouldNotStartServer()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var options = broker.ServerOptions;

        // Assert
        options.ShouldNotBeNull();
        options.Hostname4.ShouldBe("localhost");
    }

    [Fact]
    public void GetCurrentApplicationCallbackUri_CalledTwice_ShouldReturnIdenticalUris()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var uri1 = broker.GetCurrentApplicationCallbackUri();
        var uri2 = broker.GetCurrentApplicationCallbackUri();

        // Assert
        uri1.ShouldBe(uri2);
        uri1.ToString().ShouldBe(uri2.ToString());
    }

    [Fact]
    public async Task AuthenticateAsync_WithCancelledToken_ShouldRespectCancellation()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var options = WebAuthenticationOptions.None;
        var requestUri = new Uri("https://example.com/auth");
        var callbackUri = new Uri("http://localhost:5001/callback");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await broker.AuthenticateAsync(options, requestUri, callbackUri, cts.Token));
    }

    [Fact]
    public void ServerOptions_BindAddress_ShouldBeLoopbackByDefault()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var bindAddress = broker.ServerOptions.BindAddress4;

        // Assert
        bindAddress.ShouldBe(IPAddress.Loopback);
        bindAddress.ToString().ShouldNotBe(IPAddress.Any.ToString());
    }

    [Fact]
    public void ServerOptions_Hostname_ShouldBeLocalhostByDefault()
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();

        // Act
        var hostname = broker.ServerOptions.Hostname4;

        // Assert
        hostname.ShouldBe("localhost");
        hostname.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("https://auth.example.com/authorize?client_id=test")]
    [InlineData("https://oauth.provider.com/login")]
    [InlineData("http://localhost:5000/auth")]
    public async Task AuthenticateAsync_WithVariousRequestUris_ShouldAcceptValidUris(string uriString)
    {
        // Arrange
        var broker = new SystemBrowserAuthBroker();
        var options = WebAuthenticationOptions.None;
        var requestUri = new Uri(uriString);
        var callbackUri = new Uri("http://localhost:5001/callback");

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await broker.AuthenticateAsync(options, requestUri, callbackUri, CancellationToken.None));
    }
}
