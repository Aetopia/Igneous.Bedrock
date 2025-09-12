using Igneous.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using static Windows.Win32.PInvoke;
using static System.Environment;
using static System.Environment.SpecialFolder;
using System.IO;
using System;

namespace Igneous.Core;

unsafe sealed class GDKGame : Game
{
    internal GDKGame(string packageFamilyName, string applicationUserModelId, string path) : base(packageFamilyName, applicationUserModelId)
    {
        _path = Path.Combine(GetFolderPath(ApplicationData), path, "Users");
    }

    readonly string _path;

    WindowHandle? FindWindow()
    {
        fixed (char* @class = "Bedrock")
        fixed (char* string1 = _applicationUserModelId)
        {
            WindowHandle window = HWND.Null;
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                using ProcessHandle process = window.OpenProcess();

                var error = GetApplicationUserModelId(process, &length, string2);
                if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                return window;
            }

            return null;
        }
    }

    public override bool Running => FindWindow() is not null;

    public override uint? Launch()
    {
        if (FindWindow() is WindowHandle window)
        {
            window.SetForeground();
            return window.ProcessId;
        }

        using ProcessHandle bootstrapper = new(Activate());
        bootstrapper.WaitForExit();

        if (FindWindow()?.OpenProcess() is not ProcessHandle process)
            return null;

        using (process)
        {
            Directory.CreateDirectory(_path);
            using FileSystemWatcher watcher = new(_path, "*resource_init_lock")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            using EventHandle @event = new();
            watcher.Deleted += (sender, args) => @event.Set();

            var handles = stackalloc HANDLE[2];
            handles[0] = (HANDLE)process; handles[1] = @event;

            if (WaitForMultipleObjects(2, handles, false, INFINITE) > 0)
                return process.ProcessId;

            return null;
        }
    }

    public override void Terminate()
    {
        if (FindWindow() is not WindowHandle window)
            return;

        using var process = window.OpenProcess();
        process.Terminate();
    }
}