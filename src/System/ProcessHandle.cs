using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Igneous.System;

unsafe readonly struct ProcessHandle : IDisposable
{
    readonly HANDLE _handle = HANDLE.INVALID_HANDLE_VALUE;

    internal readonly uint ProcessId;

    internal bool IsRunning(uint milliseconds) => WaitForSingleObject(_handle, milliseconds) is WAIT_TIMEOUT;

    internal void WaitForExit() => WaitForSingleObject(_handle, INFINITE);

    internal void Terminate()
    {
        TerminateProcess(_handle, 0);
        WaitForExit();
    }

    internal ProcessHandle(uint processId)
    {
        ProcessId = processId;
        _handle = OpenProcess(PROCESS_ALL_ACCESS, false, processId); ;
    }

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in ProcessHandle @this) => @this._handle;
}