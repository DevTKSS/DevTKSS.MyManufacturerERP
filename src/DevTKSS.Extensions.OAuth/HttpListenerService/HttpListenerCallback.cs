namespace DevTKSS.Extensions.OAuth.HttpListenerService;

public record HttpListenerCallback
{
    private readonly HttpListenerContext _context;

    public HttpListenerCallback(HttpListenerContext context)
    {
        _context = context;
    }

    public HttpListenerRequest Request => _context.Request;
    internal bool IsResponseSet { get; private set; } = false;

    public async Task SetResponseAsync(
        string content,
        string? contentType = null,
        int statusCode = 200,
        CancellationToken cancellationToken = default)
    {
        var response = _context.Response;
        response.StatusCode = statusCode;

        response.ContentType = contentType ?? System.Net.Mime.MediaTypeNames.Text.Html;
        response.ContentEncoding = Encoding.UTF8;

        var buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;

        await using var responseOutput = response.OutputStream;
        await responseOutput.WriteAsync(buffer, cancellationToken);
        await responseOutput.FlushAsync(cancellationToken);
        IsResponseSet = true;
    }

    public static string BuildHtmlResponse(
        string title,
        string message,
        string? redirectUrl = null,
        int? redirectSeconds = null)
    {
        var metaRefresh = redirectUrl != null && redirectSeconds.HasValue
            ? $"<meta http-equiv='refresh' content='{redirectSeconds};url={redirectUrl}'>"
            : string.Empty;
        return $"""
<html>
  <head>
    {metaRefresh}
    <title>{WebUtility.HtmlEncode(title)}</title>
  </head>
  <body>
    <h2>{WebUtility.HtmlEncode(title)}</h2>
    <p>{WebUtility.HtmlEncode(message)}</p>
  </body>
</html>
""";
    }
}