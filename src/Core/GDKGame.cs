using Igneous.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;
using static System.Environment;
using static System.Environment.SpecialFolder;
using System.IO;
using System.Threading;

namespace Igneous.Core;

unsafe sealed class GDKGame : Game
{
    internal GDKGame(string packageFamilyName, string applicationUserModelId, string path) : base(packageFamilyName, applicationUserModelId)
    {
        _path = Path.Combine(GetFolderPath(ApplicationData), path, "Users");
    }

    readonly string _path;

    ProcessHandle? Process
    {
        get
        {
            fixed (char* @class = "Bedrock")
            fixed (char* string1 = _applicationUserModelId)
            {
                var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var string2 = stackalloc char[(int)length];

                HWND window = HWND.Null; while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    ProcessHandle process = new(window);

                    var error = GetApplicationUserModelId(process, &length, string2);
                    if (error is not WIN32_ERROR.ERROR_SUCCESS)
                        using (process) continue;

                    var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                    if (result is not COMPARESTRING_RESULT.CSTR_EQUAL)
                        using (process) continue;

                    return process;
                }

                return null;
            }
        }
    }

    public override bool Running
    {
        get
        {
            using var process = Process;
            return process is not null;
        }
    }

    public override uint? Launch()
    {
        using var game = Process;
        if (game is not null) { game?.Foreground(); return game?.ProcessId; }

        using ProcessHandle bootstrapper = new(Activate()); bootstrapper.Wait();
        using var process = Process; if (process is null) return null;

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
            return process?.ProcessId;

        return null;
    }

    public override void Terminate()
    {
        using var process = Process;
        process?.Terminate();
    }
}