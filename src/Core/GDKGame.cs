using Igneous.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Igneous.Core;

unsafe sealed class GDKGame : IGame
{
    const string Class = "Bedrock";

    static readonly IApplicationActivationManager _applicationActivationManager;

    static GDKGame()
    {
        ApplicationActivationManager applicationActivationManager = new();
        _applicationActivationManager = (IApplicationActivationManager)applicationActivationManager;
    }

    internal GDKGame(string packageFamilyName, string applicationUserModelId)
    {
        _packageFamilyName = packageFamilyName;
        _applicationUserModelId = applicationUserModelId;
    }

    readonly string _packageFamilyName, _applicationUserModelId;

    ProcessHandle? Process
    {
        get
        {
            fixed (char* @class = "Bedrock") fixed (char* string1 = _applicationUserModelId)
            {
                var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var string2 = stackalloc char[(int)length];

                HWND window = HWND.Null; while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    uint processId = 0; GetWindowThreadProcessId(window, &processId);
                    var process = ProcessHandle.Open(processId);

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

    public bool Installed
    {
        get
        {
            uint count = 0, length = 0;
            var error = GetPackagesByPackageFamily(_packageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER && count > 0;
        }
    }

    public bool Running
    {
        get
        {
            using var process = Process;
            return process is not null;
        }
    }

    public uint? Launch()
    {
        fixed (char* appUserModelId = _applicationUserModelId)
        {
            _applicationActivationManager.ActivateApplication(appUserModelId, null, ACTIVATEOPTIONS.AO_NOERRORUI, out var processId);

            using (var process = ProcessHandle.Open(processId))
                if (!process.Wait()) return null;

            using (var process = Process)
                return process?.ProcessId;
        }
    }

    public void Terminate()
    {
        fixed (char* @class = Class)
        fixed (char* appUserModelId = _applicationUserModelId)
        {
            var applicationUserModelIdLength = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var applicationUserModelId = stackalloc char[(int)applicationUserModelIdLength];

            HWND window = HWND.Null; while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                uint processId = 0; GetWindowThreadProcessId(window, &processId);
                using var process = ProcessHandle.Open(processId);

                var error = GetApplicationUserModelId(process, &applicationUserModelIdLength, applicationUserModelId);
                if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                var result = CompareStringOrdinal(appUserModelId, -1, applicationUserModelId, -1, true);
                if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                process.Terminate();
            }
        }
    }
}