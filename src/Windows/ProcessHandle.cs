using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Igneous.Windows;

unsafe readonly struct ProcessHandle : IDisposable
{
    readonly HANDLE _handle;

    readonly HWND _window;

    internal readonly uint ProcessId;

    internal bool Running(uint milliseconds) => WaitForSingleObject(_handle, milliseconds) is WAIT_TIMEOUT;

    internal void Wait() => WaitForSingleObject(_handle, INFINITE);

    internal void Terminate() => TerminateProcess(_handle, 0);

    internal void Foreground() => SetForegroundWindow(_window);

    internal ProcessHandle(HWND window)
    {
        uint processId = 0;
        GetWindowThreadProcessId(window, &processId);

        _window = window;
        ProcessId = processId;
        _handle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
    }

    internal ProcessHandle(uint processId)
    {
        ProcessId = processId;
        _handle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
    }

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in ProcessHandle @this) => @this._handle;
}