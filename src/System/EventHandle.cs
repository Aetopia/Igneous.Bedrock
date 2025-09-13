using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
namespace Igneous.System;

sealed class EventHandle : IDisposable
{
    readonly HANDLE _handle;

    internal void Set() => SetEvent(_handle);

    internal EventHandle() => _handle = CreateEvent(null, true, false, null);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        CloseHandle(_handle);
    }

    public static implicit operator HANDLE(EventHandle @this) => @this._handle;

    ~EventHandle() => Dispose();
}