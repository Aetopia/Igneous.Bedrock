using Windows.UI.Xaml;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Igneous.System;

readonly struct WindowHandle
{
    readonly HWND _handle = HWND.Null;

    internal WindowHandle(HWND handle) => _handle = handle;

    internal void SetForegroundWindow() => PInvoke.SetForegroundWindow(_handle);
}