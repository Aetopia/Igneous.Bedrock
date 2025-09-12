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

unsafe partial class UWPGame :Game
{
    internal UWPGame(string packageFamilyName, string applicationUserModelId) : base(packageFamilyName, applicationUserModelId)
    {
        _path = string.Format(Format, GetFolderPath(LocalApplicationData), packageFamilyName);
    }

    const string Format = @"{0}\Packages\{1}\LocalState\games\com.mojang\minecraftpe\resource_init_lock";

    readonly string _path;
}

unsafe partial class UWPGame
{
    public override bool Running
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
                    using ProcessHandle process = new(processId);

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
    void GetPackageFullName(char* packageFullName, ref uint length)
    {
        uint count = 1; PWSTR packageFullNames = new();
        GetPackagesByPackageFamily(_packageFamilyName, ref count, &packageFullNames, ref length, packageFullName);
    }

    public override uint? Launch()
    {
        fixed (char* path = _path)
        {
            var file = FileHandle.Open(path); try
            {
                if (!Running || file is not null)
                {
                    var processId = Activate();
                    using ProcessHandle process = new(processId);

                    while (process.IsRunning(1))
                        if (file is null) file = FileHandle.Open(path);
                        else if (((FileHandle)file).Deleted) return processId;

                    return null;
                }
                return Activate();
            }
            finally { file?.Dispose(); }
        }
    }

    public override void Terminate()
    {
        uint length = PACKAGE_FULL_NAME_MAX_LENGTH + 1;
        var packageFullName = stackalloc char[(int)length];

        GetPackageFullName(packageFullName, ref length);
        _packageDebugSettings.TerminateAllProcesses(packageFullName);
    }
}