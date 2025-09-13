using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Igneous.Core;

/// <summary>
/// Provides services for interacting with Minecraft: Bedrock Edition.
/// </summary>

public unsafe abstract class Game
{
    internal Game(string packageFamilyName, string applicationUserModelId)
    {
        _packageFamilyName = packageFamilyName;
        _applicationUserModelId = applicationUserModelId;
    }

    static readonly IApplicationActivationManager _applicationActivationManager;

    private protected static readonly IPackageDebugSettings _packageDebugSettings;

    static Game()
    {
        PackageDebugSettings packageDebugSettings = new();
        ApplicationActivationManager applicationActivationManager = new();

        _packageDebugSettings = (IPackageDebugSettings)packageDebugSettings;
        _applicationActivationManager = (IApplicationActivationManager)applicationActivationManager;
    }

    private protected readonly string _packageFamilyName, _applicationUserModelId;

    private protected uint Activate()
    {
        fixed (char* applicationUserModelId = _applicationUserModelId)
        {
            _applicationActivationManager.ActivateApplication(applicationUserModelId, null, ACTIVATEOPTIONS.AO_NOERRORUI, out uint processId);
            return processId;
        }
    }

    private protected void GetPackageFullName(char* packageFullName, ref uint length)
    {
        uint count = 1; PWSTR packageFullNames = new();
        GetPackagesByPackageFamily(_packageFamilyName, ref count, &packageFullNames, ref length, packageFullName);
    }

    /// <summary>
    /// Check if Minecraft: Bedrock Edition is installed.
    /// </summary>

    public bool Installed
    {
        get
        {
            uint count = 0, length = 0;
            var error = GetPackagesByPackageFamily(_packageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER && count > 0;
        }
    }

    /// <summary>
    /// Unbounds Minecraft: Bedrock Edition from the Process Lifecycle Manager.
    /// </summary>

    public bool Unbounded
    {
        set
        {
            uint length = PACKAGE_FULL_NAME_MAX_LENGTH + 1;
            var packageFullName = stackalloc char[(int)length];

            GetPackageFullName(packageFullName, ref length);
            if (value) _packageDebugSettings.EnableDebugging(packageFullName, null, null);
            else _packageDebugSettings.DisableDebugging(packageFullName);
        }
    }

    /// <summary>
    /// Launches Minecraft: Bedrock Edition.
    /// </summary>

    /// <returns>
    /// The process identifer of the game.
    /// </returns>

    public abstract uint? Launch();

    /// <summary>
    /// Terminates Minecraft: Bedrock Edition.
    /// </summary>

    public abstract void Terminate();

    /// <summary>
    /// Check if Minecraft: Bedrock Edition is running.
    /// </summary>

    public abstract bool Running { get; }
}