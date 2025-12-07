using System.Diagnostics;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;

namespace DevTKSS.MyManufacturerERP.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WebViewBrowserPage : Page
{
    public WebViewBrowserPage()
    {
        InitializeComponent();

    }
    // private Window? webViewWindow;
    //private async void OpenWindow_Click(object sender, RoutedEventArgs e)
    //{
    //    if (webViewWindow is null) webViewWindow = new Window();
    //    webViewWindow.Title = "Authentication...";
    //    webViewWindow.ExtendsContentIntoTitleBar = true;
        
    //    var webView = new WebView2()
    //    { 
    //        Source = new Uri(AddressBar.Text),
    //        HorizontalAlignment = HorizontalAlignment.Stretch,
    //        VerticalAlignment = VerticalAlignment.Stretch
    //    };
    //    await webView.EnsureCoreWebView2Async();
    //    webView.NavigationStarting += EnsureHttps;
    //    webView.NavigationCompleted += MyWebView_NavigationCompleted;
    //    webView.WebMessageReceived += (s, args) =>
    //    {
    //        Debug.WriteLine($"Message from webview: {args.TryGetWebMessageAsString()}");
    //    };
    //    webViewWindow.Content = webView;
    //    webViewWindow.Activate();
    //}
    //private void Minimize_Click(object sender, RoutedEventArgs e)
    //{
    //    var presenter = GetPresenter();
    //    presenter.Minimize();
    //}

    //private void Maximize_Click(object sender, RoutedEventArgs e)
    //{
    //    var presenter = GetPresenter();
    //    presenter.Maximize();
    //}
    //private OverlappedPresenter GetPresenter()
    //{
    //    if (webViewWindow?.AppWindow?.Presenter is OverlappedPresenter presenter)
    //    {
    //        return presenter;
    //    }
    //    throw new InvalidOperationException("AppWindow or its presenter is not available.");
    //}

    private void StatusUpdate(string message)
    {
        StatusBar.Text = message;
        Log.Debug(message);
    }
    private async void EnsureHttps(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        var uri = args.Uri ?? string.Empty;
        if (!uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            args.Cancel = true;
            await MyWebView.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
        }
        else
        {
            AddressBar.Text = uri;
        }
    }

    private bool TryCreateUri(string potentialUri, out Uri? result)
    {
       StatusUpdate("TryCreateUri");

        if ((Uri.TryCreate(potentialUri, UriKind.Absolute, out Uri? uri) || Uri.TryCreate("https://" + potentialUri, UriKind.Absolute, out uri)) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            result = uri;
            return true;
        }
        else
        {
            StatusUpdate("Unable to configure URI");
            result = null;
            return false;
        }
    }
    private void TryNavigate()
    {
        StatusUpdate("TryNavigate");

       
        if (TryCreateUri(AddressBar.Text, out Uri? destinationUri))
        {
            MyWebView.Source = destinationUri;
        }
        else
        {
            StatusUpdate("URI couldn't be figured out use it as a google search term");

            string bingString = $"https://www.google.com/search?q={Uri.EscapeDataString(AddressBar.Text)}";
            if (TryCreateUri(bingString, out destinationUri!))
            {
                AddressBar.Text = destinationUri.AbsoluteUri;
                MyWebView.Source = destinationUri;
            }
            else
            {
                StatusUpdate("URI couldn't be configured as google search term, giving up");
            }
        }
    }
    private void NavigateButton_Click(object sender, RoutedEventArgs e)
    {
        StatusUpdate("NavigateButton_OnClick: " + AddressBar.Text);

        TryNavigate();
    }

    private void MyWebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (!args.IsSuccess)
        {
            StatusUpdate($"Navigation to {sender.Source} failed with error code {args.WebErrorStatus} and HttpStatusCode {args.HttpStatusCode}.");
        }
        
    }

    private void AddressBar_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            StatusUpdate("AddressBar_KeyDown [Enter]: " + AddressBar.Text);

            e.Handled = true;
            TryNavigate();
        }
    }

}