using Igneous.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Igneous.Core;

unsafe sealed class GDKGame : Game
{
    internal GDKGame(string packageFamilyName, string applicationUserModelId) : base(packageFamilyName, applicationUserModelId) { }

    ProcessHandle? GetProcess()
    {
        fixed (char* @class = "Bedrock")
        fixed (char* string1 = _applicationUserModelId)
        {
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            HWND window = HWND.Null; while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                uint processId = 0;
                GetWindowThreadProcessId(window, &processId);

                ProcessHandle process = new(processId);
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

    public override bool Running
    {
        get
        {
            using var process = GetProcess();
            return process is not null;
        }
    }

    public override uint? Launch()
    {
        using (ProcessHandle process = new(Activate())) process.Wait();
        using (var process = GetProcess()) return process?.ProcessId;
    }

    public override void Terminate()
    {
        using var process = GetProcess();
        process?.Terminate();
    }
}