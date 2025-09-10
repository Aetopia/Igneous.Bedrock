using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Storage.FileSystem.FILE_SHARE_MODE;
using static Windows.Win32.Storage.FileSystem.FILE_CREATION_DISPOSITION;
using static Windows.Win32.Storage.FileSystem.FILE_INFO_BY_HANDLE_CLASS;
using Windows.Win32.Storage.FileSystem;

namespace Igneous.Windows;

readonly struct FileHandle : IDisposable
{
    readonly HANDLE _handle;

    internal unsafe static FileHandle? Open(PCWSTR path)
    {
        var handle = CreateFile2(path, 0, FILE_SHARE_DELETE, OPEN_EXISTING, null);
        return handle != HANDLE.INVALID_HANDLE_VALUE ? new(handle) : null;
    }

    FileHandle(in HANDLE handle) => _handle = handle;

    internal unsafe bool Deleted
    {
        get
        {
            FILE_STANDARD_INFO info = new();
            var size = (uint)sizeof(FILE_STANDARD_INFO);
            return GetFileInformationByHandleEx(_handle, FileStandardInfo, &info, size) && info.DeletePending;
        }
    }

    public void Dispose() => CloseHandle(_handle);
}