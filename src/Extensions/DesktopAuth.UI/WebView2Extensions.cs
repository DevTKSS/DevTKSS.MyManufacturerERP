using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace DevTKSS.Extensions.Uno.Authentication.Desktop.UI;

public static partial class WebView2Extensions
{
    #region IsNavigating Attached Property
    public static readonly DependencyProperty IsNavigatingProperty =
        DependencyProperty.RegisterAttached(
            "IsNavigating",
            typeof(bool),
            typeof(WebView2Extensions),
            new PropertyMetadata(default(bool),OnIsNavigatingPropertyChanged));

    [DynamicDependency(nameof(SetIsNavigating))]
    public static bool GetIsNavigating(DependencyObject obj) => (bool)obj.GetValue(IsNavigatingProperty);

    [DynamicDependency(nameof(GetIsNavigating))]
    private static void SetIsNavigating(DependencyObject obj, bool value) => obj.SetValue(IsNavigatingProperty, value);

    private static void OnIsNavigatingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not WebView2 control) throw new InvalidOperationException("The attached property 'IsNavigating' can only be applied to a WebView2 control.");

        if (e.OldValue is { })
        {
            control.NavigationStarting -= OnNavigationStarting;
            control.NavigationCompleted -= OnNavigationCompleted;
        }
        if (e.NewValue is { })
        {
            control.NavigationStarting += OnNavigationStarting;
            control.NavigationCompleted += OnNavigationCompleted;
        }
    }
    #endregion

    #region DocumentTitle Attached Property
    public static DependencyProperty DocumentTitleProperty { [DynamicDependency(nameof(GetNavigationCompletedCommand))] get; } = DependencyProperty.RegisterAttached(
            "DocumentTitle",
            typeof(string),
            typeof(WebView2Extensions),
            new PropertyMetadata(string.Empty, OnDocumentTitlePropertyChanged));

    [DynamicDependency(nameof(SetDocumentTitle))]
    public static string GetDocumentTitle(DependencyObject obj) => (string)obj.GetValue(DocumentTitleProperty);

    [DynamicDependency(nameof(GetDocumentTitle))]
    private static void SetDocumentTitle(DependencyObject obj, string value) => obj.SetValue(DocumentTitleProperty, value);

    private static async void OnDocumentTitlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not WebView2 control) throw new InvalidOperationException("The attached property 'DocumentTitle' can only be applied to a WebView2 control.");

        if (control.CoreWebView2 is null)
        {
            await control.EnsureCoreWebView2Async();
        }

        if (e.OldValue is { })
        {
            control.CoreWebView2Initialized -= OnCoreWebView2Initialized;
            if (control.CoreWebView2 is CoreWebView2 cwv)
            {
                cwv.DocumentTitleChanged -= (sender, args) =>
                {
                    // Do nothing
                    // we can not use a real eventhandler, because WebView2.CoreWebView2.DocumentTitleChanged does not provide the WebView2 instance as sender and we can not pass it as state object.
                };
            }
        }
        if (e.NewValue is { })
        {
            control.CoreWebView2Initialized += OnCoreWebView2Initialized;
            if (control.CoreWebView2 is CoreWebView2 cwv)
            {
                cwv.DocumentTitleChanged += (sender, args) =>
                {
                    SetDocumentTitle(control, cwv.DocumentTitle ?? string.Empty);
                };
            }
        }
    }
    #endregion

    #region DependencyProperty: NavigatedCommand

    public static DependencyProperty NavigationCompletedCommandProperty { [DynamicDependency(nameof(GetNavigationCompletedCommand))] get; } = DependencyProperty.RegisterAttached(
        "NavigatedCommand",
        typeof(ICommand),
        typeof(WebView2Extensions),
        new PropertyMetadata(default(ICommand), OnNavigationCompletedCommandChanged));

    [DynamicDependency(nameof(SetNavigationCompletedCommand))]
    public static ICommand GetNavigationCompletedCommand(DependencyObject obj) => (ICommand)obj.GetValue(NavigationCompletedCommandProperty);
    [DynamicDependency(nameof(GetNavigationCompletedCommand))]
    public static void SetNavigationCompletedCommand(DependencyObject obj, ICommand value) => obj.SetValue(NavigationCompletedCommandProperty, value);

    private static void OnNavigationCompletedCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not WebView2 control) throw new InvalidOperationException("The attached property 'NavigatedCommand' can only be applied to a WebView2 control.");

        if (e.OldValue is { }) control.NavigationCompleted -= OnNavigationCompleted;
        if (e.NewValue is { }) control.NavigationCompleted += OnNavigationCompleted;
    }
    #endregion

    #region DependencyProperty: NavigationStartingCommand
    public static DependencyProperty NavigationStartingCommandProperty { [DynamicDependency(nameof(GetNavigationStartingCommand))] get; } = DependencyProperty.RegisterAttached(
        "NavigationStartingCommand",
        typeof(ICommand),
        typeof(WebView2Extensions),
        new PropertyMetadata(default(ICommand), OnNavigationStartingCommandChanged));

    [DynamicDependency(nameof(SetNavigationStartingCommand))]
    public static ICommand GetNavigationStartingCommand(DependencyObject obj) => (ICommand)obj.GetValue(NavigationStartingCommandProperty);

    [DynamicDependency(nameof(GetNavigationStartingCommand))]
    public static void SetNavigationStartingCommand(DependencyObject obj, ICommand value) => obj.SetValue(NavigationStartingCommandProperty, value);

    private static void OnNavigationStartingCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not WebView2 control) throw new InvalidOperationException("The attached property 'NavigationStartingCommand' can only be applied to a WebView2 control.");

        if (e.OldValue is { }) control.NavigationStarting -= OnNavigationStarting;
        if (e.NewValue is { }) control.NavigationStarting += OnNavigationStarting;
    }
    #endregion

    #region EventHandlers
    private static void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    { 
        var command = GetNavigationCompletedCommand(sender);
        if (command?.CanExecute(args) == true)
        {
            command.Execute(args);
        }
       SetIsNavigating(sender, false);
    }

    private static void OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        var command = GetNavigationStartingCommand(sender);
        if (command?.CanExecute(args) == true)
        {
            command.Execute(args);
        }
        SetIsNavigating(sender, true);
    }

    private static void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs e)
    {
        if (sender.CoreWebView2 is not null)
        {
            SetDocumentTitle(sender, sender.CoreWebView2.DocumentTitle ?? string.Empty);
        }
    }
    #endregion
}
