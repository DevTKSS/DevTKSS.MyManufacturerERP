using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;

namespace DevTKSS.MyManufacturerERP.Services.Auth;


public static class Utilities
{
    [Obsolete("Use MainWindow.Activate() instead.")]
    public static void ShowWindowForeground(Window window)
    {
        const int SW_RESTORE = 9;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        _ = ShowWindow(hwnd, SW_RESTORE);
        _ = SetForegroundWindow(hwnd);
    }
    [Obsolete("Uno.PlatformHelper.IsPackaged() instead.")]
    public static bool IsPackaged()
    {
        try
        {
            _ = Package.Current.Id.Name;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}