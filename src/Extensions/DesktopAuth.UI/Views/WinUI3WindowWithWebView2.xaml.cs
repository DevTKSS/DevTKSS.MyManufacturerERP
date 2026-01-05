using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace DevTKSS.Extensions.Uno.Authentication.Desktop.UI;

/// <summary>
/// Desktop authentication window hosting a WebView2.
/// </summary>
public sealed partial class WinUI3WindowWithWebView2 : Window
{

    public WinUI3WindowWithWebView2()
    {

        this.InitializeComponent();

        ConfigureWindow();
        SetTitle();
    }

    private async void ConfigureWindow()
    {
        await webView.EnsureCoreWebView2Async();
        webView.CoreWebView2Initialized += WebView2_CoreWebView2Initialized;
        AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);

        if (AppWindow.Presenter is OverlappedPresenter overlapped)
        {
            overlapped.Maximize();
        }
    }

    private void WebView2_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {

        if (webView.CoreWebView2 is not null)
        {
            webView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
        }

        SetTitle();
    }

    private void CoreWebView2_DocumentTitleChanged(object? sender, object e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Title = webView.CoreWebView2?.DocumentTitle ?? "Authentication";
        });
    }

    private void SetTitle()
    {
        Title = webView.CoreWebView2?.DocumentTitle ?? "Authenticating";
    }
}
