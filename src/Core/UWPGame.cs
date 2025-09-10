using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.UI.Shell.ACTIVATEOPTIONS;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using Igneous.Windows;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Igneous.Core;

partial class UWPGame : IGame
{
    static readonly IPackageDebugSettings _packageDebugSettings;

    static readonly IApplicationActivationManager _applicationActivationManager;

    unsafe static UWPGame()
    {
        PackageDebugSettings packageDebugSettings = new();
        ApplicationActivationManager applicationActivationManager = new();

        _packageDebugSettings = (IPackageDebugSettings)packageDebugSettings;
        _applicationActivationManager = (IApplicationActivationManager)applicationActivationManager;
    }
}

unsafe partial class UWPGame
{
    internal UWPGame(string packageFamilyName, string applicationUserModelId)
    {
        _packageFamilyName = packageFamilyName;
        _applicationUserModelId = applicationUserModelId;
        _path = string.Format(Format, GetFolderPath(LocalApplicationData), packageFamilyName);
    }

    const string Format = @"{0}\Packages\{1}\LocalState\games\com.mojang\minecraftpe\resource_init_lock";

    readonly string _path, _packageFamilyName, _applicationUserModelId;
}

unsafe partial class UWPGame
{
    public bool Installed
    {
        get
        {
            uint count = 0, length = 0;
            var error = GetPackagesByPackageFamily(_packageFamilyName, ref count, null, ref length, null);
            return error is ERROR_INSUFFICIENT_BUFFER && count > 0;
        }
    }

    public bool Running
    {
        get
        {
            fixed (char* @class = "MSCTFIME UI")
            fixed (char* string1 = _applicationUserModelId)
            {
                var window = HWND.Null;
                var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var string2 = stackalloc char[(int)length];

                while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    uint processId = 0;
                    GetWindowThreadProcessId(window, &processId);
                    using var process = ProcessHandle.Open(processId);

                    var error = GetApplicationUserModelId(process, &length, string2);
                    if (error is not ERROR_SUCCESS) continue;

                    var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                    if (result is not CSTR_EQUAL) continue;

                    return true;
                }

                return false;
            }
        }
    }
}

unsafe partial class UWPGame
{
    uint Activate()
    {
        fixed (char* applicationUserModelId = _applicationUserModelId)
        {
            _applicationActivationManager.ActivateApplication(applicationUserModelId, null, AO_NOERRORUI, out uint processId);
            return processId;
        }
    }

    public uint? Launch()
    {
        fixed (char* path = _path)
        {
            var file = FileHandle.Open(path); try
            {
                if (!Running || file is not null)
                {
                    var processId = Activate();
                    var process = ProcessHandle.Open(Activate());

                    while (process.Running(1))
                        if (file is null) file = FileHandle.Open(path);
                        else if (((FileHandle)file).Deleted) return processId;

                    return null;
                }
                return Activate();
            }
            finally { file?.Dispose(); }
        }
    }

    public void Terminate()
    {
        uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
        PWSTR packageFullNames = new(), packageFullName = stackalloc char[(int)length];

        GetPackagesByPackageFamily(_packageFamilyName, ref count, &packageFullNames, ref length, packageFullName);
        _packageDebugSettings.TerminateAllProcesses(packageFullName);
    }
}