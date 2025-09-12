using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Igneous.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Igneous.Modding;

unsafe sealed class InjectionSession : IDisposable
{
    readonly ProcessHandle _process;

    readonly RemoteThreadHandle _thread;

    static readonly PAPCFUNC _function;

    static readonly LPTHREAD_START_ROUTINE _routine;

    readonly List<nint> _addresses = [];

    static InjectionSession()
    {
        var module = GetModuleHandle("Kernel32");
        var procedure = GetProcAddress(module, "LoadLibraryW");

        _function = Marshal.GetDelegateForFunctionPointer<PAPCFUNC>(procedure);
        _routine = Marshal.GetDelegateForFunctionPointer<LPTHREAD_START_ROUTINE>(procedure);
    }

    internal InjectionSession(in ProcessHandle process)
    {
        _process = process;
        _thread = new(process, _routine);
    }

    public void AddLibrary(PCWSTR filename)
    {
            const PAGE_PROTECTION_FLAGS flags = PAGE_PROTECTION_FLAGS.PAGE_READWRITE;
            const VIRTUAL_ALLOCATION_TYPE type = VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE;

            var size = (nuint)(filename.Length + 1) * sizeof(char);
            var address = VirtualAllocEx(_process, null, size, type, flags);

            WriteProcessMemory(_process, address, (void*)filename, size, null);
            QueueUserAPC(_function, _thread, (nuint)address);

            _addresses.Add((nint)address);
    }

    public void InjectLibraries() => _thread.WaitForExit();

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var address in _addresses)
            VirtualFreeEx(_process, (void*)address, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);

        _thread.Dispose();
    }
}