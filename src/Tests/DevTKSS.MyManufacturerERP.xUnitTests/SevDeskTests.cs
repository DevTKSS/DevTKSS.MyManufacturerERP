using DevTKSS.MyManufacturerERP.Infrastructure.SevDesk;

namespace DevTKSS.MyManufacturerERP.xUnitTests;

public class SevDeskTests
{
    #region SevDeskApiKeyHandler

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenApiKeyMissing()
    {
        var options = Options.Create(new SevDeskClientOptions { ApiKey = null });
        var logger = new NullLogger();
        var handler = new SevDeskApiKeyHandler(options, logger)
        {
            InnerHandler = new NoOpHandler()
        };

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://my.sevdesk.de/api/v1/Contact");

        await Should.ThrowAsync<InvalidOperationException>(() => client.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenApiKeyWhitespace()
    {
        var options = Options.Create(new SevDeskClientOptions { ApiKey = "   " });
        var logger = new NullLogger();
        var handler = new SevDeskApiKeyHandler(options, logger)
        {
            InnerHandler = new NoOpHandler()
        };

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://my.sevdesk.de/api/v1/Contact");

        await Should.ThrowAsync<InvalidOperationException>(() => client.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ShouldSetAuthorizationHeader_WhenApiKeyConfigured()
    {
        var apiKey = "test-api-key-12345";
        var options = Options.Create(new SevDeskClientOptions { ApiKey = apiKey });
        var logger = new NullLogger();
        var captureHandler = new CaptureRequestHandler();
        var handler = new SevDeskApiKeyHandler(options, logger)
        {
            InnerHandler = captureHandler
        };

        using var client = new HttpClient(handler);
        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://my.sevdesk.de/api/v1/Contact"));

        captureHandler.LastRequest.ShouldNotBeNull();
        captureHandler.LastRequest!.Headers.Authorization.ShouldNotBeNull();
        captureHandler.LastRequest.Headers.Authorization!.Scheme.ShouldBe(apiKey);
    }

    #endregion

    #region SevDeskClientOptions

    [Fact]
    public void SevDeskClientOptions_ShouldInheritFromEndpointOptions()
    {
        var options = new SevDeskClientOptions();
        var baseType = options.GetType().BaseType;
        baseType.ShouldNotBeNull();
        baseType!.Name.ShouldBe("EndpointOptions");
    }

    [Fact]
    public void SevDeskClientOptions_ShouldHaveApiKeyProperty()
    {
        var options = new SevDeskClientOptions { ApiKey = "test-key" };
        options.ApiKey.ShouldBe("test-key");
    }

    #endregion

    #region SevDesk Records

    [Fact]
    public void SevDeskContact_ShouldBePartialRecord()
    {
        var contact = new SevDeskContact { Id = 1, Name = "Test" };
        contact.Id.ShouldBe(1);
        contact.Name.ShouldBe("Test");
    }

    [Fact]
    public void SevDeskInvoice_ShouldBePartialRecord()
    {
        var invoice = new SevDeskInvoice { Id = 42, InvoiceNumber = "INV-001" };
        invoice.Id.ShouldBe(42);
        invoice.InvoiceNumber.ShouldBe("INV-001");
    }

    #endregion

    #region Test Helpers

    private sealed class NullLogger : ILogger<SevDeskApiKeyHandler>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private sealed class NoOpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }

    private sealed class CaptureRequestHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }

    #endregion
}
