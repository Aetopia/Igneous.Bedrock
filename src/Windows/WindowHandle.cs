using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Igneous.Windows;

unsafe readonly struct WindowHandle
{
    internal WindowHandle(in HWND handle)
    {
        uint processId = 0; GetWindowThreadProcessId(handle, &processId);
        ProcessId = processId; _handle = handle;
    }

    internal readonly uint ProcessId;

    readonly HWND _handle = HWND.Null;

    internal void SetForeground() => SetForegroundWindow(_handle);

    internal ProcessHandle OpenProcess() => new(ProcessId);

    public static implicit operator HWND(in WindowHandle @this) => @this._handle;

    public static implicit operator WindowHandle(in HWND @this) => new(@this);
}