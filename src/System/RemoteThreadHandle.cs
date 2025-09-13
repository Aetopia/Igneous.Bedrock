using System;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Igneous.System;

unsafe sealed class RemoteThreadHandle : IDisposable
{
    readonly HANDLE _handle;

    internal RemoteThreadHandle(in ProcessHandle process, LPTHREAD_START_ROUTINE routine)
    {
        _handle = CreateRemoteThread(process, null, 0, routine, null, (uint)PROCESS_CREATION_FLAGS.CREATE_SUSPENDED, null);
    }

    public void WaitForExit()
    {
        ResumeThread(_handle);
        WaitForSingleObject(_handle, INFINITE);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        TerminateThread(_handle, 0);
        CloseHandle(_handle);
    }

    public static implicit operator HANDLE(RemoteThreadHandle @this) => @this._handle; 

    ~RemoteThreadHandle() => Dispose();
}