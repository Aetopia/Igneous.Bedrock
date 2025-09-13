using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using static Windows.Win32.PInvoke;
using static System.Environment;
using static System.Environment.SpecialFolder;
using Windows.Win32.System.RemoteDesktop;
using System.IO;
using System;
using Igneous.System;

namespace Igneous.Core;

unsafe sealed partial class GDKGame : MinecraftGame
{
    internal GDKGame(string packageFamilyName, string applicationUserModelId, string path) : base(packageFamilyName, applicationUserModelId)
    {
        _path = Path.Combine(GetFolderPath(ApplicationData), path, "Users");
    }

    readonly string _path;
}

unsafe partial class GDKGame
{
    readonly struct GameInstance : IDisposable
    {
        internal readonly ProcessHandle Process;

        internal readonly WindowHandle Window;

        internal GameInstance(in ProcessHandle process, in WindowHandle window)
        {
            Window = window;
            Process = process;
        }

        public void Dispose() => Process.Dispose();

        public static implicit operator ProcessHandle(in GameInstance @this) => @this.Process;

        public static implicit operator WindowHandle(in GameInstance @this) => @this.Window;
    }
}

unsafe partial class GDKGame
{
    GameInstance? GetInstance()
    {
        fixed (char* @class = "Bedrock")
        fixed (char* string1 = _applicationUserModelId)
        {
            var window = HWND.Null;
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, HWND.Null, @class, null)) != HWND.Null)
            {
                uint processId = 0;
                GetWindowThreadProcessId(window, &processId);
                ProcessHandle process = new(processId);

                var error = GetApplicationUserModelId(process, &length, string2);
                if (error is not WIN32_ERROR.ERROR_SUCCESS) using (process) continue;

                var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) using (process) continue;

                return new(process, new(window));
            }

            return null;
        }
    }

    ProcessHandle LaunchBootstrapper()
    {
        fixed (char* string1 = _applicationUserModelId)
        {
            uint count = 0, level = 0;
            WTS_PROCESS_INFOW* processes = null;
            var server = HANDLE.WTS_CURRENT_SERVER_HANDLE;

            if (!WTSEnumerateProcessesEx(server, &level, WTS_CURRENT_SESSION, (PWSTR*)&processes, &count))
                return new(Activate());

            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            for (uint index = 0; index < count; index++)
            {
                var processId = processes[index].ProcessId;
                ProcessHandle process = new(processId);

                var error = GetApplicationUserModelId(process, &length, string2);
                if (error is not WIN32_ERROR.ERROR_SUCCESS) using (process) continue;

                var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) using (process) continue;

                return process;
            }

            return new(Activate());
        }
    }
}

unsafe partial class GDKGame
{
    internal override ProcessHandle? LaunchProcess()
    {
        if (GetInstance() is GameInstance instance)
        {
            instance.Window.SetForegroundWindow();
            return instance.Process;
        }

        using var bootstrapper = LaunchBootstrapper();
        bootstrapper.WaitForExit();

        if (GetInstance()?.Process is not ProcessHandle process)
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
            watcher.Deleted += (_, _) => @event.Set();

            var handles = stackalloc HANDLE[2] { process, @event };
            if (WaitForMultipleObjects(2, handles, false, INFINITE) > 0)
                return process;

            return null;
        }
    }
}

unsafe partial class GDKGame
{
    public override bool IsRunning
    {
        get
        {
            using var process = GetInstance()?.Process;
            return process is not null;
        }
    }

    public override uint? Launch()
    {
        if (LaunchProcess() is not ProcessHandle process) return null;
        using (process) return process.ProcessId;
    }

    public override void Terminate()
    {
        using var process = GetInstance()?.Process;
        process?.Terminate();
    }
}