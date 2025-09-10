using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Igneous.Windows;

readonly struct ProcessHandle : IDisposable
{
    readonly HANDLE _handle;

    internal bool Running(uint milliseconds) => WaitForSingleObject(_handle, milliseconds) is WAIT_TIMEOUT;

    internal bool Wait() => WaitForSingleObject(_handle, INFINITE) is WAIT_OBJECT_0;

    internal static ProcessHandle Open(uint processId) => new(processId);

    ProcessHandle(uint processId) => _handle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in ProcessHandle @this) => @this._handle;
}