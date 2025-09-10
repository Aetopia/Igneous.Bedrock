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

    public bool Installed => false;

    public bool Running => false;

    public uint? Launch()
    {
        fixed (char* appUserModelId = _applicationUserModelId)
        {
            _applicationActivationManager.ActivateApplication(appUserModelId, null, ACTIVATEOPTIONS.AO_NOERRORUI, out var processId);
            using (var process = ProcessHandle.Open(processId)) if (!process.Wait()) return null;

            fixed (char* @class = Class)
            {
                var applicationUserModelIdLength = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var applicationUserModelId = stackalloc char[(int)applicationUserModelIdLength];

                HWND window = HWND.Null; while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    GetWindowThreadProcessId(window, &processId);
                    using var process = ProcessHandle.Open(processId);

                    var error = GetApplicationUserModelId(process, &applicationUserModelIdLength, applicationUserModelId);
                    if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                    var result = CompareStringOrdinal(appUserModelId, -1, applicationUserModelId, -1, true);
                    if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                    return processId;
                }
            }
        }

        return null;
    }

    public void Terminate()
    {

    }
}